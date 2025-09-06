using UnityEngine;

/// <summary>
/// Demo script for navigation system
/// Provides simple UI and context menu options for testing navigation
/// </summary>
public class NavigationDemo : MonoBehaviour
{
    [Header("Navigation References")]
    [SerializeField] private Navigator navigator;
    [SerializeField] private DroneController droneController;
    [SerializeField] private WaypointQueue waypointQueue;
    
    [Header("Demo Configuration")]
    [SerializeField] private Vector2 demoGpsOrigin = new Vector2(19.432608f, -99.133209f); // Mexico City
    [SerializeField] private Vector2 demoGpsTarget = new Vector2(19.444608f, -99.121209f); // ~1.5km NE
    [SerializeField] private Vector3 demoWorldTarget = new Vector3(200f, 50f, 200f);
    
    [Header("Demo Waypoints")]
    [SerializeField] private Vector3[] demoWaypoints = {
        new Vector3(50f, 30f, 0f),
        new Vector3(100f, 40f, 50f),
        new Vector3(150f, 50f, 100f),
        new Vector3(200f, 60f, 150f)
    };
    
    void Start()
    {
        // Find components if not assigned
        if (navigator == null) navigator = FindObjectOfType<Navigator>();
        if (droneController == null) droneController = FindObjectOfType<DroneController>();
        if (waypointQueue == null) waypointQueue = FindObjectOfType<WaypointQueue>();
        
        // Set GPS origin
        if (navigator != null)
        {
            navigator.SetGpsOrigin(demoGpsOrigin);
        }
        
        Debug.Log("[NavigationDemo] Navigation Demo initialized. Use context menu or inspector buttons to test navigation.");
    }
    
    /// <summary>
    /// Demo: Navigate to GPS coordinates
    /// </summary>
    [ContextMenu("Demo: Navigate to GPS Target")]
    public void DemoGpsNavigation()
    {
        if (navigator == null)
        {
            Debug.LogError("[NavigationDemo] Navigator not found!");
            return;
        }
        
        Debug.Log($"[NavigationDemo] Starting GPS navigation to: {demoGpsTarget}");
        navigator.GoToGpsCoordinates(demoGpsTarget);
    }
    
    /// <summary>
    /// Demo: Navigate to world position
    /// </summary>
    [ContextMenu("Demo: Navigate to World Target")]
    public void DemoWorldNavigation()
    {
        if (navigator == null)
        {
            Debug.LogError("[NavigationDemo] Navigator not found!");
            return;
        }
        
        Debug.Log($"[NavigationDemo] Starting world navigation to: {demoWorldTarget}");
        navigator.GoToWorldPosition(demoWorldTarget);
    }
    
    /// <summary>
    /// Demo: Navigate through multiple waypoints
    /// </summary>
    [ContextMenu("Demo: Navigate Waypoint Route")]
    public void DemoWaypointNavigation()
    {
        if (navigator == null)
        {
            Debug.LogError("[NavigationDemo] Navigator not found!");
            return;
        }
        
        Debug.Log($"[NavigationDemo] Starting waypoint navigation with {demoWaypoints.Length} waypoints");
        
        // Clear any existing navigation
        navigator.ClearNavigation();
        
        // Add all demo waypoints
        foreach (var waypoint in demoWaypoints)
        {
            navigator.AddWaypoint(waypoint);
        }
    }
    
    /// <summary>
    /// Demo: Navigate to nearby position (< 150m)
    /// </summary>
    [ContextMenu("Demo: Navigate Short Distance")]
    public void DemoShortDistanceNavigation()
    {
        if (navigator == null)
        {
            Debug.LogError("[NavigationDemo] Navigator not found!");
            return;
        }
        
        Vector3 shortTarget = transform.position + new Vector3(50f, 10f, 50f); // ~70m away
        Debug.Log($"[NavigationDemo] Starting short distance navigation to: {shortTarget}");
        navigator.GoToWorldPosition(shortTarget);
    }
    
