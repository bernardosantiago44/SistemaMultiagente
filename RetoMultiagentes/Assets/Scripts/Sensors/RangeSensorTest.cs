using UnityEngine;

/// <summary>
/// Simple test script to demonstrate RangeSensor integration with DroneController
/// Attach this to a GameObject with DroneController and RangeSensor components
/// </summary>
public class RangeSensorTest : MonoBehaviour
{
    [Header("Components")]
    [SerializeField] private DroneController droneController;
    [SerializeField] private RangeSensor rangeSensor;
    
    [Header("Test Settings")]
    [SerializeField] private bool enableAutoTest = true;
    [SerializeField] private float testDuration = 30f;
    [SerializeField] private bool enableAltitudeHoldTest = true;
    [SerializeField] private float targetAltitude = 2.0f;
    
    // Test state
    private float testStartTime;
    private int totalReadings = 0;
    private float averageDistance = 0f;
    private float minDistance = float.MaxValue;
    private float maxDistance = 0f;
    
    private void Start()
    {
        // Auto-find components if not assigned
        if (droneController == null)
            droneController = GetComponent<DroneController>();
        if (rangeSensor == null)
            rangeSensor = GetComponent<RangeSensor>();
        
        // Validate required components
        if (droneController == null)
        {
            Debug.LogError("[RangeSensorTest] DroneController component not found!", this);
            enabled = false;
            return;
        }
        
        if (rangeSensor == null)
        {
            Debug.LogError("[RangeSensorTest] RangeSensor component not found!", this);
            enabled = false;
            return;
        }
        
        // Subscribe to range sensor events
        rangeSensor.OnRangeReading += OnRangeReadingReceived;
        
        if (enableAutoTest)
        {
            StartTest();
        }
    }
    
    private void OnDestroy()
    {
        // Unsubscribe from events
        if (rangeSensor != null)
            rangeSensor.OnRangeReading -= OnRangeReadingReceived;
    }
    
    private void StartTest()
    {
        Debug.Log("[RangeSensorTest] Starting range sensor test...");
        testStartTime = Time.time;
        
        // Arm the drone
        droneController.Arm();
        
        if (enableAltitudeHoldTest)
        {
            // Take off to test altitude
            droneController.TakeOff(targetAltitude);
            Debug.Log($"[RangeSensorTest] Taking off to {targetAltitude}m altitude");
        }
    }
    
    private void OnRangeReadingReceived(RangeReading reading)
    {
        totalReadings++;
        
        // Update statistics
        if (reading.hasHit)
        {
            averageDistance = ((averageDistance * (totalReadings - 1)) + reading.distance) / totalReadings;
            minDistance = Mathf.Min(minDistance, reading.distance);
            maxDistance = Mathf.Max(maxDistance, reading.distance);
        }
        
        // Log reading every 50 readings to avoid spam
        if (totalReadings % 50 == 0)
        {
            string status = reading.hasHit ? $"Distance: {reading.distance:F2}m" : "Out of range";
            Debug.Log($"[RangeSensorTest] Reading #{totalReadings}: {status} (Confidence: {reading.confidence:F2})");
        }
    }
    
    private void Update()
    {
        if (!enableAutoTest) return;
        
        // Check if test should end
        if (Time.time - testStartTime > testDuration)
        {
            EndTest();
            enableAutoTest = false;
        }
        
        // Display real-time information
        if (Time.time % 5f < 0.1f) // Every 5 seconds
        {
            DisplayTestInfo();
        }
    }
    
    private void DisplayTestInfo()
    {
        Debug.Log($"[RangeSensorTest] Status - Current Distance: {rangeSensor.CurrentDistance:F2}m, " +
                  $"Valid Reading: {rangeSensor.HasValidReading}, " +
                  $"Framerate: {rangeSensor.GetActualFramerate():F1}Hz, " +
                  $"Altitude AGL: {droneController.GetAltitudeAgl():F2}m");
    }
    
    private void EndTest()
    {
        Debug.Log("[RangeSensorTest] Test completed!");
        Debug.Log($"[RangeSensorTest] Total readings: {totalReadings}");
        Debug.Log($"[RangeSensorTest] Average distance: {averageDistance:F2}m");
        Debug.Log($"[RangeSensorTest] Min distance: {minDistance:F2}m");
        Debug.Log($"[RangeSensorTest] Max distance: {maxDistance:F2}m");
        Debug.Log($"[RangeSensorTest] Final sensor framerate: {rangeSensor.GetActualFramerate():F1}Hz");
        
        // Land the drone
        droneController.Disarm();
    }
    
    // Manual test methods for inspector buttons
    [ContextMenu("Test Single Reading")]
    public void TestSingleReading()
    {
        if (rangeSensor == null) return;
        
        Debug.Log($"[RangeSensorTest] Single reading - Distance: {rangeSensor.GetDistance():F2}m, " +
                  $"In Range: {rangeSensor.IsInRange()}, " +
                  $"Confidence: {(rangeSensor.LastReading?.confidence ?? 0f):F2}");
    }
    
    [ContextMenu("Reset Statistics")]
    public void ResetStatistics()
    {
        totalReadings = 0;
        averageDistance = 0f;
        minDistance = float.MaxValue;
        maxDistance = 0f;
        Debug.Log("[RangeSensorTest] Statistics reset");
    }
    
    [ContextMenu("Toggle Debug Gizmos")]
    public void ToggleDebugGizmos()
    {
        if (rangeSensor != null)
        {
            // Note: This would require making showDebugGizmos public in RangeSensor
            Debug.Log("[RangeSensorTest] Debug gizmos toggle requested (manual change required in RangeSensor inspector)");
        }
    }
    
    [ContextMenu("Validate Acceptance Criteria")]
    public void ValidateAcceptanceCriteria()
    {
        if (rangeSensor != null)
        {
            Debug.Log("[RangeSensorTest] Running acceptance criteria validation...");
            bool passed = rangeSensor.ValidateAcceptanceCriteria();
            
            if (passed)
            {
                Debug.Log("[RangeSensorTest] ✓ All acceptance criteria validated successfully!");
            }
            else
            {
                Debug.LogWarning("[RangeSensorTest] ⚠ Some acceptance criteria issues detected. Check logs above.");
            }
        }
        else
        {
            Debug.LogError("[RangeSensorTest] Cannot validate: RangeSensor component not found!");
        }
    }
    
    private void OnGUI()
    {
        if (!enableAutoTest) return;
        
        // Simple on-screen display
        GUI.Label(new Rect(10, 10, 300, 20), $"Range Sensor Test - Time: {Time.time - testStartTime:F1}s");
        GUI.Label(new Rect(10, 30, 300, 20), $"Current Distance: {rangeSensor.CurrentDistance:F2}m");
        GUI.Label(new Rect(10, 50, 300, 20), $"Valid Reading: {rangeSensor.HasValidReading}");
        GUI.Label(new Rect(10, 70, 300, 20), $"Total Readings: {totalReadings}");
        GUI.Label(new Rect(10, 90, 300, 20), $"Sensor Framerate: {rangeSensor.GetActualFramerate():F1}Hz");
        
        if (droneController != null)
        {
            GUI.Label(new Rect(10, 110, 300, 20), $"Drone Altitude AGL: {droneController.GetAltitudeAgl():F2}m");
            GUI.Label(new Rect(10, 130, 300, 20), $"Drone Armed: {droneController.IsArmed()}");
        }
    }
}