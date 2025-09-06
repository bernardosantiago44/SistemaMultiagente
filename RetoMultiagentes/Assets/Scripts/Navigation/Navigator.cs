using UnityEngine;

/// <summary>
/// Main navigation controller for drone movement towards target positions
/// Integrates with DroneController and uses GpsMapper for coordinate conversion
/// Implements simple kinematic movement following KISS principle
/// </summary>
[RequireComponent(typeof(DroneController))]
public class Navigator : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private DroneController droneController;
    [SerializeField] private WaypointQueue waypointQueue;
    
    [Header("Navigation Configuration")]
    [SerializeField] private float minimumDistance = 150f; // meters - requirement from issue
    [SerializeField] private float targetReachThreshold = 5.0f; // meters
    //[SerializeField] private float navigationSpeed = 10f; // m/s
    [SerializeField] private float targetAltitude = 50f; // meters AGL
    
    [Header("GPS Configuration")]
    [SerializeField] private Vector2 gpsOrigin = new Vector2(19.432608f, -99.133209f); // Default Mexico City coordinates
    //[SerializeField] private bool useGpsCoordinates = true;
    
    [Header("Navigation State")]
    [SerializeField] private bool isNavigating = false;
    [SerializeField] private Vector3 currentTarget = Vector3.zero;
    [SerializeField] private Vector2 currentTargetGps = Vector2.zero;
    [SerializeField] private float distanceToTarget = 0f;
    
    [Header("Debug")]
    [SerializeField] private bool enableLogging = true;
    [SerializeField] private bool visualizeNavigation = true;
    
    // Navigation state
    private bool hasValidTarget = false;
    private Vector3 lastPosition = Vector3.zero;
    
    // Events
    public System.Action<Vector3> OnTargetReached;
    public System.Action<Vector3> OnNavigationStarted;
    public System.Action OnNavigationCompleted;
    
    private void Reset()
    {
        droneController = GetComponent<DroneController>();
        waypointQueue = GetComponent<WaypointQueue>();
    }
    
    void Awake()
    {
        if (droneController == null) droneController = GetComponent<DroneController>();
        if (waypointQueue == null) waypointQueue = GetComponent<WaypointQueue>();
        
        // Subscribe to mission events to automatically start navigation
        MissionManager.OnMissionLoaded += OnMissionLoaded;
    }
    
    void OnDestroy()
    {
        MissionManager.OnMissionLoaded -= OnMissionLoaded;
    }
    
    void Start()
    {
        lastPosition = transform.position;
    }
    
    void Update()
    {
        if (!isNavigating || !hasValidTarget) return;
        
        UpdateDistanceToTarget();
        CheckTargetReached();
        UpdateNavigationLogging();
    }
    
    /// <summary>
    /// Event handler for when a mission is loaded
    /// Automatically starts navigation to mission GPS coordinates
    /// </summary>
    /// <param name="mission">Loaded mission with GPS coordinates</param>
    private void OnMissionLoaded(Mission mission)
    {
        if (mission != null && mission.IsValid())
        {
            if (enableLogging)
            {
                Debug.Log($"[Navigator] Mission loaded, starting navigation to: {mission.GpsCoordinates}");
            }
            
            GoToGpsCoordinates(mission.GpsCoordinates);
        }
    }
    
    /// <summary>
    /// Navigate to GPS coordinates
    /// </summary>
    /// <param name="gpsCoordinates">Target GPS coordinates (latitude, longitude)</param>
    public void GoToGpsCoordinates(Vector2 gpsCoordinates)
    {
        if (!GpsMapper.IsValidGpsCoordinates(gpsCoordinates))
        {
            Debug.LogError($"[Navigator] Invalid GPS coordinates: {gpsCoordinates}");
            return;
        }
        
        // Convert GPS to Unity world position
        Vector3 worldPosition = GpsMapper.GpsToUnityPosition(gpsCoordinates, gpsOrigin);
        worldPosition.y = targetAltitude; // Set target altitude
        
        currentTargetGps = gpsCoordinates;
        
        if (enableLogging)
        {
            Debug.Log($"[Navigator] GPS target: {gpsCoordinates} -> World position: {worldPosition}");
        }
        
        GoToWorldPosition(worldPosition);
    }
    
    /// <summary>
    /// Navigate to world position
    /// </summary>
    /// <param name="worldPosition">Target world position</param>
    public void GoToWorldPosition(Vector3 worldPosition)
    {
        // Check minimum distance requirement
        float distance = Vector3.Distance(transform.position, worldPosition);
        
        if (distance < minimumDistance)
        {
            if (enableLogging)
            {
                Debug.LogWarning($"[Navigator] Target distance ({distance:F1}m) is less than minimum required ({minimumDistance}m). Proceeding anyway.");
            }
        }
        
        currentTarget = worldPosition;
        hasValidTarget = true;
        isNavigating = true;
        
        // Log transit message as required
        Debug.Log("En tránsito");
        
        if (enableLogging)
        {
            Debug.Log($"[Navigator] Starting navigation to world position: {worldPosition} (distance: {distance:F1}m)");
        }
        
        // Set target in drone controller
        droneController.GoTo(worldPosition);
        
        // Trigger navigation started event
        OnNavigationStarted?.Invoke(worldPosition);
    }
    
    /// <summary>
    /// Add waypoint to queue and start navigation if not already navigating
    /// </summary>
    /// <param name="waypoint">Waypoint to add</param>
    public void AddWaypoint(Vector3 waypoint)
    {
        if (waypointQueue != null)
        {
            waypointQueue.AddWaypoint(waypoint);
            
            // Start navigation if not already navigating
            if (!isNavigating)
            {
                ProcessNextWaypoint();
            }
        }
        else
        {
            // If no waypoint queue, navigate directly
            GoToWorldPosition(waypoint);
        }
    }
    
    /// <summary>
    /// Add GPS waypoint to queue
    /// </summary>
    /// <param name="gpsWaypoint">GPS coordinates to add as waypoint</param>
    public void AddGpsWaypoint(Vector2 gpsWaypoint)
    {
        Vector3 worldPosition = GpsMapper.GpsToUnityPosition(gpsWaypoint, gpsOrigin);
        worldPosition.y = targetAltitude;
        AddWaypoint(worldPosition);
    }
    
    /// <summary>
    /// Process the next waypoint from the queue
    /// </summary>
    private void ProcessNextWaypoint()
    {
        if (waypointQueue == null) return;
        
        Vector3? nextWaypoint = waypointQueue.GetNextWaypoint();
        if (nextWaypoint != null)
        {
            GoToWorldPosition(nextWaypoint.Value);
        }
        else
        {
            // No more waypoints
            StopNavigation();
        }
    }
    
    /// <summary>
    /// Stop current navigation
    /// </summary>
    public void StopNavigation()
    {
        isNavigating = false;
        hasValidTarget = false;
        currentTarget = Vector3.zero;
        currentTargetGps = Vector2.zero;
        
        if (enableLogging)
        {
            Debug.Log("[Navigator] Navigation stopped");
        }
        
        OnNavigationCompleted?.Invoke();
    }
    
    /// <summary>
    /// Clear all waypoints and stop navigation
    /// </summary>
    public void ClearNavigation()
    {
        StopNavigation();
        
        if (waypointQueue != null)
        {
            waypointQueue.ClearWaypoints();
        }
        
        if (enableLogging)
        {
            Debug.Log("[Navigator] Navigation cleared");
        }
    }
    
    /// <summary>
    /// Update distance to current target
    /// </summary>
    private void UpdateDistanceToTarget()
    {
        if (hasValidTarget)
        {
            distanceToTarget = Vector3.Distance(transform.position, currentTarget);
        }
    }
    
    /// <summary>
    /// Check if target has been reached
    /// </summary>
    private void CheckTargetReached()
    {
        if (!hasValidTarget) return;
        
        if (distanceToTarget <= targetReachThreshold)
        {
            if (enableLogging)
            {
                Debug.Log($"[Navigator] Target reached: {currentTarget} (distance: {distanceToTarget:F2}m)");
            }
            
            Vector3 reachedTarget = currentTarget;
            
            // Mark target as reached
            OnTargetReached?.Invoke(reachedTarget);
            
            // Process next waypoint if available
            if (waypointQueue != null && waypointQueue.HasWaypoints())
            {
                ProcessNextWaypoint();
            }
            else
            {
                // No more targets
                StopNavigation();
            }
        }
    }
    
    /// <summary>
    /// Update navigation logging
    /// </summary>
    private void UpdateNavigationLogging()
    {
        if (!enableLogging) return;
        
        // Log progress periodically
        float distanceTraveled = Vector3.Distance(transform.position, lastPosition);
        if (distanceTraveled > 10f) // Log every 10 meters of movement
        {
            Debug.Log($"[Navigator] En tránsito - Distance to target: {distanceToTarget:F1}m");
            lastPosition = transform.position;
        }
    }
    
    /// <summary>
    /// Set GPS origin for coordinate conversion
    /// </summary>
    /// <param name="origin">GPS origin coordinates</param>
    public void SetGpsOrigin(Vector2 origin)
    {
        if (GpsMapper.IsValidGpsCoordinates(origin))
        {
            gpsOrigin = origin;
            
            if (enableLogging)
            {
                Debug.Log($"[Navigator] GPS origin set to: {origin}");
            }
        }
        else
        {
            Debug.LogError($"[Navigator] Invalid GPS origin coordinates: {origin}");
        }
    }
    
    /// <summary>
    /// Get current navigation status
    /// </summary>
    /// <returns>True if currently navigating</returns>
    public bool IsNavigating()
    {
        return isNavigating && hasValidTarget;
    }
    
    /// <summary>
    /// Get distance to current target
    /// </summary>
    /// <returns>Distance in meters</returns>
    public float GetDistanceToTarget()
    {
        return distanceToTarget;
    }
    
    /// <summary>
    /// Get current target position
    /// </summary>
    /// <returns>Current target world position</returns>
    public Vector3 GetCurrentTarget()
    {
        return currentTarget;
    }
    
    /// <summary>
    /// Get current target GPS coordinates
    /// </summary>
    /// <returns>Current target GPS coordinates</returns>
    public Vector2 GetCurrentTargetGps()
    {
        return currentTargetGps;
    }
    
