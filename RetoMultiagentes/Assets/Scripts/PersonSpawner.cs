using UnityEngine;

/// <summary>
/// PersonSpawner handles spawning person objects at random locations from the spawn points database
/// </summary>
public class PersonSpawner : MonoBehaviour
{
    [Header("Spawning Configuration")]
    [SerializeField] private GameObject personPrefab;
    [SerializeField] private bool spawnOnStart = true;
    [SerializeField] private float spawnRadius = 1.0f;
    [SerializeField] private LayerMask groundLayerMask = -1;
    
    [Header("Multiple Spawning")]
    [SerializeField] private bool spawnMultiplePersons = false;
    [SerializeField] private int numberOfPersonsToSpawn = 1;
    [SerializeField] private float minDistanceBetweenSpawns = 2.0f;
    
    [Header("Debug")]
    [SerializeField] private bool showDebugGizmos = true;
    
    private Vector3 lastSpawnPosition = Vector3.zero;

    void Start()
    {
        if (spawnOnStart)
        {
            if (spawnMultiplePersons)
            {
                SpawnMultiplePersons();
            }
            else
            {
                SpawnPerson();
            }
        }
    }

    /// <summary>
    /// Spawn a single person at a random spawn point
    /// </summary>
    /// <returns>The spawned GameObject, or null if spawning failed</returns>
    public GameObject SpawnPerson()
    {
        Vector3 spawnPosition = Spawnpoints.GetRandomSpawnPoint();
        
        if (spawnPosition == Vector3.zero)
        {
            Debug.LogError("[PersonSpawner] Failed to get spawn position from Spawnpoints");
            return null;
        }

        return SpawnPersonAtPosition(spawnPosition);
    }

    /// <summary>
    /// Spawn multiple persons at different random spawn points
    /// </summary>
    /// <returns>Array of spawned GameObjects</returns>
    public GameObject[] SpawnMultiplePersons()
    {
        int availableSpawnPoints = Spawnpoints.GetSpawnPointCount();
        int actualSpawnCount = Mathf.Min(numberOfPersonsToSpawn, availableSpawnPoints);
        
        if (actualSpawnCount == 0)
        {
            Debug.LogError("[PersonSpawner] No spawn points available for spawning");
            return new GameObject[0];
        }

        GameObject[] spawnedPersons = new GameObject[actualSpawnCount];
        var usedSpawnPoints = new System.Collections.Generic.HashSet<int>();

        for (int i = 0; i < actualSpawnCount; i++)
        {
            Vector3 spawnPosition;
            int attempts = 0;
            int maxAttempts = availableSpawnPoints * 2; // Prevent infinite loop
            
            do
            {
                spawnPosition = Spawnpoints.GetRandomSpawnPoint();
                attempts++;
                
                if (attempts > maxAttempts)
                {
                    Debug.LogWarning($"[PersonSpawner] Could not find suitable spawn point after {maxAttempts} attempts for person {i + 1}");
                    break;
                }
            }
            while (IsPositionTooClose(spawnPosition, usedSpawnPoints) && attempts < maxAttempts);

            GameObject spawnedPerson = SpawnPersonAtPosition(spawnPosition);
            if (spawnedPerson != null)
            {
                spawnedPersons[i] = spawnedPerson;
                // Track this position to maintain minimum distance
                lastSpawnPosition = spawnPosition;
            }
        }

        Debug.Log($"[PersonSpawner] Successfully spawned {actualSpawnCount} persons");
        return spawnedPersons;
    }

    /// <summary>
    /// Spawn a person at a specific position with ground alignment
    /// </summary>
    /// <param name="position">World position to spawn the person</param>
    /// <returns>The spawned GameObject, or null if spawning failed</returns>
    private GameObject SpawnPersonAtPosition(Vector3 position)
    {
        if (personPrefab == null)
        {
            Debug.LogError("[PersonSpawner] Person prefab is not assigned!");
            return null;
        }

        // Adjust position to ground if needed
        Vector3 finalPosition = AdjustPositionToGround(position);
        
        // Create rotation (can be random or facing a specific direction)
        Quaternion rotation = Quaternion.Euler(0, Random.Range(0f, 360f), 0);
        
        // Instantiate the person
        GameObject spawnedPerson = Instantiate(personPrefab, finalPosition, rotation);
        
        // Set a meaningful name
        spawnedPerson.name = $"Person_Spawned_{System.DateTime.Now:HHmmss}";
        
        Debug.Log($"[PersonSpawner] Spawned person at position: {finalPosition}");
        lastSpawnPosition = finalPosition;
        
        return spawnedPerson;
    }

    /// <summary>
    /// Adjust spawn position to align with ground using raycast
    /// </summary>
    /// <param name="position">Original spawn position</param>
    /// <returns>Ground-aligned position</returns>
    private Vector3 AdjustPositionToGround(Vector3 position)
    {
        // Cast ray downward from spawn position
        Vector3 rayStart = position + Vector3.up * 10f; // Start from above
        
        if (Physics.Raycast(rayStart, Vector3.down, out RaycastHit hit, 50f, groundLayerMask))
        {
            // Position person on ground surface
            return hit.point;
        }
        
        // If no ground found, use original position
        Debug.LogWarning($"[PersonSpawner] No ground found at spawn position: {position}");
        return position;
    }

    /// <summary>
    /// Check if a position is too close to previously used spawn points
    /// </summary>
    /// <param name="position">Position to check</param>
    /// <param name="usedSpawnPoints">Set of previously used spawn point indices</param>
    /// <returns>True if position is too close to existing spawns</returns>
    private bool IsPositionTooClose(Vector3 position, System.Collections.Generic.HashSet<int> usedSpawnPoints)
    {
        if (lastSpawnPosition == Vector3.zero) return false;
        
        float distance = Vector3.Distance(position, lastSpawnPosition);
        return distance < minDistanceBetweenSpawns;
    }

    /// <summary>
    /// Public method to spawn a person manually (can be called from UI or other scripts)
    /// </summary>
    public void SpawnPersonManually()
    {
        SpawnPerson();
    }

    /// <summary>
    /// Get information about available spawn points for debugging
    /// </summary>
    /// <returns>String with spawn point information</returns>
    public string GetSpawnPointInfo()
    {
        int count = Spawnpoints.GetSpawnPointCount();
        return $"Available spawn points: {count}";
    }

#if UNITY_EDITOR
    /// <summary>
    /// Draw debug gizmos to visualize spawn points
    /// </summary>
    private void OnDrawGizmos()
    {
        if (!showDebugGizmos) return;

        // Draw all spawn points
        var allSpawnPoints = Spawnpoints.GetAllSpawnPoints();
        
        foreach (var point in allSpawnPoints)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(point, spawnRadius);
            
            // Draw a small upward arrow to indicate spawn direction
            Gizmos.color = Color.yellow;
            Gizmos.DrawRay(point, Vector3.up * 2f);
        }

        // Highlight last spawn position
        if (lastSpawnPosition != Vector3.zero)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(lastSpawnPosition, spawnRadius * 1.2f);
        }
    }

    /// <summary>
    /// Draw debug gizmos when selected
    /// </summary>
    private void OnDrawGizmosSelected()
    {
        if (!showDebugGizmos) return;

        // Draw spawn radius
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, spawnRadius);

        // Draw minimum distance indicator if spawning multiple
        if (spawnMultiplePersons)
        {
            Gizmos.color = Color.magenta;
            Gizmos.DrawWireSphere(transform.position, minDistanceBetweenSpawns);
        }
    }
#endif
}