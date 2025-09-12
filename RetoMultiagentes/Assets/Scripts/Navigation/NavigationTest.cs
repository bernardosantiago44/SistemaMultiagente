using UnityEngine;

/// <summary>
/// Test script to validate navigation functionality
/// Tests Navigator and WaypointQueue integration with DroneController and GpsMapper
/// </summary>
public class NavigationTest : MonoBehaviour
{
    [Header("Test Configuration")]
    [SerializeField] private bool runTestsOnStart = true;
    [SerializeField] private Navigator navigator;
    [SerializeField] private DroneController droneController;
    [SerializeField] private WaypointQueue waypointQueue;
    
    [Header("Test GPS Coordinates")]
    [SerializeField] private Vector2 testGpsOrigin = new Vector2(19.432608f, -99.133209f); // Mexico City
    [SerializeField] private Vector2 testGpsTarget = new Vector2(19.442608f, -99.123209f); // ~1.5km away
    
    [Header("Test World Coordinates")]
    [SerializeField] private Vector3 testWorldTarget = new Vector3(100f, 50f, 100f); // 100m east, 100m north, 50m altitude
    
    void Start()
    {
        if (runTestsOnStart)
        {
            RunTests();
        }
        
        // Subscribe to navigation events
        if (navigator != null)
        {
            navigator.OnNavigationStarted += OnNavigationStarted;
            navigator.OnTargetReached += OnTargetReached;
            navigator.OnNavigationCompleted += OnNavigationCompleted;
        }
    }
    
    void OnDestroy()
    {
        // Unsubscribe from events
        if (navigator != null)
        {
            navigator.OnNavigationStarted -= OnNavigationStarted;
            navigator.OnTargetReached -= OnTargetReached;
            navigator.OnNavigationCompleted -= OnNavigationCompleted;
        }
    }
    
    /// <summary>
    /// Event handler for navigation started
    /// </summary>
    private void OnNavigationStarted(Vector3 target)
    {
        Debug.Log($"[NavigationTest] Navigation Started to: {target}");
    }
    
    /// <summary>
    /// Event handler for target reached
    /// </summary>
    private void OnTargetReached(Vector3 target)
    {
        Debug.Log($"[NavigationTest] Target Reached: {target}");
    }
    
    /// <summary>
    /// Event handler for navigation completed
    /// </summary>
    private void OnNavigationCompleted()
    {
        Debug.Log("[NavigationTest] Navigation Completed");
    }
    
    /// <summary>
    /// Runs navigation system tests
    /// </summary>
    public void RunTests()
    {
        Debug.Log("[NavigationTest] Starting Navigation System Tests...");
        
        TestComponentReferences();
        TestGpsNavigation();
        TestWorldNavigation();
        TestWaypointQueue();
        TestMinimumDistance();
        TestDroneControllerIntegration();
        
        Debug.Log("[NavigationTest] Navigation System Tests Completed");
    }
    
    /// <summary>
    /// Tests that all required components are properly referenced
    /// </summary>
    private void TestComponentReferences()
    {
        Debug.Log("[NavigationTest] Testing Component References...");
        
        if (navigator == null)
        {
            navigator = FindFirstObjectByType<Navigator>();
        }
        
        if (droneController == null)
        {
            droneController = FindFirstObjectByType<DroneController>();
        }
        
        if (waypointQueue == null)
        {
            waypointQueue = FindFirstObjectByType<WaypointQueue>();
        }
        
        Debug.Log($"Navigator found: {navigator != null}");
        Debug.Log($"DroneController found: {droneController != null}");
        Debug.Log($"WaypointQueue found: {waypointQueue != null}");
    }
    
    /// <summary>
    /// Tests GPS coordinate navigation
    /// </summary>
    private void TestGpsNavigation()
    {
        Debug.Log("[NavigationTest] Testing GPS Navigation...");
        
        if (navigator == null)
        {
            Debug.LogError("Navigator not found for GPS navigation test");
            return;
        }
        
        // Test GPS coordinate validation
        bool validOrigin = GpsMapper.IsValidGpsCoordinates(testGpsOrigin);
        bool validTarget = GpsMapper.IsValidGpsCoordinates(testGpsTarget);
        
        Debug.Log($"GPS Origin valid: {validOrigin} ({testGpsOrigin})");
        Debug.Log($"GPS Target valid: {validTarget} ({testGpsTarget})");
        
        // Test GPS to world conversion
        Vector3 worldTarget = GpsMapper.GpsToUnityPosition(testGpsTarget, testGpsOrigin);
        Debug.Log($"GPS {testGpsTarget} -> World {worldTarget}");
        
        // Test distance calculation
        float gpsDistance = GpsMapper.CalculateGpsDistance(testGpsOrigin, testGpsTarget);
        Debug.Log($"GPS distance: {gpsDistance:F1}m");
        
        // Set GPS origin and test navigation
        navigator.SetGpsOrigin(testGpsOrigin);
    }
    
    /// <summary>
    /// Tests world position navigation
    /// </summary>
    private void TestWorldNavigation()
    {
        Debug.Log("[NavigationTest] Testing World Navigation...");
        
        if (navigator == null)
        {
            Debug.LogError("Navigator not found for world navigation test");
            return;
        }
        
        float distance = Vector3.Distance(transform.position, testWorldTarget);
        Debug.Log($"Distance to test target: {distance:F1}m");
        
        Debug.Log($"Test world target: {testWorldTarget}");
    }
    