#if UNITY_EDITOR
    private void OnDrawGizmos()
    {
        if (!visualizeNavigation) return;
        
        // Draw current target
        if (hasValidTarget)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(currentTarget, targetReachThreshold);
            Gizmos.DrawSphere(currentTarget, 1f);
            
            // Draw line to target
            Gizmos.color = Color.green;
            Gizmos.DrawLine(transform.position, currentTarget);
            
            // Draw distance text
            Vector3 midPoint = Vector3.Lerp(transform.position, currentTarget, 0.5f);
            UnityEditor.Handles.Label(midPoint, $"{distanceToTarget:F1}m");
        }
        
        // Draw GPS origin
        Vector3 originWorld = GpsMapper.GpsToUnityPosition(gpsOrigin, gpsOrigin);
        Gizmos.color = Color.blue;
        Gizmos.DrawWireCube(originWorld, Vector3.one * 2f);
    }
    
    private void OnDrawGizmosSelected()
    {
        if (!visualizeNavigation) return;
        
        // Draw minimum distance circle
        Gizmos.color = new Color(1, 1, 0, 0.2f);
        Gizmos.DrawSphere(transform.position, minimumDistance);
        
        // Draw reach threshold
        if (hasValidTarget)
        {
            Gizmos.color = new Color(0, 1, 0, 0.3f);
            Gizmos.DrawSphere(currentTarget, targetReachThreshold);
        }
    }
#endif
}