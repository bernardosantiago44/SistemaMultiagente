using UnityEngine;

/// <summary>
/// Demo script showing how to use the new PID-based flight control system
/// Demonstrates Issue #8 implementation for altitude and velocity control
/// </summary>
public class FlightControlDemo : MonoBehaviour
{
    [Header("Demo Configuration")]
    [SerializeField] private DroneController droneController;
    [SerializeField] private FlightProfile flightProfile;
    
    [Header("Test Waypoints")]
    [SerializeField] private Vector3[] waypoints = new Vector3[]
    {
        new Vector3(0, 10, 0),
        new Vector3(20, 15, 20),
        new Vector3(-20, 20, 20),
        new Vector3(0, 5, 0)
    };
    
    [Header("Demo Controls")]
    [SerializeField] private bool autoDemo = true;
    [SerializeField] private float waypointHoldTime = 10f;
    [SerializeField] private float reachThreshold = 2f;
    
    private int currentWaypointIndex = 0;
    private float waypointReachedTime = 0f;
    private bool waypointReached = false;

    void Start()
    {
        if (droneController == null)
            droneController = FindFirstObjectByType<DroneController>();
            
        if (droneController != null && autoDemo)
        {
            StartDemo();
        }
    }

    void Update()
    {
        if (!autoDemo || droneController == null) return;

        HandleWaypointNavigation();
        LogFlightMetrics();
    }

    private void StartDemo()
    {
        Debug.Log("[FlightControlDemo] Starting PID flight control demonstration");
        
        if (waypoints.Length > 0)
        {
            GoToWaypoint(0);
        }
    }

    private void HandleWaypointNavigation()
    {
        if (waypoints.Length == 0) return;

        Vector3 currentPos = droneController.transform.position;
        Vector3 currentTarget = waypoints[currentWaypointIndex];
        float distanceToTarget = Vector3.Distance(currentPos, currentTarget);

        // Check if we've reached the current waypoint
        if (!waypointReached && distanceToTarget < reachThreshold)
        {
            waypointReached = true;
            waypointReachedTime = Time.time;
            Debug.Log($"[FlightControlDemo] Reached waypoint {currentWaypointIndex}: {currentTarget}");
        }

        // Wait at waypoint, then move to next
        if (waypointReached && (Time.time - waypointReachedTime) > waypointHoldTime)
        {
            int nextWaypoint = (currentWaypointIndex + 1) % waypoints.Length;
            GoToWaypoint(nextWaypoint);
        }
    }

    private void GoToWaypoint(int index)
    {
        if (index < 0 || index >= waypoints.Length) return;

        currentWaypointIndex = index;
        waypointReached = false;
        
        Vector3 target = waypoints[index];
        droneController.GoTo(target);
        
        Debug.Log($"[FlightControlDemo] Going to waypoint {index}: {target}");
    }

    private void LogFlightMetrics()
    {
        if (Time.frameCount % 60 == 0) // Log every second at 60 FPS
        {
            float altError = droneController.GetAltitudeError();
            float velError = droneController.GetVelocityError();
            Vector3 velocity = droneController.GetVelocity();
            Vector3 position = droneController.transform.position;

            Debug.Log($"[FlightControlDemo] Pos: ({position.x:F1}, {position.y:F1}, {position.z:F1}) | " +
                      $"Alt Error: {altError:F2}m | Vel: {velocity.magnitude:F2}m/s (Error: {velError:F2}m/s)");
        }
    }

    // Manual controls for testing
    [ContextMenu("Go to Waypoint 0")]
    public void GoToWaypoint0() => GoToWaypoint(0);
    
    [ContextMenu("Go to Waypoint 1")]
    public void GoToWaypoint1() => GoToWaypoint(1);
    
    [ContextMenu("Go to Waypoint 2")]
    public void GoToWaypoint2() => GoToWaypoint(2);
    
    [ContextMenu("Go to Waypoint 3")]
    public void GoToWaypoint3() => GoToWaypoint(3);
    
    [ContextMenu("Reset PID Controllers")]
    public void ResetControllers()
    {
        if (droneController != null)
        {
            droneController.ResetPIDControllers();
            Debug.Log("[FlightControlDemo] PID controllers reset");
        }
    }

    [ContextMenu("Tune PID - Conservative")]
    public void SetConservativePID()
    {
        if (flightProfile != null)
        {
            // Conservative settings for stable but slower response
            flightProfile.altitudeKp = 0.8f;
            flightProfile.altitudeKi = 0.05f;
            flightProfile.altitudeKd = 0.3f;
            flightProfile.velocityKp = 0.8f;
            flightProfile.velocityKi = 0.05f;
            flightProfile.velocityKd = 0.3f;
            Debug.Log("[FlightControlDemo] Applied conservative PID tuning");
        }
    }

    [ContextMenu("Tune PID - Aggressive")]
    public void SetAggressivePID()
    {
        if (flightProfile != null)
        {
            // Aggressive settings for faster response
            flightProfile.altitudeKp = 1.5f;
            flightProfile.altitudeKi = 0.2f;
            flightProfile.altitudeKd = 0.8f;
            flightProfile.velocityKp = 1.5f;
            flightProfile.velocityKi = 0.2f;
            flightProfile.velocityKd = 0.8f;
            Debug.Log("[FlightControlDemo] Applied aggressive PID tuning");
        }
    }

#if UNITY_EDITOR
    private void OnDrawGizmos()
    {
        if (waypoints == null) return;

        // Draw waypoint path
        Gizmos.color = Color.yellow;
        for (int i = 0; i < waypoints.Length; i++)
        {
            Gizmos.DrawWireSphere(waypoints[i], 1f);
            
            // Draw path connections
            int nextIndex = (i + 1) % waypoints.Length;
            Gizmos.DrawLine(waypoints[i], waypoints[nextIndex]);
            
            // Label waypoints
            #if UNITY_EDITOR
            UnityEditor.Handles.Label(waypoints[i] + Vector3.up * 2, $"WP{i}");
            #endif
        }

        // Highlight current target
        if (currentWaypointIndex < waypoints.Length)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(waypoints[currentWaypointIndex], 1.5f);
        }
    }
#endif
}