using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Test script to verify Spawnpoints and PersonSpawner functionality
/// </summary>
public class SpawnpointsTest : MonoBehaviour
{
    [Header("Test Configuration")]
    [SerializeField] private bool runTestsOnStart = true;
    [SerializeField] private bool verboseLogging = true;
    
    [Header("Drone Integration Test")]
    [SerializeField] private DroneController droneController;
    [SerializeField] private float droneTestThreshold = 5.0f;
    [SerializeField] private bool testDroneCanReachSpawnPoints = false;

    void Start()
    {
        if (runTestsOnStart)
        {
            RunTests();
        }
    }

    /// <summary>
    /// Run all spawn point tests
    /// </summary>
    public void RunTests()
    {
        Debug.Log("[SpawnpointsTest] Starting Spawnpoints System Tests...");
        
        TestSpawnpointsLoading();
        TestRandomSpawnPointSelection();
        TestSpawnPointCount();
        TestSpawnPointByIndex();
        TestPersonSpawnerIntegration();
        
        if (testDroneCanReachSpawnPoints && droneController != null)
        {
            TestDroneReachability();
        }
        
        Debug.Log("[SpawnpointsTest] Spawnpoints System Tests Completed");
    }

    /// <summary>
    /// Test that spawn points are loaded correctly from JSON
    /// </summary>
    private void TestSpawnpointsLoading()
    {
        Debug.Log("[SpawnpointsTest] Testing Spawnpoints Loading...");
        
        int spawnPointCount = Spawnpoints.GetSpawnPointCount();
        
        if (spawnPointCount >= 20)
        {
            Debug.Log($"✓ Spawn points loaded successfully: {spawnPointCount} points (meets minimum requirement of 20)");
        }
        else
        {
            Debug.LogError($"✗ Insufficient spawn points: {spawnPointCount} (minimum required: 20)");
        }
        
        List<Vector3> allPoints = Spawnpoints.GetAllSpawnPoints();
        if (allPoints.Count == spawnPointCount)
        {
            Debug.Log("✓ GetAllSpawnPoints() returns correct count");
        }
        else
        {
            Debug.LogError($"✗ GetAllSpawnPoints() count mismatch: {allPoints.Count} vs {spawnPointCount}");
        }
    }

    /// <summary>
    /// Test random spawn point selection
    /// </summary>
    private void TestRandomSpawnPointSelection()
    {
        Debug.Log("[SpawnpointsTest] Testing Random Spawn Point Selection...");
        
        HashSet<Vector3> selectedPoints = new HashSet<Vector3>();
        int testIterations = 10;
        
        for (int i = 0; i < testIterations; i++)
        {
            Vector3 randomPoint = Spawnpoints.GetRandomSpawnPoint();
            
            if (randomPoint != Vector3.zero)
            {
                selectedPoints.Add(randomPoint);
                if (verboseLogging)
                {
                    Debug.Log($"  Random point {i + 1}: {randomPoint}");
                }
            }
            else
            {
                Debug.LogError($"✗ GetRandomSpawnPoint() returned Vector3.zero on iteration {i + 1}");
            }
        }
        
        Debug.Log($"✓ Random selection test completed. Selected {selectedPoints.Count} unique points out of {testIterations} iterations");
        
        if (selectedPoints.Count > 1)
        {
            Debug.Log("✓ Random selection is working (multiple unique points selected)");
        }
        else if (selectedPoints.Count == 1 && Spawnpoints.GetSpawnPointCount() == 1)
        {
            Debug.Log("✓ Random selection consistent with single spawn point");
        }
        else
        {
            Debug.LogWarning("⚠ Random selection may not be working properly (low variety in selected points)");
        }
    }

    /// <summary>
    /// Test spawn point count functionality
    /// </summary>
    private void TestSpawnPointCount()
    {
        Debug.Log("[SpawnpointsTest] Testing Spawn Point Count...");
        
        int count = Spawnpoints.GetSpawnPointCount();
        List<Vector3> allPoints = Spawnpoints.GetAllSpawnPoints();
        
        if (count == allPoints.Count)
        {
            Debug.Log($"✓ Spawn point count is consistent: {count}");
        }
        else
        {
            Debug.LogError($"✗ Spawn point count inconsistency: GetSpawnPointCount()={count}, GetAllSpawnPoints().Count={allPoints.Count}");
        }
    }

