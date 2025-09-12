using UnityEngine;
using System.Collections;

/// <summary>
/// Example script demonstrating integration between PersonSpawner and DroneController
/// This script shows how the drone can visit spawn points where persons are located
/// </summary>
public class DronePersonInteraction : MonoBehaviour
{
    [Header("Components")]
    [SerializeField] private DroneController droneController;
    [SerializeField] private PersonSpawner personSpawner;
    
    [Header("Mission Configuration")]
    [SerializeField] private bool startMissionOnStart = false;
    [SerializeField] private bool visitAllSpawnPoints = false;
    [SerializeField] private float droneAltitudeAboveSpawn = 10f;
    [SerializeField] private float timeAtEachPoint = 3f;
    
    [Header("Debug")]
    [SerializeField] private bool debugMode = true;
    
    private bool missionInProgress = false;
    // private int currentSpawnPointIndex = 0;

    void Start()
    {
        if (startMissionOnStart)
        {
            StartCoroutine(RunMission());
        }
    }

    /// <summary>
    /// Main mission coroutine that demonstrates drone visiting spawn points
    /// </summary>
    /// <returns></returns>
    public IEnumerator RunMission()
    {
        if (missionInProgress)
        {
            Debug.LogWarning("[DronePersonInteraction] Mission already in progress");
            yield break;
        }

        missionInProgress = true;
        
        if (droneController == null)
        {
            Debug.LogError("[DronePersonInteraction] DroneController not assigned");
            missionInProgress = false;
            yield break;
        }

        Debug.Log("[DronePersonInteraction] Starting mission: Drone visiting spawn points");
        
        // Get all spawn points
        var spawnPoints = Spawnpoints.GetAllSpawnPoints();
        
        if (spawnPoints.Count == 0)
        {
            Debug.LogError("[DronePersonInteraction] No spawn points available");
            missionInProgress = false;
            yield break;
        }

        Debug.Log($"[DronePersonInteraction] Found {spawnPoints.Count} spawn points to visit");

        // Spawn a person first if PersonSpawner is available
        if (personSpawner != null)
        {
            Debug.Log("[DronePersonInteraction] Spawning person at random location");
            personSpawner.SpawnPerson();
            yield return new WaitForSeconds(1f); // Brief pause after spawning
        }

        // Take off to mission altitude
        droneController.TakeOff(droneAltitudeAboveSpawn);
        yield return new WaitForSeconds(2f);

        if (visitAllSpawnPoints)
        {
            // Visit all spawn points sequentially
            for (int i = 0; i < spawnPoints.Count; i++)
            {
                yield return StartCoroutine(VisitSpawnPoint(spawnPoints[i], i + 1));
            }
        }
        else
        {
            // Visit just a few random spawn points
            int pointsToVisit = Mathf.Min(3, spawnPoints.Count);
            
            for (int i = 0; i < pointsToVisit; i++)
            {
                Vector3 randomPoint = Spawnpoints.GetRandomSpawnPoint();
                yield return StartCoroutine(VisitSpawnPoint(randomPoint, i + 1));
            }
        }

        Debug.Log("[DronePersonInteraction] Mission completed successfully");
        missionInProgress = false;
    }

    /// <summary>
    /// Coroutine to visit a specific spawn point
    /// </summary>
    /// <param name="spawnPoint">The spawn point coordinates</param>
    /// <param name="pointNumber">Number of this point in the sequence</param>
    /// <returns></returns>
    private IEnumerator VisitSpawnPoint(Vector3 spawnPoint, int pointNumber)
    {
        // Create target position at altitude above the spawn point
        Vector3 targetPosition = new Vector3(spawnPoint.x, spawnPoint.y + droneAltitudeAboveSpawn, spawnPoint.z);
        
        Debug.Log($"[DronePersonInteraction] Visiting spawn point {pointNumber}: {spawnPoint} (target: {targetPosition})");
        
        // Command drone to go to the position
        droneController.GoTo(targetPosition);
        
        // Wait for drone to reach the target (simple distance check)
        float timeout = 30f; // Maximum time to wait
        float startTime = Time.time;
        
        while (Vector3.Distance(droneController.transform.position, targetPosition) > 2f && 
               (Time.time - startTime) < timeout)
        {
            yield return new WaitForSeconds(0.1f);
        }
        
        if ((Time.time - startTime) >= timeout)
        {
            Debug.LogWarning($"[DronePersonInteraction] Timeout reaching spawn point {pointNumber}");
        }
        else
        {
            Debug.Log($"[DronePersonInteraction] Reached spawn point {pointNumber}");
        }
        
        // Stay at the position for the specified time
        yield return new WaitForSeconds(timeAtEachPoint);
        
        if (debugMode)
        {
            Debug.Log($"[DronePersonInteraction] Leaving spawn point {pointNumber}");
        }
    }

    /// <summary>
    /// Manually start the mission (can be called from UI or other scripts)
    /// </summary>
    [ContextMenu("Start Mission")]
    public void StartMission()
    {
        if (!missionInProgress)
        {
            StartCoroutine(RunMission());
        }
    }

    /// <summary>
    /// Stop the current mission
    /// </summary>
    [ContextMenu("Stop Mission")]
    public void StopMission()
    {
        if (missionInProgress)
        {
            StopAllCoroutines();
            missionInProgress = false;
            droneController.ClearTarget();
            Debug.Log("[DronePersonInteraction] Mission stopped");
        }
    }

    /// <summary>
    /// Test drone can reach a random spawn point
    /// </summary>
    [ContextMenu("Test Drone Reach")]
    public void TestDroneReachability()
    {
        if (droneController == null)
        {
            Debug.LogError("[DronePersonInteraction] DroneController not assigned");
            return;
        }

        Vector3 randomSpawnPoint = Spawnpoints.GetRandomSpawnPoint();
        Vector3 dronePosition = droneController.transform.position;
        float distance = Vector3.Distance(dronePosition, randomSpawnPoint);
        
        Debug.Log($"[DronePersonInteraction] Drone at {dronePosition}");
        Debug.Log($"[DronePersonInteraction] Random spawn point: {randomSpawnPoint}");
        Debug.Log($"[DronePersonInteraction] Distance: {distance:F2}m");
        
        // Test if drone can be commanded to go there
        Vector3 targetAboveSpawn = new Vector3(randomSpawnPoint.x, randomSpawnPoint.y + droneAltitudeAboveSpawn, randomSpawnPoint.z);
        droneController.GoTo(targetAboveSpawn);
        
        Debug.Log($"[DronePersonInteraction] Commanded drone to go to: {targetAboveSpawn}");
    }

#if UNITY_EDITOR
    private void OnDrawGizmosSelected()
    {
        if (!debugMode) return;

        // Draw spawn points
        var spawnPoints = Spawnpoints.GetAllSpawnPoints();
        foreach (var point in spawnPoints)
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireSphere(point, 1f);
            
            // Draw target altitude above spawn point
            Vector3 targetPos = new Vector3(point.x, point.y + droneAltitudeAboveSpawn, point.z);
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(targetPos, 0.5f);
            Gizmos.DrawLine(point, targetPos);
        }

        // Draw drone current position and target if available
        if (droneController != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(droneController.transform.position, 1.5f);
            
            if (droneController.HasTargetPosition())
            {
                Gizmos.color = Color.green;
                Vector3 target = droneController.GetTargetPosition();
                Gizmos.DrawWireSphere(target, 1f);
                Gizmos.DrawLine(droneController.transform.position, target);
            }
        }
    }
#endif
}