    /// <summary>
    /// Tests waypoint queue functionality
    /// </summary>
    private void TestWaypointQueue()
    {
        Debug.Log("[NavigationTest] Testing Waypoint Queue...");
        
        if (waypointQueue == null)
        {
            Debug.LogWarning("WaypointQueue not found - creating test waypoints in Navigator");
            return;
        }
        
        // Test adding waypoints
        Vector3 waypoint1 = transform.position + new Vector3(50f, 0f, 0f);
        Vector3 waypoint2 = transform.position + new Vector3(0f, 0f, 50f);
        Vector3 waypoint3 = transform.position + new Vector3(-50f, 0f, 0f);
        
        waypointQueue.AddWaypoint(waypoint1);
        waypointQueue.AddWaypoint(waypoint2);
        waypointQueue.AddWaypoint(waypoint3);
        
        Debug.Log($"Waypoints added. Queue count: {waypointQueue.GetWaypointCount()}");
        
        // Test peek functionality
        Vector3? nextWaypoint = waypointQueue.PeekNextWaypoint();
        Debug.Log($"Next waypoint (peek): {nextWaypoint}");
        
        // Test dequeue functionality
        Vector3? dequeuedWaypoint = waypointQueue.GetNextWaypoint();
        Debug.Log($"Dequeued waypoint: {dequeuedWaypoint}");
        Debug.Log($"Remaining waypoints: {waypointQueue.GetWaypointCount()}");
    }
    
    /// <summary>
    /// Tests minimum distance requirement (150m)
    /// </summary>
    private void TestMinimumDistance()
    {
        Debug.Log("[NavigationTest] Testing Minimum Distance Requirement...");
        
        // Test short distance (should warn but proceed)
        Vector3 shortTarget = transform.position + new Vector3(10f, 0f, 0f); // 10m away
        float shortDistance = Vector3.Distance(transform.position, shortTarget);
        Debug.Log($"Short distance test: {shortDistance:F1}m (should be < 150m)");
        
        // Test long distance (should pass normally)
        Vector3 longTarget = transform.position + new Vector3(200f, 0f, 0f); // 200m away
        float longDistance = Vector3.Distance(transform.position, longTarget);
        Debug.Log($"Long distance test: {longDistance:F1}m (should be > 150m)");
    }
    
    /// <summary>
    /// Tests integration with DroneController
    /// </summary>
    private void TestDroneControllerIntegration()
    {
        Debug.Log("[NavigationTest] Testing DroneController Integration...");
        
        if (droneController == null)
        {
            Debug.LogError("DroneController not found for integration test");
            return;
        }
        
        Debug.Log($"Drone armed: {droneController.IsArmed()}");
        Debug.Log($"Drone in flight: {droneController.InFlight()}");
        Debug.Log($"Drone altitude: {droneController.GetAltitudeAgl():F1}m");
        Debug.Log($"Drone velocity: {droneController.GetVelocity()}");
        
        // Test GoTo method
        Vector3 testTarget = transform.position + new Vector3(0f, 10f, 0f);
        droneController.GoTo(testTarget);
        Debug.Log($"GoTo called with target: {testTarget}");
        Debug.Log($"Drone has target: {droneController.HasTargetPosition()}");
        Debug.Log($"Drone target position: {droneController.GetTargetPosition()}");
    }
    
    /// <summary>
    /// Context menu method to start GPS navigation test
    /// </summary>
    [ContextMenu("Test GPS Navigation")]
    public void StartGpsNavigationTest()
    {
        if (navigator != null)
        {
            Debug.Log("[NavigationTest] Starting GPS navigation test...");
            navigator.GoToGpsCoordinates(testGpsTarget);
        }
    }
    
    /// <summary>
    /// Context menu method to start world navigation test
    /// </summary>
    [ContextMenu("Test World Navigation")]
    public void StartWorldNavigationTest()
    {
        if (navigator != null)
        {
            Debug.Log("[NavigationTest] Starting world navigation test...");
            navigator.GoToWorldPosition(testWorldTarget);
        }
    }
    
    /// <summary>
    /// Context menu method to stop navigation
    /// </summary>
    [ContextMenu("Stop Navigation")]
    public void StopNavigationTest()
    {
        if (navigator != null)
        {
            Debug.Log("[NavigationTest] Stopping navigation...");
            navigator.StopNavigation();
        }
    }
    
    /// <summary>
    /// Context menu method to test orientation functionality
    /// </summary>
    [ContextMenu("Test Orientation")]
    public void TestOrientationFunctionality()
    {
        if (navigator != null && droneController != null)
        {
            Debug.Log("[NavigationTest] Testing orientation functionality...");
            
            // Get current rotation for comparison
            Vector3 currentEuler = transform.eulerAngles;
            Debug.Log($"Current drone rotation: {currentEuler}");
            
            // Test orientation towards a target 45 degrees to the right
            Vector3 rightTarget = transform.position + transform.right * 100f;
            Debug.Log($"Orienting towards right target: {rightTarget}");
            droneController.OrientTowards(rightTarget);
            
            // Log the new rotation after orientation
            Vector3 newEuler = transform.eulerAngles;
            Debug.Log($"New drone rotation after orientation: {newEuler}");
            
            // Calculate the direction we should be facing
            Vector3 expectedDirection = (rightTarget - transform.position).normalized;
            Vector3 expectedDirectionHorizontal = new Vector3(expectedDirection.x, 0f, expectedDirection.z).normalized;
            Debug.Log($"Expected direction: {expectedDirectionHorizontal}");
            Debug.Log($"Current forward direction: {transform.forward}");
        }
        else
        {
            Debug.LogError("[NavigationTest] Navigator or DroneController not found for orientation test");
        }
    }

    /// <summary>
    /// Context menu method to clear navigation
    /// </summary>
    [ContextMenu("Clear Navigation")]
    public void ClearNavigationTest()
    {
        if (navigator != null)
        {
            Debug.Log("[NavigationTest] Clearing navigation...");
            navigator.ClearNavigation();
        }
    }
}