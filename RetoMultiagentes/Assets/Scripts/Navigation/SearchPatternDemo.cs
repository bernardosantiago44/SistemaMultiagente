using UnityEngine;

/// <summary>
/// Demonstration script showing how to use the LawnmowerPattern for search missions
/// Integrates with the existing mission system and provides an example of pattern usage
/// </summary>
public class SearchPatternDemo : MonoBehaviour
{
    [Header("Search Configuration")]
    [SerializeField] private LawnmowerPattern lawnmowerPattern;
    [SerializeField] private Navigator navigator;
    [SerializeField] private DroneController droneController;
    
    [Header("Demo Mission Parameters")]
    [SerializeField] private Vector3 searchAreaCenter = new Vector3(100, 0, 100);
    [SerializeField] private float searchAreaSize = 200f; // 200x200 meter search area
    [SerializeField] private float searchAltitude = 60f; // 60 meter search altitude
    [SerializeField] private float sensorCoverageWidth = 25f; // 25 meter sensor coverage
    [SerializeField] private float passOverlap = 15f; // 15% overlap between passes
    
    [Header("Demo Controls")]
    [SerializeField] private bool autoStartOnPlay = false;
    [SerializeField] private bool visualizeSearchArea = true;
    
    [Header("Mission Status")]
    [SerializeField] private bool missionActive = false;
    [SerializeField] private int totalWaypoints = 0;
    [SerializeField] private float expectedCoverage = 0f;
    [SerializeField] private string missionStatus = "Ready";
    
    void Start()
    {
        // Auto-find components if not assigned
        if (lawnmowerPattern == null) lawnmowerPattern = GetComponent<LawnmowerPattern>();
        if (navigator == null) navigator = FindFirstObjectByType<Navigator>();
        if (droneController == null) droneController = FindFirstObjectByType<DroneController>();
        
        // Create lawnmower pattern component if it doesn't exist
        if (lawnmowerPattern == null)
        {
            lawnmowerPattern = gameObject.AddComponent<LawnmowerPattern>();
            Debug.Log("[SearchPatternDemo] Created LawnmowerPattern component");
        }
        
        // Subscribe to pattern events
        if (lawnmowerPattern != null)
        {
            lawnmowerPattern.OnWaypointsGenerated += OnWaypointsGenerated;
            lawnmowerPattern.OnCoverageCalculated += OnCoverageCalculated;
        }
        
        // Subscribe to navigation events
        if (navigator != null)
        {
            navigator.OnNavigationStarted += OnNavigationStarted;
            navigator.OnTargetReached += OnTargetReached;
            navigator.OnNavigationCompleted += OnNavigationCompleted;
        }
        
        if (autoStartOnPlay)
        {
            StartSearchMission();
        }
    }
    
    void OnDestroy()
    {
        // Unsubscribe from events
        if (lawnmowerPattern != null)
        {
            lawnmowerPattern.OnWaypointsGenerated -= OnWaypointsGenerated;
            lawnmowerPattern.OnCoverageCalculated -= OnCoverageCalculated;
        }
        
        if (navigator != null)
        {
            navigator.OnNavigationStarted -= OnNavigationStarted;
            navigator.OnTargetReached -= OnTargetReached;
            navigator.OnNavigationCompleted -= OnNavigationCompleted;
        }
    }
    
    /// <summary>
    /// Start a search mission using the lawnmower pattern
    /// </summary>
    [ContextMenu("Start Search Mission")]
    public void StartSearchMission()
    {
        if (missionActive)
        {
            Debug.LogWarning("[SearchPatternDemo] Mission already active");
            return;
        }
        
        if (lawnmowerPattern == null)
        {
            Debug.LogError("[SearchPatternDemo] No LawnmowerPattern component available");
            return;
        }
        
        Debug.Log("[SearchPatternDemo] Starting lawnmower search mission...");
        missionStatus = "Configuring Pattern";
        
        // Configure the search pattern
        ConfigureSearchPattern();
        
        // Execute the pattern
        missionStatus = "Generating Waypoints";
        lawnmowerPattern.ExecutePattern();
        
        missionActive = true;
        missionStatus = "Mission Active";
        
        Debug.Log($"[SearchPatternDemo] Search mission started with {totalWaypoints} waypoints and {expectedCoverage:F1}% coverage");
    }
    
    /// <summary>
    /// Stop the current search mission
    /// </summary>
    [ContextMenu("Stop Search Mission")]
    public void StopSearchMission()
    {
        if (!missionActive)
        {
            Debug.LogWarning("[SearchPatternDemo] No active mission to stop");
            return;
        }
        
        Debug.Log("[SearchPatternDemo] Stopping search mission...");
        
        // Stop navigation
        if (navigator != null)
        {
            navigator.StopNavigation();
        }
        
        // Clear pattern waypoints
        if (lawnmowerPattern != null)
        {
            lawnmowerPattern.ClearWaypoints();
        }
        
        missionActive = false;
        missionStatus = "Mission Stopped";
        totalWaypoints = 0;
        expectedCoverage = 0f;
        
        Debug.Log("[SearchPatternDemo] Search mission stopped");
    }
    
