using UnityEngine;

[DisallowMultipleComponent]
[RequireComponent(typeof(Rigidbody))]
public class DroneController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Rigidbody rb;
    [SerializeField] private Transform centerOfMass;
    [SerializeField] private FlightProfile flightProfile; // ScriptableObject

    [Header("Sensors")]
    [SerializeField] private RangeSensor rangeSensor;
    [SerializeField] private bool useTerrainRelativeAltitude = false;

    [Header("State")]
    [SerializeField] private bool armed = false;
    [SerializeField] private bool debugManualInput = true;
    private bool inFlight = false;

    [Header("Setpoints")]
    [SerializeField, Tooltip("m")] private float targetAltitude = 1.5f;
    [SerializeField] private Vector3 targetPosition = Vector3.zero;
    [SerializeField] private bool hasTargetPosition = false;

    // --- Parámetros (fallback si no hay FlightProfile) ---
    [Header("Fallback Params")]
    [SerializeField] private float massKg = 1.2f;
    [SerializeField] private float maxTiltDeg = 25f;

    // OJO: estos ahora son **aceleraciones**, no "rates"
    [SerializeField, Tooltip("m/s^2")] private float maxClimbAccel = 5.0f;
    [SerializeField, Tooltip("m/s^2 (valor positivo)")] private float maxDescentAccel = 3.0f;
    [SerializeField, Tooltip("m/s^2")] private float lateralAccel = 8.0f;

    // Braking (m/s^2) y ganancias (1/s)
    [SerializeField, Tooltip("Límite de frenado vertical (m/s^2)")]
    private float verticalBrakeAccelMax = 8f;
    [SerializeField, Tooltip("Ganancia vertical de frenado (1/s)")]
    private float verticalBrakeK = 6f;

    [SerializeField, Tooltip("Límite de frenado horizontal (m/s^2)")]
    private float lateralBrakeAccelMax = 5f;
    [SerializeField, Tooltip("Ganancia horizontal de frenado (1/s)")]
    private float lateralBrakeK = 4f;

    // Umbrales para evitar microtemblores
    [SerializeField] private float vDeadband = 0.03f;   // m/s

    [SerializeField] private float yawRateDeg = 90f;

    // --- Internos ---
    private float thrustCmd = 0f;                 // N (Fuerza vertical total)
    private Vector3 bodyAccelCmd = Vector3.zero;  // m/s^2 (x,z en marco mundo)
    private float lastGroundHeight = 0f;

    // --- PID (si usas FlightProfile) ---
    private AltitudeHold altitudeController;
    private VelocityController velocityController;

    public System.Action OnArmed;
    public System.Action OnDisarmed;

    private void Reset()
    {
        rb = GetComponent<Rigidbody>();
    }

    void Awake()
    {
        if (rb == null) rb = GetComponent<Rigidbody>();
        rb.useGravity = true;
        rb.mass = massKg;

        // Centro de masa
        if (centerOfMass != null)
            rb.centerOfMass = rb.transform.InverseTransformPoint(centerOfMass.position);

        // Sobrescribe desde FlightProfile (si existe)
        if (flightProfile != null)
        {
            massKg = flightProfile.massKg;
            maxTiltDeg = flightProfile.maxTiltDeg;

            // Si tu ScriptableObject tiene rates, mapea a aceleraciones aquí si quieres.
            rb.mass = massKg;
        }

        // Ajustes de rigidez/suavizado
        rb.linearDamping = 0.2f;
        rb.angularDamping = 1.5f;

        Arm();

        // Fuerza para sostener peso (hover)
        float g = Physics.gravity.magnitude;
        thrustCmd = massKg * g;

        // Inicializa controladores si están disponibles
        if (flightProfile != null)
        {
            if (rangeSensor == null) rangeSensor = GetComponent<RangeSensor>();
            altitudeController = (rangeSensor != null && useTerrainRelativeAltitude)
                ? new AltitudeHold(flightProfile, rangeSensor)
                : new AltitudeHold(flightProfile);
            velocityController = new VelocityController(flightProfile);
        }
        else
        {
            Debug.LogWarning("[DroneController] FlightProfile is null - PID controllers not initialized");
        }
    }

    void Update()
    {
        print("Altitude AGL: " + GetAltitudeAgl().ToString("F2") + " m");
        if (!armed) return;

        if (hasTargetPosition && !debugManualInput)
            HandleAutomaticNavigation();
        else if (debugManualInput)
            HandleManualInput();
    }

    private void HandleManualInput()
    {
        // --- Lectura de teclas ---
        float inputH = 0f; if (Input.GetKey(KeyCode.A)) inputH -= 1f; if (Input.GetKey(KeyCode.D)) inputH += 1f;
        float inputV = 0f; if (Input.GetKey(KeyCode.W)) inputV += 1f; if (Input.GetKey(KeyCode.S)) inputV -= 1f;

        float up = 0f;
        if (Input.GetKey(KeyCode.Space)) up += 1f;
        if (Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.C)) up -= 1f;
        up = Mathf.Clamp(up, -1f, 1f);

        // --- Velocidades actuales ---
        Vector3 vel = rb.linearVelocity;
        float vY = vel.y;
        Vector3 vH = new Vector3(vel.x, 0f, vel.z);

        // --- Lateral ---
        // Si hay input -> aceleración mandada; si NO, frenado activo hacia 0
        if (Mathf.Abs(inputH) > 0.01f || Mathf.Abs(inputV) > 0.01f)
        {
            Vector3 fwd = transform.forward * (inputV * lateralAccel);
            Vector3 right = transform.right * (inputH * lateralAccel);
            bodyAccelCmd = fwd + right; // m/s^2
        }
        else
        {
            if (vH.sqrMagnitude > vDeadband * vDeadband)
            {
                // a_cmd = -K * v  (limitado)
                Vector3 brake = -vH * lateralBrakeK;
                bodyAccelCmd = Vector3.ClampMagnitude(brake, lateralBrakeAccelMax);
            }
            else
            {
                bodyAccelCmd = Vector3.zero;
            }
        }

        // --- Vertical ---
        float g = Physics.gravity.magnitude;
        float hoverThrust = massKg * g;
        float climbAccelCmd = 0f;

        if (up > 0f)
        {
            climbAccelCmd = up * maxClimbAccel;           // subir mientras mantienes
        }
        else if (up < 0f)
        {
            climbAccelCmd = up * maxDescentAccel;         // bajar mientras mantienes (up es negativo)
        }
        else
        {
            // Sin tecla: frenado activo a vY -> 0
            if (Mathf.Abs(vY) > vDeadband)
            {
                float aBrake = Mathf.Clamp(-verticalBrakeK * vY, -verticalBrakeAccelMax, verticalBrakeAccelMax);
                climbAccelCmd = aBrake;
            }
            else
            {
                climbAccelCmd = 0f; // ya está prácticamente parado
            }
        }

        // Fuerza total vertical (N) = hover + m * a_cmd
        thrustCmd = Mathf.Max(0f, hoverThrust + massKg * climbAccelCmd);
    }

    private void HandleAutomaticNavigation()
    {
        if (!hasTargetPosition) return;

        Vector3 currentPos = transform.position;
        Vector3 direction = (targetPosition - currentPos).normalized;

        // Horizontal
        float horizontalDistance = Vector3.Distance(
            new Vector3(currentPos.x, 0, currentPos.z),
            new Vector3(targetPosition.x, 0, targetPosition.z)
        );

        if (horizontalDistance > 0.5f)
        {
            Vector3 horizontalDirection = new Vector3(direction.x, 0, direction.z).normalized;
            Vector3 horizontalVelocity = new Vector3(rb.linearVelocity.x, 0, rb.linearVelocity.z);
            float currentSpeed = horizontalVelocity.magnitude;

            if (velocityController != null && flightProfile != null)
            {
                float forwardThrust = velocityController.GetForwardThrust(currentSpeed); // N
                // a = F/m
                bodyAccelCmd = horizontalDirection * (forwardThrust / massKg);
            }
            else
            {
                bodyAccelCmd = horizontalDirection * Mathf.Min(lateralAccel, horizontalDistance * 2f);
            }
        }
        else
        {
            bodyAccelCmd = Vector3.zero;
        }

        // Vertical
        float climbAccelCmd;
        float g = Physics.gravity.magnitude;
        float hoverThrust = massKg * g;

        if (altitudeController != null && flightProfile != null)
        {
            float up = 0f;
            if (flightProfile.targetAltitude > targetPosition.y)
            {
                flightProfile.targetAltitude = targetPosition.y;
                up = 1f; // fuerza a subir si hay cambio de setpoint
            }
            else if (flightProfile.targetAltitude < targetPosition.y)
            {
                flightProfile.targetAltitude = targetPosition.y;
                up = -1f; // fuerza a bajar si hay cambio de setpoint
            }

            float currentAltitude = transform.position.y;

            climbAccelCmd = up * maxClimbAccel;
            thrustCmd = Mathf.Max(0f, hoverThrust + massKg * climbAccelCmd);
        }
        else
        {
            float altitudeError = targetPosition.y - currentPos.y;

            if (Mathf.Abs(altitudeError) > 0.5f)
            {
                float climbAccel = Mathf.Clamp(altitudeError * 0.5f, -maxDescentAccel, maxClimbAccel);
                thrustCmd = Mathf.Max(0f, hoverThrust + massKg * climbAccel);
            }
            else
            {
                thrustCmd = hoverThrust;
            }
        }
    }

    private void FixedUpdate()
    {
        if (!armed) return;

        // Vertical: fuerza en N continua (compensa gravedad sin picos)
        rb.AddForce(Vector3.up * thrustCmd, ForceMode.Force);

        // Lateral: aceleración pura (m/s^2). Sin input -> 0 inmediatamente.
        rb.AddForce(bodyAccelCmd, ForceMode.Acceleration);

        ClampTilt();

        if (debugManualInput)
        {
            float yawInput = 0f;
            if (Input.GetKey(KeyCode.Q)) yawInput -= 1f;
            if (Input.GetKey(KeyCode.E)) yawInput += 1f;

            if (Mathf.Abs(yawInput) > 0.01f)
            {
                float yawDeltaDeg = yawRateDeg * yawInput * Time.fixedDeltaTime;
                rb.MoveRotation(Quaternion.AngleAxis(yawDeltaDeg, Vector3.up) * rb.rotation);
            }
        }
    }

    private void ClampTilt()
    {
        Vector3 fwd = Vector3.ProjectOnPlane(transform.forward, Vector3.up).normalized;
        Vector3 right = Vector3.ProjectOnPlane(transform.right, Vector3.up).normalized;

        float pitchDeg = Vector3.SignedAngle(fwd, transform.forward, transform.right);
        float rollDeg  = Vector3.SignedAngle(right, transform.right, transform.forward);

        if (Mathf.Abs(pitchDeg) > maxTiltDeg || Mathf.Abs(rollDeg) > maxTiltDeg)
        {
            Quaternion target = Quaternion.LookRotation(fwd, Vector3.up);
            rb.MoveRotation(Quaternion.Slerp(rb.rotation, target, 0.05f));
        }
    }

    // --- API mínima ---
    public void Arm()
    {
        armed = true;
        OnArmed?.Invoke();
    }

    public void Disarm()
    {
        armed = false;
        inFlight = false;
        thrustCmd = 0f;
        bodyAccelCmd = Vector3.zero;
        OnDisarmed?.Invoke();
    }

    public void TakeOff(float targetAltMeters = 1.5f)
    {
        targetAltitude = targetAltMeters;
        inFlight = true;

        if (flightProfile != null)
            flightProfile.targetAltitude = targetAltMeters;

        ResetPIDControllers();
    }

    public void LandAt(Vector3 worldPos, float radius = 2f) { /* TODO */ }

    public void GoTo(Vector3 worldPos)
    {
        targetPosition = worldPos;
        hasTargetPosition = true;
        targetAltitude = worldPos.y;

        if (flightProfile != null)
            flightProfile.targetAltitude = worldPos.y;

        Debug.Log($"[DroneController] Target set to: {worldPos}");
    }

    public void ClearTarget()
    {
        hasTargetPosition = false;
        targetPosition = Vector3.zero;
    }

    public Vector3 GetTargetPosition() => targetPosition;
    public bool HasTargetPosition() => hasTargetPosition;

    // --- Telemetría básica ---
    public float GetAltitudeAgl()
    {
        if (Physics.Raycast(transform.position, Vector3.down, out var hit, 500f))
        {
            lastGroundHeight = hit.point.y;
            return transform.position.y - lastGroundHeight;
        }
        return transform.position.y - lastGroundHeight;
    }

    public Vector3 GetVelocity() => rb.linearVelocity;
    public bool IsArmed() => armed;
    public bool InFlight() => inFlight;

    // --- PID telemetry ---
    public float GetAltitudeError()
    {
        if (altitudeController != null)
            return altitudeController.GetCurrentError(transform.position.y);
        return 0f;
    }

    public float GetVelocityError()
    {
        if (velocityController != null)
        {
            Vector3 horizontalVelocity = new Vector3(rb.linearVelocity.x, 0, rb.linearVelocity.z);
            return velocityController.GetCurrentError(horizontalVelocity.magnitude);
        }
        return 0f;
    }

    public void ResetPIDControllers()
    {
        if (altitudeController != null) altitudeController.Reset();
        if (velocityController != null) velocityController.Reset();
    }

    // --- Range Sensor integration ---
    public void SetRangeSensor(RangeSensor sensor)
    {
        rangeSensor = sensor;
        if (altitudeController != null) altitudeController.SetRangeSensor(sensor);
    }

    public void SetTerrainRelativeMode(bool enabled)
    {
        useTerrainRelativeAltitude = enabled;
        if (altitudeController != null) altitudeController.SetTerrainRelativeMode(enabled);
    }

    public RangeSensor GetRangeSensor() => rangeSensor;

    public bool IsUsingTerrainRelativeAltitude()
    {
        return altitudeController != null && altitudeController.IsUsingTerrainRelativeMode();
    }

    public float GetRangeSensorDistance()
    {
        if (rangeSensor != null) return rangeSensor.GetDistance();
        return -1f;
    }

#if UNITY_EDITOR
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawLine(transform.position, transform.position + transform.up * 0.5f);

        Gizmos.color = Color.yellow;
        Gizmos.DrawRay(transform.position, Vector3.down * GetAltitudeAgl());

        if (rangeSensor != null && Application.isPlaying)
        {
            float sensorDistance = rangeSensor.GetDistance();
            if (rangeSensor.IsInRange())
            {
                Gizmos.color = IsUsingTerrainRelativeAltitude() ? Color.green : Color.blue;
                Gizmos.DrawRay(transform.position, Vector3.down * sensorDistance);
                if (IsUsingTerrainRelativeAltitude())
                {
                    Gizmos.color = Color.green;
                    Gizmos.DrawWireCube(transform.position + Vector3.down * sensorDistance, Vector3.one * 0.2f);
                }
            }
        }
    }
#endif
}