    /// <summary>
    /// Demo: Navigate to distant position (> 150m)
    /// </summary>
    [ContextMenu("Demo: Navigate Long Distance")]
    public void DemoLongDistanceNavigation()
    {
        if (navigator == null)
        {
            Debug.LogError("[NavigationDemo] Navigator not found!");
            return;
        }
        
        Vector3 longTarget = transform.position + new Vector3(300f, 50f, 300f); // ~424m away
        Debug.Log($"[NavigationDemo] Starting long distance navigation to: {longTarget}");
        navigator.GoToWorldPosition(longTarget);
    }
    
    /// <summary>
    /// Demo: Stop current navigation
    /// </summary>
    [ContextMenu("Demo: Stop Navigation")]
    public void DemoStopNavigation()
    {
        if (navigator == null)
        {
            Debug.LogError("[NavigationDemo] Navigator not found!");
            return;
        }
        
        Debug.Log("[NavigationDemo] Stopping navigation");
        navigator.StopNavigation();
    }
    
    /// <summary>
    /// Demo: Toggle manual/automatic control
    /// </summary>
    [ContextMenu("Demo: Toggle Manual Control")]
    public void DemoToggleManualControl()
    {
        if (droneController == null)
        {
            Debug.LogError("[NavigationDemo] DroneController not found!");
            return;
        }
        
        // This would require accessing the debugManualInput field
        Debug.Log("[NavigationDemo] Manual control toggle - check DroneController inspector to toggle debugManualInput");
    }
    
    /// <summary>
    /// Demo: Show navigation status
    /// </summary>
    [ContextMenu("Demo: Show Navigation Status")]
    public void DemoShowStatus()
    {
        Debug.Log("[NavigationDemo] === Navigation Status ===");
        
        if (navigator != null)
        {
            Debug.Log($"Navigating: {navigator.IsNavigating()}");
            Debug.Log($"Distance to target: {navigator.GetDistanceToTarget():F1}m");
            Debug.Log($"Current target: {navigator.GetCurrentTarget()}");
            Debug.Log($"Current GPS target: {navigator.GetCurrentTargetGps()}");
        }
        
        if (droneController != null)
        {
            Debug.Log($"Drone armed: {droneController.IsArmed()}");
            Debug.Log($"Drone in flight: {droneController.InFlight()}");
            Debug.Log($"Drone altitude: {droneController.GetAltitudeAgl():F1}m");
            Debug.Log($"Drone velocity: {droneController.GetVelocity()}");
            Debug.Log($"Drone has target: {droneController.HasTargetPosition()}");
        }
        
        if (waypointQueue != null)
        {
            Debug.Log($"Waypoints in queue: {waypointQueue.GetWaypointCount()}");
            Debug.Log($"Has waypoints: {waypointQueue.HasWaypoints()}");
        }
    }
    
    void OnGUI()
    {
        // Simple GUI for testing
        GUILayout.BeginArea(new Rect(10, 10, 200, 400));
        GUILayout.Label("Navigation Demo Controls");
        
        if (GUILayout.Button("GPS Navigation"))
        {
            DemoGpsNavigation();
        }
        
        if (GUILayout.Button("World Navigation"))
        {
            DemoWorldNavigation();
        }
        
        if (GUILayout.Button("Waypoint Route"))
        {
            DemoWaypointNavigation();
        }
        
        if (GUILayout.Button("Short Distance"))
        {
            DemoShortDistanceNavigation();
        }
        
        if (GUILayout.Button("Long Distance"))
        {
            DemoLongDistanceNavigation();
        }
        
        if (GUILayout.Button("Stop Navigation"))
        {
            DemoStopNavigation();
        }
        
        if (GUILayout.Button("Show Status"))
        {
            DemoShowStatus();
        }
        
        GUILayout.EndArea();
    }
}