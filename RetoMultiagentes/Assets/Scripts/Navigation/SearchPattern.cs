using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Base class for implementing search patterns that generate waypoints for area coverage
/// Provides common functionality and interface for different search algorithms
/// </summary>
public abstract class SearchPattern : MonoBehaviour
{
    [Header("Search Area Configuration")]
    [SerializeField] protected Vector3 centerPosition = Vector3.zero;
    [SerializeField] protected float areaSize = 100f; // Side length of square area in meters
    [SerializeField] protected float altitude = 50f; // Search altitude above ground
    
    [Header("Pattern Configuration")]
    [SerializeField] protected float stepDistance = 10f; // Distance between parallel lines
    [SerializeField] protected float overlapPercentage = 10f; // Overlap between adjacent passes (0-50%)
    
    [Header("Integration")]
    [SerializeField] protected Navigator navigator;
    [SerializeField] protected WaypointQueue waypointQueue;
    
    [Header("Debug")]
    [SerializeField] protected bool visualizePattern = true;
    [SerializeField] protected bool logPatternGeneration = true;
    
    // Generated waypoints
    protected List<Vector3> generatedWaypoints = new List<Vector3>();
    protected float calculatedCoverage = 0f;
    
    // Events
    public System.Action<List<Vector3>> OnWaypointsGenerated;
    public System.Action<float> OnCoverageCalculated;
    
    protected virtual void Reset()
    {
        // Auto-find components if not assigned
        if (navigator == null) navigator = GetComponent<Navigator>();
        if (waypointQueue == null) waypointQueue = GetComponent<WaypointQueue>();
    }
    
    protected virtual void Awake()
    {
        // Auto-find components if not assigned
        if (navigator == null) navigator = FindFirstObjectByType<Navigator>();
        if (waypointQueue == null) waypointQueue = FindFirstObjectByType<WaypointQueue>();
    }
    
    /// <summary>
    /// Generate waypoints for the search pattern
    /// </summary>
    /// <returns>List of waypoints covering the search area</returns>
    public abstract List<Vector3> GenerateWaypoints();
    
    /// <summary>
    /// Calculate the area coverage percentage of the generated pattern
    /// </summary>
    /// <returns>Coverage percentage (0-100)</returns>
    public abstract float CalculateCoverage();
    
    /// <summary>
    /// Execute the search pattern by generating waypoints and sending them to Navigator
    /// </summary>
    public virtual void ExecutePattern()
    {
        // Generate waypoints
        generatedWaypoints = GenerateWaypoints();
        
        if (generatedWaypoints.Count == 0)
        {
            Debug.LogWarning($"[{GetType().Name}] No waypoints generated for search pattern");
            return;
        }
        
        // Calculate coverage
        calculatedCoverage = CalculateCoverage();
        
        if (logPatternGeneration)
        {
            Debug.Log($"[{GetType().Name}] Generated {generatedWaypoints.Count} waypoints with {calculatedCoverage:F1}% coverage");
        }
        
        // Trigger events
        OnWaypointsGenerated?.Invoke(generatedWaypoints);
        OnCoverageCalculated?.Invoke(calculatedCoverage);
        
        // Send waypoints to navigation system
        if (waypointQueue != null)
        {
            waypointQueue.AddWaypoints(generatedWaypoints);
            
            if (logPatternGeneration)
            {
                Debug.Log($"[{GetType().Name}] Waypoints added to navigation queue");
            }
        }
        else if (navigator != null)
        {
            // Fallback: add waypoints directly to navigator
            foreach (var waypoint in generatedWaypoints)
            {
                navigator.AddWaypoint(waypoint);
            }
            
            if (logPatternGeneration)
            {
                Debug.Log($"[{GetType().Name}] Waypoints added directly to navigator");
            }
        }
        else
        {
            Debug.LogError($"[{GetType().Name}] No Navigator or WaypointQueue found for pattern execution");
        }
    }
    
    /// <summary>
    /// Set the center position for the search area
    /// </summary>
    /// <param name="position">Center position in world coordinates</param>
    public virtual void SetCenterPosition(Vector3 position)
    {
        centerPosition = position;
        
        if (logPatternGeneration)
        {
            Debug.Log($"[{GetType().Name}] Center position set to: {position}");
        }
    }
    
