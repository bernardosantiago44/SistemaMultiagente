using UnityEngine;

/// <summary>
/// Test script to demonstrate Range Sensor integration with AltitudeHold system
/// Shows both absolute and terrain-relative altitude control modes
/// </summary>
public class TerrainFollowingTest : MonoBehaviour
{
    [Header("Components")]
    [SerializeField] private DroneController droneController;
    [SerializeField] private RangeSensor rangeSensor;
    
    [Header("Test Settings")]
    [SerializeField] private bool enableAutoTest = true;
    [SerializeField] private float testDuration = 60f;
    [SerializeField] private bool startWithTerrainRelativeMode = true;
    [SerializeField] private float targetAltitudeAboveGround = 3.0f;
    
    [Header("Navigation Test")]
    [SerializeField] private bool enableNavigationTest = true;
    [SerializeField] private Vector3[] waypoints = new Vector3[]
    {
        new Vector3(0, 5, 0),
        new Vector3(10, 5, 0),
        new Vector3(10, 5, 10),
        new Vector3(0, 5, 10)
    };
    [SerializeField] private float waypointReachDistance = 2f;
    
    // Test state
    private float testStartTime;
    private int currentWaypointIndex = 0;
    private bool testCompleted = false;
    private float modeToggleTimer = 0f;
    private float modeToggleInterval = 15f; // Switch modes every 15 seconds
    
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
            Debug.LogError("[TerrainFollowingTest] DroneController component not found!", this);
            enabled = false;
            return;
        }
        
        if (rangeSensor == null)
        {
            Debug.LogError("[TerrainFollowingTest] RangeSensor component not found!", this);
            enabled = false;
            return;
        }
        
        if (enableAutoTest)
        {
            StartTest();
        }
    }
    
    private void StartTest()
    {
        Debug.Log("[TerrainFollowingTest] Starting terrain-following test...");
        testStartTime = Time.time;
        modeToggleTimer = Time.time;
        
        // Configure the drone with the range sensor
        droneController.SetRangeSensor(rangeSensor);
        
        // Set initial mode
        droneController.SetTerrainRelativeMode(startWithTerrainRelativeMode);
        
        // Arm the drone
        droneController.Arm();
        
        // Take off to initial altitude
        droneController.TakeOff(targetAltitudeAboveGround);
        
        string mode = startWithTerrainRelativeMode ? "terrain-relative" : "absolute";
        Debug.Log($"[TerrainFollowingTest] Taking off to {targetAltitudeAboveGround}m in {mode} mode");
        
        if (enableNavigationTest && waypoints.Length > 0)
        {
            // Start navigation to first waypoint
            droneController.GoTo(waypoints[0]);
            Debug.Log($"[TerrainFollowingTest] Navigating to waypoint 0: {waypoints[0]}");
        }
    }
    
    private void Update()
    {
        if (!enableAutoTest || testCompleted) return;
        
        // Check if test should end
        if (Time.time - testStartTime > testDuration)
        {
            EndTest();
            return;
        }
        
        // Handle mode toggling
        if (Time.time - modeToggleTimer > modeToggleInterval)
        {
            ToggleAltitudeMode();
            modeToggleTimer = Time.time;
        }
        
        // Handle waypoint navigation
        if (enableNavigationTest)
        {
            HandleWaypointNavigation();
        }
        
        // Log status every few seconds
        if (Time.time % 3f < 0.1f)
        {
            LogCurrentStatus();
        }
    }
    
    private void ToggleAltitudeMode()
    {
        bool currentMode = droneController.IsUsingTerrainRelativeAltitude();
        bool newMode = !currentMode;
        
        droneController.SetTerrainRelativeMode(newMode);
        
        string modeText = newMode ? "terrain-relative" : "absolute";
        Debug.Log($"[TerrainFollowingTest] Switched to {modeText} altitude mode");
    }
    
    private void HandleWaypointNavigation()
    {
        if (waypoints.Length == 0) return;
        
        Vector3 currentPos = transform.position;
        Vector3 targetWaypoint = waypoints[currentWaypointIndex];
        
        // Check if we've reached the current waypoint
        float distance = Vector3.Distance(currentPos, targetWaypoint);
        if (distance < waypointReachDistance)
        {
            // Move to next waypoint
            currentWaypointIndex = (currentWaypointIndex + 1) % waypoints.Length;
            Vector3 nextWaypoint = waypoints[currentWaypointIndex];
            
            // Adjust waypoint altitude for terrain-relative mode
            if (droneController.IsUsingTerrainRelativeAltitude())
            {
                nextWaypoint.y = targetAltitudeAboveGround; // Height above ground
            }
            
            droneController.GoTo(nextWaypoint);
            Debug.Log($"[TerrainFollowingTest] Reached waypoint, moving to waypoint {currentWaypointIndex}: {nextWaypoint}");
        }
    }
    
    private void LogCurrentStatus()
    {
        string mode = droneController.IsUsingTerrainRelativeAltitude() ? "terrain-relative" : "absolute";
        float rangeDistance = droneController.GetRangeSensorDistance();
        float altitudeAgl = droneController.GetAltitudeAgl();
        float altitudeError = droneController.GetAltitudeError();
        
        Debug.Log($"[TerrainFollowingTest] Mode: {mode}, " +
                  $"Range: {rangeDistance:F2}m, " +
                  $"AGL: {altitudeAgl:F2}m, " +
                  $"Alt Error: {altitudeError:F2}m, " +
                  $"Waypoint: {currentWaypointIndex}");
    }
    
    private void EndTest()
    {
        if (testCompleted) return;
        
        testCompleted = true;
        Debug.Log("[TerrainFollowingTest] Test completed!");
        
        // Land the drone
        droneController.ClearTarget();
        droneController.Disarm();
        
        // Final report
        string finalMode = droneController.IsUsingTerrainRelativeAltitude() ? "terrain-relative" : "absolute";
        Debug.Log($"[TerrainFollowingTest] Final status - Mode: {finalMode}, " +
                  $"Range: {droneController.GetRangeSensorDistance():F2}m, " +
                  $"AGL: {droneController.GetAltitudeAgl():F2}m");
    }
    
    // Manual test controls
    [ContextMenu("Toggle Terrain Relative Mode")]
    public void ManualToggleMode()
    {
        if (droneController != null)
        {
            ToggleAltitudeMode();
        }
    }
    
    [ContextMenu("Test Range Sensor Reading")]
    public void TestRangeSensorReading()
    {
        if (rangeSensor != null)
        {
            Debug.Log($"[TerrainFollowingTest] Range sensor reading: {rangeSensor.GetDistance():F2}m, " +
                      $"In range: {rangeSensor.IsInRange()}, " +
                      $"Confidence: {(rangeSensor.LastReading?.confidence ?? 0f):F2}");
        }
    }
    
    [ContextMenu("Reset to First Waypoint")]
    public void ResetToFirstWaypoint()
    {
        currentWaypointIndex = 0;
        if (waypoints.Length > 0 && droneController != null)
        {
            droneController.GoTo(waypoints[0]);
            Debug.Log($"[TerrainFollowingTest] Reset to waypoint 0: {waypoints[0]}");
        }
    }
    
    private void OnGUI()
    {
        if (!enableAutoTest || testCompleted) return;
        
        // Enhanced on-screen display
        int y = 10;
        int lineHeight = 20;
        
        GUI.Label(new Rect(10, y, 400, lineHeight), $"Terrain Following Test - Time: {Time.time - testStartTime:F1}s");
        y += lineHeight;
        
        string mode = droneController.IsUsingTerrainRelativeAltitude() ? "TERRAIN-RELATIVE" : "ABSOLUTE";
        GUI.Label(new Rect(10, y, 400, lineHeight), $"Altitude Mode: {mode}");
        y += lineHeight;
        
        GUI.Label(new Rect(10, y, 400, lineHeight), $"Range Sensor: {droneController.GetRangeSensorDistance():F2}m");
        y += lineHeight;
        
        GUI.Label(new Rect(10, y, 400, lineHeight), $"Altitude AGL: {droneController.GetAltitudeAgl():F2}m");
        y += lineHeight;
        
        GUI.Label(new Rect(10, y, 400, lineHeight), $"Altitude Error: {droneController.GetAltitudeError():F2}m");
        y += lineHeight;
        
        if (enableNavigationTest)
        {
            GUI.Label(new Rect(10, y, 400, lineHeight), $"Current Waypoint: {currentWaypointIndex} / {waypoints.Length}");
            y += lineHeight;
        }
        
        float nextToggle = modeToggleInterval - (Time.time - modeToggleTimer);
        GUI.Label(new Rect(10, y, 400, lineHeight), $"Next mode toggle in: {nextToggle:F1}s");
    }
}