    /// <summary>
    /// Configure the lawnmower pattern with demo parameters
    /// </summary>
    private void ConfigureSearchPattern()
    {
        if (lawnmowerPattern == null) return;
        
        // Set basic pattern parameters
        lawnmowerPattern.SetCenterPosition(searchAreaCenter);
        lawnmowerPattern.SetAreaSize(searchAreaSize);
        lawnmowerPattern.SetAltitude(searchAltitude);
        lawnmowerPattern.SetSensorWidth(sensorCoverageWidth);
        lawnmowerPattern.SetOverlapPercentage(passOverlap);
        
        // Calculate optimal step distance based on sensor coverage and overlap
        float overlapDistance = sensorCoverageWidth * (passOverlap / 100f);
        float optimalStep = sensorCoverageWidth - overlapDistance;
        lawnmowerPattern.SetStepDistance(optimalStep);
        
        // Enable coverage optimization
        lawnmowerPattern.SetCoverageOptimization(true);
        
        // Configure pattern direction (start from bottom, left-to-right first)
        lawnmowerPattern.SetPatternDirection(true, true);
        
        // Set turn radius for smooth navigation
        lawnmowerPattern.SetTurnRadius(10f);
        
        Debug.Log($"[SearchPatternDemo] Pattern configured: {searchAreaSize}x{searchAreaSize}m area, {optimalStep:F1}m steps, {passOverlap}% overlap");
    }
    
    /// <summary>
    /// Event handler for when waypoints are generated
    /// </summary>
    private void OnWaypointsGenerated(System.Collections.Generic.List<Vector3> waypoints)
    {
        totalWaypoints = waypoints.Count;
        Debug.Log($"[SearchPatternDemo] Pattern generated {totalWaypoints} waypoints");
    }
    
    /// <summary>
    /// Event handler for when coverage is calculated
    /// </summary>
    private void OnCoverageCalculated(float coverage)
    {
        expectedCoverage = coverage;
        
        if (coverage >= 90f)
        {
            Debug.Log($"[SearchPatternDemo] Coverage requirement met: {coverage:F1}%");
        }
        else
        {
            Debug.LogWarning($"[SearchPatternDemo] Coverage below requirement: {coverage:F1}% (need ≥90%)");
        }
    }
    
    /// <summary>
    /// Event handler for navigation started
    /// </summary>
    private void OnNavigationStarted(Vector3 target)
    {
        missionStatus = "Navigating to Search Area";
        Debug.Log($"[SearchPatternDemo] Navigation started to: {target}");
    }
    
    /// <summary>
    /// Event handler for target reached
    /// </summary>
    private void OnTargetReached(Vector3 target)
    {
        Debug.Log($"[SearchPatternDemo] Waypoint reached: {target}");
        
        // Update mission status based on remaining waypoints
        if (navigator != null && navigator.IsNavigating())
        {
            missionStatus = "Executing Search Pattern";
        }
    }
    
    /// <summary>
    /// Event handler for navigation completed
    /// </summary>
    private void OnNavigationCompleted()
    {
        missionStatus = "Search Pattern Completed";
        missionActive = false;
        
        Debug.Log("[SearchPatternDemo] Search pattern navigation completed!");
        
        // Here you could trigger additional actions like:
        // - Analyzing collected data
        // - Returning to base
        // - Starting a new search pattern
    }
    
    /// <summary>
    /// Manual method to configure a custom search area
    /// </summary>
    /// <param name="center">Center of search area</param>
    /// <param name="size">Size of search area</param>
    /// <param name="altitude">Search altitude</param>
    public void ConfigureCustomSearchArea(Vector3 center, float size, float altitude)
    {
        searchAreaCenter = center;
        searchAreaSize = size;
        searchAltitude = altitude;
        
        Debug.Log($"[SearchPatternDemo] Custom search area configured: Center={center}, Size={size}m, Altitude={altitude}m");
    }
    
    /// <summary>
    /// Get mission status information
    /// </summary>
    /// <returns>Mission status string</returns>
    public string GetMissionStatus()
    {
        if (missionActive && navigator != null)
        {
            float distanceToTarget = navigator.GetDistanceToTarget();
            Vector3 currentTarget = navigator.GetCurrentTarget();
            
            return $"{missionStatus} | Target: {currentTarget} | Distance: {distanceToTarget:F1}m";
        }
        
        return missionStatus;
    }
    
#if UNITY_EDITOR
    private void OnDrawGizmos()
    {
        if (!visualizeSearchArea) return;
        
        // Draw search area
        Gizmos.color = new Color(1, 1, 0, 0.3f); // Yellow with transparency
        Vector3 searchCenter = searchAreaCenter + Vector3.up * searchAltitude;
        Gizmos.DrawCube(searchCenter, new Vector3(searchAreaSize, 2f, searchAreaSize));
        
        // Draw search area outline
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireCube(searchCenter, new Vector3(searchAreaSize, 2f, searchAreaSize));
        
        // Draw center point
        Gizmos.color = Color.red;
        Gizmos.DrawSphere(searchCenter, 3f);
        
        // Draw area label
        Vector3 labelPos = searchCenter + Vector3.up * 15f;
        UnityEditor.Handles.Label(labelPos, $"Search Area\n{searchAreaSize}x{searchAreaSize}m\nAltitude: {searchAltitude}m");
    }
#endif
}