    /// <summary>
    /// Set the search area size
    /// </summary>
    /// <param name="size">Side length of square area in meters</param>
    public virtual void SetAreaSize(float size)
    {
        areaSize = Mathf.Max(1f, size);
        
        if (logPatternGeneration)
        {
            Debug.Log($"[{GetType().Name}] Area size set to: {areaSize}m");
        }
    }
    
    /// <summary>
    /// Set the step distance between pattern lines
    /// </summary>
    /// <param name="distance">Step distance in meters</param>
    public virtual void SetStepDistance(float distance)
    {
        stepDistance = Mathf.Max(0.1f, distance);
        
        if (logPatternGeneration)
        {
            Debug.Log($"[{GetType().Name}] Step distance set to: {stepDistance}m");
        }
    }
    
    /// <summary>
    /// Set the overlap percentage between adjacent passes
    /// </summary>
    /// <param name="overlap">Overlap percentage (0-50)</param>
    public virtual void SetOverlapPercentage(float overlap)
    {
        overlapPercentage = Mathf.Clamp(overlap, 0f, 50f);
        
        if (logPatternGeneration)
        {
            Debug.Log($"[{GetType().Name}] Overlap percentage set to: {overlapPercentage}%");
        }
    }
    
    /// <summary>
    /// Set the search altitude
    /// </summary>
    /// <param name="newAltitude">Altitude above ground in meters</param>
    public virtual void SetAltitude(float newAltitude)
    {
        altitude = Mathf.Max(1f, newAltitude);
        
        if (logPatternGeneration)
        {
            Debug.Log($"[{GetType().Name}] Altitude set to: {altitude}m");
        }
    }
    
    /// <summary>
    /// Get the generated waypoints
    /// </summary>
    /// <returns>List of generated waypoints</returns>
    public virtual List<Vector3> GetWaypoints()
    {
        return new List<Vector3>(generatedWaypoints);
    }
    
    /// <summary>
    /// Get the calculated coverage percentage
    /// </summary>
    /// <returns>Coverage percentage</returns>
    public virtual float GetCoverage()
    {
        return calculatedCoverage;
    }
    
    /// <summary>
    /// Clear generated waypoints
    /// </summary>
    public virtual void ClearWaypoints()
    {
        generatedWaypoints.Clear();
        calculatedCoverage = 0f;
        
        if (logPatternGeneration)
        {
            Debug.Log($"[{GetType().Name}] Waypoints cleared");
        }
    }
    
    /// <summary>
    /// Calculate effective step distance accounting for overlap
    /// </summary>
    /// <returns>Effective step distance in meters</returns>
    protected virtual float GetEffectiveStepDistance()
    {
        float overlapFactor = 1f - (overlapPercentage / 100f);
        return stepDistance * overlapFactor;
    }
    
#if UNITY_EDITOR
    protected virtual void OnDrawGizmos()
    {
        if (!visualizePattern) return;
        
        // Draw search area
        Gizmos.color = new Color(0, 1, 1, 0.3f); // Cyan with transparency
        Vector3 areaCenter = centerPosition + Vector3.up * altitude;
        Gizmos.DrawCube(areaCenter, new Vector3(areaSize, 1f, areaSize));
        
        // Draw area outline
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireCube(areaCenter, new Vector3(areaSize, 1f, areaSize));
    }
    
    protected virtual void OnDrawGizmosSelected()
    {
        if (!visualizePattern) return;
        
        // Draw generated waypoints
        if (generatedWaypoints.Count > 0)
        {
            Gizmos.color = Color.yellow;
            
            for (int i = 0; i < generatedWaypoints.Count; i++)
            {
                Vector3 waypoint = generatedWaypoints[i];
                Gizmos.DrawWireSphere(waypoint, 2f);
                
                // Draw path lines
                if (i > 0)
                {
                    Gizmos.color = Color.green;
                    Gizmos.DrawLine(generatedWaypoints[i - 1], waypoint);
                    Gizmos.color = Color.yellow;
                }
            }
        }
        
        // Draw coverage information
        if (calculatedCoverage > 0f)
        {
            Vector3 labelPos = centerPosition + Vector3.up * (altitude + 10f);
            UnityEditor.Handles.Label(labelPos, $"Coverage: {calculatedCoverage:F1}%\nWaypoints: {generatedWaypoints.Count}");
        }
    }
#endif
}