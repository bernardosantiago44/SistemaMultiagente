using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Queue system for managing waypoints in navigation
/// Provides functionality to add, retrieve, and manage waypoints for drone navigation
/// </summary>
public class WaypointQueue : MonoBehaviour
{
    [Header("Waypoint Configuration")]
    [SerializeField] private List<Vector3> waypoints = new List<Vector3>();
    [SerializeField] private bool autoRemoveReached = true;
    [SerializeField] private float reachThreshold = 2.0f; // meters
    
    [Header("Debug")]
    [SerializeField] private bool logWaypointOperations = true;
    [SerializeField] private bool visualizeWaypoints = true;
    
    private Queue<Vector3> waypointQueue = new Queue<Vector3>();
    private Vector3? currentTarget = null;
    
    void Awake()
    {
        // Initialize queue with serialized waypoints
        foreach (var waypoint in waypoints)
        {
            waypointQueue.Enqueue(waypoint);
        }
    }
    
    /// <summary>
    /// Adds a waypoint to the queue
    /// </summary>
    /// <param name="waypoint">World position waypoint to add</param>
    public void AddWaypoint(Vector3 waypoint)
    {
        waypointQueue.Enqueue(waypoint);
        
        if (logWaypointOperations)
        {
            Debug.Log($"[WaypointQueue] Waypoint added: {waypoint}. Queue size: {waypointQueue.Count}");
        }
    }
    
    /// <summary>
    /// Adds multiple waypoints to the queue
    /// </summary>
    /// <param name="waypointList">List of world position waypoints to add</param>
    public void AddWaypoints(List<Vector3> waypointList)
    {
        foreach (var waypoint in waypointList)
        {
            waypointQueue.Enqueue(waypoint);
        }
        
        if (logWaypointOperations)
        {
            Debug.Log($"[WaypointQueue] {waypointList.Count} waypoints added. Queue size: {waypointQueue.Count}");
        }
    }
    
    /// <summary>
    /// Gets the next waypoint without removing it from the queue
    /// </summary>
    /// <returns>Next waypoint or null if queue is empty</returns>
    public Vector3? PeekNextWaypoint()
    {
        if (waypointQueue.Count > 0)
        {
            return waypointQueue.Peek();
        }
        return null;
    }
    
    /// <summary>
    /// Gets and removes the next waypoint from the queue
    /// </summary>
    /// <returns>Next waypoint or null if queue is empty</returns>
    public Vector3? GetNextWaypoint()
    {
        if (waypointQueue.Count > 0)
        {
            Vector3 nextWaypoint = waypointQueue.Dequeue();
            currentTarget = nextWaypoint;
            
            if (logWaypointOperations)
            {
                Debug.Log($"[WaypointQueue] Waypoint dequeued: {nextWaypoint}. Remaining: {waypointQueue.Count}");
            }
            
            return nextWaypoint;
        }
        
        currentTarget = null;
        return null;
    }
    
    /// <summary>
    /// Gets the current target waypoint
    /// </summary>
    /// <returns>Current target or null if none</returns>
    public Vector3? GetCurrentTarget()
    {
        return currentTarget;
    }
    
    /// <summary>
    /// Checks if the given position has reached the current target
    /// </summary>
    /// <param name="currentPosition">Current position to check</param>
    /// <returns>True if target is reached within threshold</returns>
    public bool IsTargetReached(Vector3 currentPosition)
    {
        if (currentTarget == null) return false;
        
        float distance = Vector3.Distance(currentPosition, currentTarget.Value);
        return distance <= reachThreshold;
    }
    
    /// <summary>
    /// Marks the current target as reached and optionally gets the next waypoint
    /// </summary>
    /// <param name="currentPosition">Current position for validation</param>
    /// <returns>Next waypoint or null if none</returns>
    public Vector3? MarkTargetReached(Vector3 currentPosition)
    {
        if (currentTarget != null)
        {
            if (logWaypointOperations)
            {
                float distance = Vector3.Distance(currentPosition, currentTarget.Value);
                Debug.Log($"[WaypointQueue] Target reached: {currentTarget.Value} (distance: {distance:F2}m)");
            }
            
            currentTarget = null;
            
            if (autoRemoveReached)
            {
                return GetNextWaypoint();
            }
        }
        
        return null;
    }
    
    /// <summary>
    /// Clears all waypoints from the queue
    /// </summary>
    public void ClearWaypoints()
    {
        int count = waypointQueue.Count;
        waypointQueue.Clear();
        currentTarget = null;
        
        if (logWaypointOperations)
        {
            Debug.Log($"[WaypointQueue] {count} waypoints cleared");
        }
    }
    
    /// <summary>
    /// Gets the number of waypoints remaining in the queue
    /// </summary>
    /// <returns>Number of waypoints in queue</returns>
    public int GetWaypointCount()
    {
        return waypointQueue.Count;
    }
    
    /// <summary>
    /// Checks if the queue has waypoints
    /// </summary>
    /// <returns>True if queue has waypoints</returns>
    public bool HasWaypoints()
    {
        return waypointQueue.Count > 0 || currentTarget != null;
    }
    
    /// <summary>
    /// Sets the reach threshold for waypoints
    /// </summary>
    /// <param name="threshold">Distance threshold in meters</param>
    public void SetReachThreshold(float threshold)
    {
        reachThreshold = Mathf.Max(0.1f, threshold);
        
        if (logWaypointOperations)
        {
            Debug.Log($"[WaypointQueue] Reach threshold set to: {reachThreshold}m");
        }
    }
    
#if UNITY_EDITOR
    private void OnDrawGizmos()
    {
        if (!visualizeWaypoints) return;
        
        // Draw queue waypoints
        Gizmos.color = Color.yellow;
        int index = 0;
        foreach (var waypoint in waypointQueue)
        {
            Gizmos.DrawWireSphere(waypoint, 1.0f);
            index++;
        }
        
        // Draw current target
        if (currentTarget != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(currentTarget.Value, reachThreshold);
            Gizmos.DrawSphere(currentTarget.Value, 0.5f);
        }
        
        // Draw path lines
        if (waypointQueue.Count > 1)
        {
            Gizmos.color = Color.cyan;
            Vector3 prev = currentTarget ?? transform.position;
            
            foreach (var waypoint in waypointQueue)
            {
                Gizmos.DrawLine(prev, waypoint);
                prev = waypoint;
            }
        }
    }
    
    private void OnDrawGizmosSelected()
    {
        if (!visualizeWaypoints) return;
        
        // Draw reach threshold for current target
        if (currentTarget != null)
        {
            Gizmos.color = new Color(1, 0, 0, 0.3f);
            Gizmos.DrawSphere(currentTarget.Value, reachThreshold);
        }
    }
#endif
}