using System.Collections.Generic;
using System.IO;
using UnityEngine;

/// <summary>
/// Static class that provides access to person spawn points loaded from JSON configuration
/// </summary>
public static class Spawnpoints
{
    private static List<Vector3> spawnPositions = null;
    private static bool isInitialized = false;

    /// <summary>
    /// Data structure for deserializing spawn points from JSON
    /// </summary>
    [System.Serializable]
    private class SpawnPointData
    {
        public int id;
        public float x;
        public float y;
        public float z;
    }

    /// <summary>
    /// Container for the JSON array of spawn locations
    /// </summary>
    [System.Serializable]
    private class SpawnPointsContainer
    {
        public List<SpawnPointData> spawnLocations;
    }

    /// <summary>
    /// Initialize the spawn points by loading from JSON file
    /// </summary>
    private static void Initialize()
    {
        if (isInitialized) return;

        spawnPositions = new List<Vector3>();
        
        try
        {
            string filePath = Path.Combine(Application.streamingAssetsPath, "spawn_points.json");
            
            if (File.Exists(filePath))
            {
                string jsonContent = File.ReadAllText(filePath);
                SpawnPointsContainer container = JsonUtility.FromJson<SpawnPointsContainer>(jsonContent);
                
                if (container?.spawnLocations != null)
                {
                    foreach (var spawnPoint in container.spawnLocations)
                    {
                        spawnPositions.Add(new Vector3(spawnPoint.x, spawnPoint.y, spawnPoint.z));
                    }
                    
                    Debug.Log($"[Spawnpoints] Loaded {spawnPositions.Count} spawn points from JSON");
                }
                else
                {
                    Debug.LogError("[Spawnpoints] Failed to parse spawn points from JSON - container or spawnLocations is null");
                }
            }
            else
            {
                Debug.LogError($"[Spawnpoints] Spawn points file not found at: {filePath}");
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError($"[Spawnpoints] Error loading spawn points: {e.Message}");
        }
        
        isInitialized = true;
    }

    /// <summary>
    /// Get a random spawn position from the available spawn points
    /// </summary>
    /// <returns>Random Vector3 position, or Vector3.zero if no spawn points available</returns>
    public static Vector3 GetRandomSpawnPoint()
    {
        Initialize();
        
        if (spawnPositions == null || spawnPositions.Count == 0)
        {
            Debug.LogWarning("[Spawnpoints] No spawn points available, returning Vector3.zero");
            return Vector3.zero;
        }
        
        int randomIndex = Random.Range(0, spawnPositions.Count);
        Vector3 selectedPoint = spawnPositions[randomIndex];
        
        Debug.Log($"[Spawnpoints] Selected spawn point {randomIndex}: {selectedPoint}");
        return selectedPoint;
    }

    /// <summary>
    /// Get all available spawn points
    /// </summary>
    /// <returns>List of all spawn point positions</returns>
    public static List<Vector3> GetAllSpawnPoints()
    {
        Initialize();
        return new List<Vector3>(spawnPositions ?? new List<Vector3>());
    }

    /// <summary>
    /// Get the number of available spawn points
    /// </summary>
    /// <returns>Total count of spawn points</returns>
    public static int GetSpawnPointCount()
    {
        Initialize();
        return spawnPositions?.Count ?? 0;
    }

    /// <summary>
    /// Get a specific spawn point by index
    /// </summary>
    /// <param name="index">Index of the spawn point (0-based)</param>
    /// <returns>Spawn point at the specified index, or Vector3.zero if index is invalid</returns>
    public static Vector3 GetSpawnPointByIndex(int index)
    {
        Initialize();
        
        if (spawnPositions == null || index < 0 || index >= spawnPositions.Count)
        {
            Debug.LogWarning($"[Spawnpoints] Invalid spawn point index: {index}");
            return Vector3.zero;
        }
        
        return spawnPositions[index];
    }

    /// <summary>
    /// Force reload of spawn points from the JSON file
    /// </summary>
    public static void ReloadSpawnPoints()
    {
        isInitialized = false;
        spawnPositions = null;
        Initialize();
    }
}