    /// <summary>
    /// Test accessing spawn points by index
    /// </summary>
    private void TestSpawnPointByIndex()
    {
        Debug.Log("[SpawnpointsTest] Testing Spawn Point By Index...");
        
        int totalCount = Spawnpoints.GetSpawnPointCount();
        
        if (totalCount > 0)
        {
            // Test valid indices
            Vector3 firstPoint = Spawnpoints.GetSpawnPointByIndex(0);
            Vector3 lastPoint = Spawnpoints.GetSpawnPointByIndex(totalCount - 1);
            
            if (firstPoint != Vector3.zero && lastPoint != Vector3.zero)
            {
                Debug.Log($"✓ Valid index access works: first={firstPoint}, last={lastPoint}");
            }
            else
            {
                Debug.LogError("✗ Valid index access returned Vector3.zero");
            }
            
            // Test invalid indices
            Vector3 invalidLow = Spawnpoints.GetSpawnPointByIndex(-1);
            Vector3 invalidHigh = Spawnpoints.GetSpawnPointByIndex(totalCount);
            
            if (invalidLow == Vector3.zero && invalidHigh == Vector3.zero)
            {
                Debug.Log("✓ Invalid index access correctly returns Vector3.zero");
            }
            else
            {
                Debug.LogError("✗ Invalid index access should return Vector3.zero");
            }
        }
        else
        {
            Debug.LogError("✗ No spawn points available for index testing");
        }
    }

    /// <summary>
    /// Test PersonSpawner integration
    /// </summary>
    private void TestPersonSpawnerIntegration()
    {
        Debug.Log("[SpawnpointsTest] Testing PersonSpawner Integration...");
        
        PersonSpawner spawner = FindFirstObjectByType<PersonSpawner>();
        
        if (spawner != null)
        {
            string spawnInfo = spawner.GetSpawnPointInfo();
            Debug.Log($"✓ PersonSpawner found: {spawnInfo}");
            
            // Note: We don't actually spawn a person in the test to avoid cluttering the scene
            // but we verify that the spawner can access spawn point information
        }
        else
        {
            Debug.LogWarning("⚠ No PersonSpawner found in scene for integration test");
        }
    }

    /// <summary>
    /// Test that drone can potentially reach spawn points (basic distance check)
    /// </summary>
    private void TestDroneReachability()
    {
        Debug.Log("[SpawnpointsTest] Testing Drone Reachability...");
        
        if (droneController == null)
        {
            Debug.LogError("✗ DroneController not assigned for reachability test");
            return;
        }
        
        Vector3 dronePosition = droneController.transform.position;
        List<Vector3> allSpawnPoints = Spawnpoints.GetAllSpawnPoints();
        
        int reachablePoints = 0;
        float maxDistance = 0f;
        float minDistance = float.MaxValue;
        
        foreach (Vector3 spawnPoint in allSpawnPoints)
        {
            float distance = Vector3.Distance(dronePosition, spawnPoint);
            
            if (distance <= droneTestThreshold)
            {
                reachablePoints++;
            }
            
            maxDistance = Mathf.Max(maxDistance, distance);
            minDistance = Mathf.Min(minDistance, distance);
            
            if (verboseLogging)
            {
                Debug.Log($"  Distance to {spawnPoint}: {distance:F2}m");
            }
        }
        
        Debug.Log($"Drone reachability analysis:");
        Debug.Log($"  Drone position: {dronePosition}");
        Debug.Log($"  Points within threshold ({droneTestThreshold}m): {reachablePoints}/{allSpawnPoints.Count}");
        Debug.Log($"  Distance range: {minDistance:F2}m - {maxDistance:F2}m");
        
        if (reachablePoints > 0)
        {
            Debug.Log($"✓ Drone can reach {reachablePoints} spawn points within threshold");
        }
        else
        {
            Debug.LogWarning($"⚠ No spawn points within drone threshold of {droneTestThreshold}m. Consider adjusting threshold or spawn point positions.");
        }
    }

    /// <summary>
    /// Manual test trigger for runtime testing
    /// </summary>
    [ContextMenu("Run Tests")]
    public void RunTestsManually()
    {
        RunTests();
    }
}