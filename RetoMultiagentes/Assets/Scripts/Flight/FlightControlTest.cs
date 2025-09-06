using UnityEngine;

/// <summary>
/// Test script for validating flight control PID performance according to Issue #8 criteria
/// RMS altitude error < 0.5m and stable velocity
/// </summary>
public class FlightControlTest : MonoBehaviour
{
    [Header("Test Configuration")]
    [SerializeField] private DroneController droneController;
    [SerializeField] private float testDuration = 60f; // seconds
    [SerializeField] private bool enableDebugLogging = true;
    [SerializeField] private float logInterval = 1f; // seconds

    [Header("Test Results")]
    [SerializeField] private float altitudeRMSError = 0f;
    [SerializeField] private float velocityStability = 0f;
    [SerializeField] private bool testPassed = false;

    private float testStartTime;
    private float lastLogTime;
    private bool testRunning = false;
    private float altitudeErrorSum = 0f;
    private int altitudeSamples = 0;
    private float velocityVarianceSum = 0f;
    private float lastVelocity = 0f;
    private int velocitySamples = 0;

    void Start()
    {
        if (droneController == null)
            droneController = FindObjectOfType<DroneController>();
        
        if (droneController != null)
        {
            StartTest();
        }
        else
        {
            Debug.LogError("[FlightControlTest] DroneController not found!");
        }
    }

    void Update()
    {
        if (!testRunning) return;

        float currentTime = Time.time;
        
        // Update test data
        UpdateTestMetrics();

        // Log progress at intervals
        if (enableDebugLogging && (currentTime - lastLogTime) >= logInterval)
        {
            LogTestProgress();
            lastLogTime = currentTime;
        }

        // Check if test duration completed
        if ((currentTime - testStartTime) >= testDuration)
        {
            CompleteTest();
        }
    }

    private void StartTest()
    {
        testStartTime = Time.time;
        lastLogTime = testStartTime;
        testRunning = true;
        altitudeErrorSum = 0f;
        altitudeSamples = 0;
        velocityVarianceSum = 0f;
        velocitySamples = 0;
        lastVelocity = 0f;

        Debug.Log($"[FlightControlTest] Starting {testDuration}s flight control test");
        
        // Set a target position to activate automatic navigation
        Vector3 testTarget = droneController.transform.position + Vector3.up * 20f;
        droneController.GoTo(testTarget);
    }

    private void UpdateTestMetrics()
    {
        if (droneController == null) return;

        // Altitude error measurement
        float altitudeError = Mathf.Abs(droneController.GetAltitudeError());
        altitudeErrorSum += altitudeError * altitudeError; // Sum of squares for RMS
        altitudeSamples++;

        // Velocity stability measurement
        Vector3 horizontalVelocity = droneController.GetVelocity();
        horizontalVelocity.y = 0; // Only horizontal component
        float currentVelocity = horizontalVelocity.magnitude;
        
        if (velocitySamples > 0)
        {
            float velocityChange = Mathf.Abs(currentVelocity - lastVelocity);
            velocityVarianceSum += velocityChange;
        }
        
        lastVelocity = currentVelocity;
        velocitySamples++;
    }

    private void LogTestProgress()
    {
        if (droneController == null) return;

        float altitudeError = droneController.GetAltitudeError();
        float velocityError = droneController.GetVelocityError();
        Vector3 velocity = droneController.GetVelocity();
        float altitude = droneController.transform.position.y;

        float currentRMS = CalculateCurrentRMS();
        
        Debug.Log($"[FlightControlTest] Time: {Time.time - testStartTime:F1}s | " +
                  $"Alt: {altitude:F2}m (error: {altitudeError:F2}m) | " +
                  $"Vel: {velocity.magnitude:F2}m/s (error: {velocityError:F2}m/s) | " +
                  $"RMS: {currentRMS:F3}m");
    }

    private void CompleteTest()
    {
        testRunning = false;
        
        // Calculate final metrics
        altitudeRMSError = CalculateCurrentRMS();
        velocityStability = velocitySamples > 0 ? velocityVarianceSum / velocitySamples : 0f;
        
        // Test criteria from Issue #8
        bool altitudeTestPassed = altitudeRMSError < 0.5f;
        bool velocityTestPassed = velocityStability < 2.0f; // Reasonable stability threshold
        testPassed = altitudeTestPassed && velocityTestPassed;

        // Log final results
        Debug.Log($"[FlightControlTest] Test completed after {testDuration}s");
        Debug.Log($"[FlightControlTest] Altitude RMS Error: {altitudeRMSError:F3}m (required: <0.5m) - {(altitudeTestPassed ? "PASS" : "FAIL")}");
        Debug.Log($"[FlightControlTest] Velocity Stability: {velocityStability:F3}m/s (required: stable) - {(velocityTestPassed ? "PASS" : "FAIL")}");
        Debug.Log($"[FlightControlTest] Overall Test Result: {(testPassed ? "PASS" : "FAIL")}");
    }

    private float CalculateCurrentRMS()
    {
        if (altitudeSamples == 0) return 0f;
        return Mathf.Sqrt(altitudeErrorSum / altitudeSamples);
    }

    public void RestartTest()
    {
        if (droneController != null)
        {
            droneController.ResetPIDControllers();
            StartTest();
        }
    }

#if UNITY_EDITOR
    private void OnDrawGizmosSelected()
    {
        if (droneController != null)
        {
            // Draw test status
            Gizmos.color = testPassed ? Color.green : Color.red;
            Gizmos.DrawWireSphere(droneController.transform.position, 1f);
        }
    }
#endif
}