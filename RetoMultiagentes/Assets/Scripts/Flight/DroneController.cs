using UnityEngine;

[DisallowMultipleComponent]
[RequireComponent(typeof(Rigidbody))]
public class DroneController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Rigidbody rb;
    [SerializeField] private Transform centerOfMass;
    [SerializeField] private FlightProfile flightProfile; // ScriptableObject (Issue #8/#19)

    // --- Flags / Estado ---
    [Header("State")]
    [SerializeField] private bool armed = false;
    [SerializeField] private bool debugManualInput = true;
    private bool inFlight = false;

    // --- Consignas (se usarán en issues posteriores) ---
    [Header("Setpoints")]
    // [SerializeField, Tooltip("m/s")] private float targetForwardSpeed = 0f;
    [SerializeField, Tooltip("m")] private float targetAltitude = 1.5f;
    // [SerializeField, Tooltip("deg")] private float targetYawDeg = 0f;
    [SerializeField] private Vector3 targetPosition = Vector3.zero;
    [SerializeField] private bool hasTargetPosition = false;

    // --- Parámetros básicos (fallback si no hay FlightProfile) ---
    [Header("Fallback Params")]
    [SerializeField] private float massKg = 1.2f;
    [SerializeField] private float maxTiltDeg = 25f;
    [SerializeField] private float maxClimbRate = 3.0f;   // m/s
    [SerializeField] private float maxDescentRate = 2.0f; // m/s
    [SerializeField] private float lateralAccel = 8.0f;   // m/s^2
    [SerializeField] private float yawRateDeg = 90f;      // deg/s

    // --- Internos ---
    private float thrustCmd = 0f;     // N
    private Vector3 bodyAccelCmd = Vector3.zero; // m/s^2 en marco mundo
    private float lastGroundHeight = 0f;

    // --- Eventos (stubs para integrar luego) ---
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
        // Asegura que el centro de masa esté bien posicionado
        if (centerOfMass != null) rb.centerOfMass = rb.transform.InverseTransformPoint(centerOfMass.position);

        // Si hay FlightProfile, sobrescribir parámetros
        if (flightProfile != null)
        {
            massKg = flightProfile.massKg;
            maxTiltDeg = flightProfile.maxTiltDeg;
            maxClimbRate = flightProfile.maxClimbRate;
            maxDescentRate = flightProfile.maxDescentRate;
            lateralAccel = flightProfile.lateralAccel;
            yawRateDeg = flightProfile.yawRateDeg;
            rb.mass = massKg;
        }

        // Armar el dron automáticamente al iniciar
        Arm();

        // Inicializar thrust para sostener el dron
        float g = Physics.gravity.magnitude;
        thrustCmd = massKg * g;
    }

    void Update()
    {
        if (!armed) return;

        // Choose between manual control and automatic navigation
        if (hasTargetPosition && !debugManualInput)
        {
            // Automatic navigation mode
            HandleAutomaticNavigation();
        }
        else if (debugManualInput)
        {
            // Manual control mode (debug)
            HandleManualInput();
        }
    }

    private void HandleManualInput()
    {
        var inputH = HandleHorizontalInput();
        var inputV = HandleVerticalInput();
        var up = HandleUpwardInput();

        // Comandos simples de aceleración lateral y vertical
        Vector3 fwd = transform.forward * (inputV * lateralAccel);
        Vector3 right = transform.right * (inputH * lateralAccel);
        bodyAccelCmd = fwd + right;

        // Tracción vertical (N): sostener peso +/- empuje por input
        float g = Physics.gravity.magnitude;
        float hoverThrust = massKg * g;
        float climbCmd = Mathf.Clamp(up, -1f, 1f) * (up > 0 ? maxClimbRate : maxDescentRate);
        // Convertir rate deseado a delta de thrust aproximado
        float climbAccel = climbCmd; // m/s -> simplificado: 1:1 como aceleración
        thrustCmd = Mathf.Max(0f, hoverThrust + massKg * climbAccel);
    }

    private void HandleAutomaticNavigation()
    {
        if (!hasTargetPosition) return;

        Vector3 currentPos = transform.position;
        Vector3 direction = (targetPosition - currentPos).normalized;
        float distance = Vector3.Distance(currentPos, targetPosition);

        // Simple proportional control for horizontal movement
        float horizontalDistance = Vector3.Distance(
            new Vector3(currentPos.x, 0, currentPos.z), 
            new Vector3(targetPosition.x, 0, targetPosition.z)
        );
        
        if (horizontalDistance > 0.5f) // Dead zone
        {
            Vector3 horizontalDirection = new Vector3(direction.x, 0, direction.z).normalized;
            bodyAccelCmd = horizontalDirection * Mathf.Min(lateralAccel, horizontalDistance * 2f);
        }
        else
        {
            bodyAccelCmd = Vector3.zero;
        }

        // Simple altitude control
        float altitudeError = targetPosition.y - currentPos.y;
        float g = Physics.gravity.magnitude;
        float hoverThrust = massKg * g;
        
        if (Mathf.Abs(altitudeError) > 0.5f) // Dead zone
        {
            float climbCmd = Mathf.Clamp(altitudeError * 0.5f, -maxDescentRate, maxClimbRate);
            thrustCmd = Mathf.Max(0f, hoverThrust + massKg * climbCmd);
        }
        else
        {
            thrustCmd = hoverThrust;
        }
    }

    private float HandleHorizontalInput()
    {
        float inputH = 0f;
        if (Input.GetKey(KeyCode.A)) inputH -= 1f;
        if (Input.GetKey(KeyCode.D)) inputH += 1f;
        return inputH;
    }
    private float HandleVerticalInput()
    {
        float inputV = 0f;
        if (Input.GetKey(KeyCode.W)) inputV += 1f;
        if (Input.GetKey(KeyCode.S)) inputV -= 1f;
        return inputV;
    }

    private float HandleUpwardInput()
    {
        float up = 0f;
        if (Input.GetKey(KeyCode.Space)) up += 4f;
        if (Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.C)) up -= 1f;
        return up;
    }

    private void FixedUpdate()
    {
        if (!armed) return;

        // Aplicar empuje vertical
        Vector3 up = transform.up;
        rb.AddForce(up * thrustCmd, ForceMode.Force);

        // Aplicar “aceleración” lateral como fuerza
        rb.AddForce(bodyAccelCmd * massKg, ForceMode.Force);

        // Limitar inclinación
        ClampTilt();

        // Pequeño yaw manual para pruebas
        if (debugManualInput)
        {
            float yawInput = 0f;
            if (Input.GetKey(KeyCode.Q)) yawInput -= 1f;
            if (Input.GetKey(KeyCode.E)) yawInput += 1f;
            if (Mathf.Abs(yawInput) > 0.01f)
            {
                float yawDelta = yawRateDeg * yawInput * Mathf.Deg2Rad * Time.fixedDeltaTime;
                rb.MoveRotation(Quaternion.AngleAxis(yawDelta * Mathf.Rad2Deg, Vector3.up) * rb.rotation);
            }
        }
    }

    private void ClampTilt()
    {
        // Limita el ángulo de pitch/roll respecto a mundo-Y
        Vector3 fwd = Vector3.ProjectOnPlane(transform.forward, Vector3.up).normalized;
        Vector3 right = Vector3.ProjectOnPlane(transform.right, Vector3.up).normalized;

        float pitchDeg = Vector3.SignedAngle(fwd, transform.forward, transform.right);
        float rollDeg  = Vector3.SignedAngle(right, transform.right, transform.forward);

        if (Mathf.Abs(pitchDeg) > maxTiltDeg || Mathf.Abs(rollDeg) > maxTiltDeg)
        {
            // Corrección suave volviendo hacia proyección horizontal
            Quaternion target = Quaternion.LookRotation(fwd, Vector3.up);
            rb.MoveRotation(Quaternion.Slerp(rb.rotation, target, 0.05f));
        }
    }

    // --- API mínima para siguientes issues (stubs) ---
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
        // TODO: Issue #8 implementará control de altitud con PID
    }

    public void LandAt(Vector3 worldPos, float radius = 2f)
    {
        // TODO: Issue #15 implementará aproximación y aterrizaje
    }

    public void GoTo(Vector3 worldPos)
    {
        targetPosition = worldPos;
        hasTargetPosition = true;
        
        // Set target altitude from the world position
        targetAltitude = worldPos.y;
        
        Debug.Log($"[DroneController] Target set to: {worldPos}");
    }

    public void ClearTarget()
    {
        hasTargetPosition = false;
        targetPosition = Vector3.zero;
    }

    public Vector3 GetTargetPosition()
    {
        return targetPosition;
    }

    public bool HasTargetPosition()
    {
        return hasTargetPosition;
    }

    // --- Telemetría básica ---
    public float GetAltitudeAgl()
    {
        // Raycast al suelo para altura aproximada
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

#if UNITY_EDITOR
    private void OnDrawGizmosSelected()
    {
        // Visual debug
        Gizmos.color = Color.cyan;
        Gizmos.DrawLine(transform.position, transform.position + transform.up * 0.5f);
        Gizmos.color = Color.yellow;
        Gizmos.DrawRay(transform.position, Vector3.down * GetAltitudeAgl());
    }
#endif
}

// ScriptableObject sugerido para Issue #8/#19
[CreateAssetMenu(fileName = "FlightProfile", menuName = "MAV/FlightProfile")]
public class FlightProfile : ScriptableObject
{
    public float massKg = 1.2f;
    public float maxTiltDeg = 25f;
    public float maxClimbRate = 3.0f;
    public float maxDescentRate = 2.0f;
    public float lateralAccel = 8.0f;
    public float yawRateDeg = 90f;
}
