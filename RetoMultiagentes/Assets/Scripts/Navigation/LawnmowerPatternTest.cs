using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Test script to validate LawnmowerPattern functionality
/// Tests pattern generation, coverage calculation, and integration with navigation systems
/// </summary>
public class LawnmowerPatternTest : MonoBehaviour
{
    [Header("Test Configuration")]
    [SerializeField] private bool runTestsOnStart = true;
    [SerializeField] private LawnmowerPattern lawnmowerPattern;
    [SerializeField] private Navigator navigator;
    [SerializeField] private WaypointQueue waypointQueue;
    
    [Header("Test Parameters")]
    [SerializeField] private Vector3 testCenter = new Vector3(0, 0, 0);
    [SerializeField] private float testAreaSize = 100f; // 100x100 meter area
    [SerializeField] private float testStepDistance = 15f; // 15 meter steps
    [SerializeField] private float testSensorWidth = 20f; // 20 meter sensor width
    [SerializeField] private float testOverlap = 10f; // 10% overlap
    [SerializeField] private float testAltitude = 50f; // 50 meter altitude
    
    [Header("Test Results")]
    [SerializeField] private int generatedWaypoints = 0;
    [SerializeField] private float calculatedCoverage = 0f;
    [SerializeField] private bool coverageRequirementMet = false;
    
    void Start()
    {
        if (runTestsOnStart)
        {
            RunTests();
        }
    }
    
    /// <summary>
    /// Run comprehensive tests for the lawnmower pattern
    /// </summary>
    public void RunTests()
    {
        Debug.Log("[LawnmowerPatternTest] Starting Lawnmower Pattern Tests...");
        
        TestComponentReferences();
        TestPatternGeneration();
        TestCoverageCalculation();
        TestParameterValidation();
        TestNavigationIntegration();
        TestCoverageOptimization();
        
        Debug.Log("[LawnmowerPatternTest] Lawnmower Pattern Tests Completed");
    }
    
    /// <summary>
    /// Test component references and auto-discovery
    /// </summary>
    private void TestComponentReferences()
    {
        Debug.Log("[LawnmowerPatternTest] Testing Component References...");
        
        // Find components if not assigned
        if (lawnmowerPattern == null)
        {
            lawnmowerPattern = FindFirstObjectByType<LawnmowerPattern>();
            if (lawnmowerPattern == null)
            {
                // Create a test instance
                GameObject patternGO = new GameObject("Test_LawnmowerPattern");
                lawnmowerPattern = patternGO.AddComponent<LawnmowerPattern>();
                Debug.Log("Created test LawnmowerPattern instance");
            }
        }
        
        if (navigator == null)
        {
            navigator = FindFirstObjectByType<Navigator>();
        }
        
        if (waypointQueue == null)
        {
            waypointQueue = FindFirstObjectByType<WaypointQueue>();
        }
        
        Debug.Log($"LawnmowerPattern found: {lawnmowerPattern != null}");
        Debug.Log($"Navigator found: {navigator != null}");
        Debug.Log($"WaypointQueue found: {waypointQueue != null}");
    }
    
    /// <summary>
    /// Test basic pattern generation
    /// </summary>
    private void TestPatternGeneration()
    {
        Debug.Log("[LawnmowerPatternTest] Testing Pattern Generation...");
        
        if (lawnmowerPattern == null)
        {
            Debug.LogError("LawnmowerPattern not available for testing");
            return;
        }
        
        // Configure test parameters
        lawnmowerPattern.SetCenterPosition(testCenter);
        lawnmowerPattern.SetAreaSize(testAreaSize);
        lawnmowerPattern.SetStepDistance(testStepDistance);
        lawnmowerPattern.SetSensorWidth(testSensorWidth);
        lawnmowerPattern.SetOverlapPercentage(testOverlap);
        lawnmowerPattern.SetAltitude(testAltitude);
        
        // Generate waypoints
        List<Vector3> waypoints = lawnmowerPattern.GenerateWaypoints();
        generatedWaypoints = waypoints.Count;
        
        Debug.Log($"Generated waypoints: {generatedWaypoints}");
        Debug.Log($"Area size: {testAreaSize}x{testAreaSize}m");
        Debug.Log($"Step distance: {testStepDistance}m");
        Debug.Log($"Sensor width: {testSensorWidth}m");
        
        // Validate waypoints
        if (waypoints.Count > 0)
        {
            Debug.Log($"First waypoint: {waypoints[0]}");
            Debug.Log($"Last waypoint: {waypoints[waypoints.Count - 1]}");
            
            // Check if waypoints are within expected bounds
            float halfArea = testAreaSize * 0.5f;
            Vector3 expectedMin = testCenter + new Vector3(-halfArea, testAltitude, -halfArea);
            Vector3 expectedMax = testCenter + new Vector3(halfArea, testAltitude, halfArea);
            
            bool allWaypointsInBounds = true;
            foreach (var waypoint in waypoints)
            {
                if (waypoint.x < expectedMin.x || waypoint.x > expectedMax.x ||
                    waypoint.z < expectedMin.z || waypoint.z > expectedMax.z ||
                    Mathf.Abs(waypoint.y - testAltitude) > 1f)
                {
                    allWaypointsInBounds = false;
                    Debug.LogWarning($"Waypoint out of bounds: {waypoint}");
                    break;
                }
            }
            
            Debug.Log($"All waypoints in bounds: {allWaypointsInBounds}");
        }
        else
        {
            Debug.LogError("No waypoints generated!");
        }
    }
    
    /// <summary>
    /// Test coverage calculation
    /// </summary>
    private void TestCoverageCalculation()
    {
        Debug.Log("[LawnmowerPatternTest] Testing Coverage Calculation...");
        
        if (lawnmowerPattern == null)
        {
            Debug.LogError("LawnmowerPattern not available for testing");
            return;
        }
        
        calculatedCoverage = lawnmowerPattern.CalculateCoverage();
        coverageRequirementMet = calculatedCoverage >= 90f;
        
        Debug.Log($"Calculated coverage: {calculatedCoverage:F2}%");
        Debug.Log($"Coverage requirement (≥90%) met: {coverageRequirementMet}");
        
        // Test different sensor widths
        float[] testSensorWidths = { 10f, 15f, 20f, 25f, 30f };
        
        foreach (float sensorWidth in testSensorWidths)
        {
            lawnmowerPattern.SetSensorWidth(sensorWidth);
            float coverage = lawnmowerPattern.CalculateCoverage();
            Debug.Log($"Sensor width {sensorWidth}m: Coverage {coverage:F1}%");
        }
        
        // Reset to original sensor width
        lawnmowerPattern.SetSensorWidth(testSensorWidth);
    }
    
    /// <summary>
    /// Test parameter validation
    /// </summary>
    private void TestParameterValidation()
    {
        Debug.Log("[LawnmowerPatternTest] Testing Parameter Validation...");
        
        if (lawnmowerPattern == null)
        {
            Debug.LogError("LawnmowerPattern not available for testing");
            return;
        }
        
        // Test invalid parameters
        Debug.Log("Testing invalid area size (0)...");
        lawnmowerPattern.SetAreaSize(0f);
        List<Vector3> waypoints = lawnmowerPattern.GenerateWaypoints();
        Debug.Log($"Waypoints with invalid area: {waypoints.Count} (should be 0)");
        
        Debug.Log("Testing invalid step distance (0)...");
        lawnmowerPattern.SetAreaSize(testAreaSize); // Reset area
        lawnmowerPattern.SetStepDistance(0f);
        waypoints = lawnmowerPattern.GenerateWaypoints();
        Debug.Log($"Waypoints with invalid step: {waypoints.Count} (should be 0)");
        
        // Reset to valid parameters
        lawnmowerPattern.SetStepDistance(testStepDistance);
        
        // Test edge cases
        Debug.Log("Testing very small area (1m)...");
        lawnmowerPattern.SetAreaSize(1f);
        waypoints = lawnmowerPattern.GenerateWaypoints();
        Debug.Log($"Waypoints with 1m area: {waypoints.Count}");
        
        Debug.Log("Testing very large step distance...");
        lawnmowerPattern.SetAreaSize(testAreaSize); // Reset area
        lawnmowerPattern.SetStepDistance(testAreaSize * 2f);
        waypoints = lawnmowerPattern.GenerateWaypoints();
        Debug.Log($"Waypoints with large step: {waypoints.Count}");
        
        // Reset to valid test parameters
        lawnmowerPattern.SetStepDistance(testStepDistance);
    }
    
    /// <summary>
    /// Test integration with navigation system
    /// </summary>
    private void TestNavigationIntegration()
    {
        Debug.Log("[LawnmowerPatternTest] Testing Navigation Integration...");
        
        if (lawnmowerPattern == null)
        {
            Debug.LogError("LawnmowerPattern not available for testing");
            return;
        }
        
        // Test waypoint queue integration
        if (waypointQueue != null)
        {
            Debug.Log("Testing WaypointQueue integration...");
            
            int initialWaypoints = waypointQueue.GetWaypointCount();
            lawnmowerPattern.ExecutePattern();
            int finalWaypoints = waypointQueue.GetWaypointCount();
            
            int addedWaypoints = finalWaypoints - initialWaypoints;
            Debug.Log($"Waypoints added to queue: {addedWaypoints}");
            Debug.Log($"Generated waypoints: {lawnmowerPattern.GetWaypoints().Count}");
            
            if (addedWaypoints == lawnmowerPattern.GetWaypoints().Count)
            {
                Debug.Log("✓ WaypointQueue integration successful");
            }
            else
            {
                Debug.LogWarning("⚠ WaypointQueue integration issue");
            }
        }
        else
        {
            Debug.Log("WaypointQueue not available - testing direct Navigator integration");
            
            if (navigator != null)
            {
                Debug.Log("Testing direct Navigator integration...");
                // Note: This test would require the Navigator to be in a testable state
                Debug.Log("Direct Navigator integration test would require active Navigator");
            }
            else
            {
                Debug.Log("No navigation components available for integration testing");
            }
        }
    }
    
    /// <summary>
    /// Test coverage optimization features
    /// </summary>
    private void TestCoverageOptimization()
    {
        Debug.Log("[LawnmowerPatternTest] Testing Coverage Optimization...");
        
        if (lawnmowerPattern == null)
        {
            Debug.LogError("LawnmowerPattern not available for testing");
            return;
        }
        
        // Test with optimization disabled
        lawnmowerPattern.SetCoverageOptimization(false);
        List<Vector3> waypointsNoOpt = lawnmowerPattern.GenerateWaypoints();
        float coverageNoOpt = lawnmowerPattern.CalculateCoverage();
        
        Debug.Log($"Without optimization: {waypointsNoOpt.Count} waypoints, {coverageNoOpt:F1}% coverage");
        
        // Test with optimization enabled
        lawnmowerPattern.SetCoverageOptimization(true);
        List<Vector3> waypointsOpt = lawnmowerPattern.GenerateWaypoints();
        float coverageOpt = lawnmowerPattern.CalculateCoverage();
        
        Debug.Log($"With optimization: {waypointsOpt.Count} waypoints, {coverageOpt:F1}% coverage");
        
        // Test different overlap percentages
        float[] testOverlaps = { 0f, 5f, 10f, 20f, 30f };
        
        foreach (float overlap in testOverlaps)
        {
            lawnmowerPattern.SetOverlapPercentage(overlap);
            List<Vector3> waypoints = lawnmowerPattern.GenerateWaypoints();
            float coverage = lawnmowerPattern.CalculateCoverage();
            Debug.Log($"Overlap {overlap}%: {waypoints.Count} waypoints, {coverage:F1}% coverage");
        }
        
        // Reset to original overlap
        lawnmowerPattern.SetOverlapPercentage(testOverlap);
    }
    
    /// <summary>
    /// Context menu method to run pattern generation test
    /// </summary>
    [ContextMenu("Test Pattern Generation")]
    public void TestPatternGenerationOnly()
    {
        TestComponentReferences();
        TestPatternGeneration();
    }
    
    /// <summary>
    /// Context menu method to test coverage calculation
    /// </summary>
    [ContextMenu("Test Coverage Calculation")]
    public void TestCoverageCalculationOnly()
    {
        TestComponentReferences();
        TestCoverageCalculation();
    }
    
    /// <summary>
    /// Context menu method to execute the pattern
    /// </summary>
    [ContextMenu("Execute Lawnmower Pattern")]
    public void ExecuteLawnmowerPattern()
    {
        TestComponentReferences();
        
        if (lawnmowerPattern != null)
        {
            Debug.Log("[LawnmowerPatternTest] Executing lawnmower pattern...");
            lawnmowerPattern.ExecutePattern();
        }
    }
    
    /// <summary>
    /// Context menu method to clear pattern
    /// </summary>
    [ContextMenu("Clear Pattern")]
    public void ClearPattern()
    {
        if (lawnmowerPattern != null)
        {
            Debug.Log("[LawnmowerPatternTest] Clearing pattern...");
            lawnmowerPattern.ClearWaypoints();
        }
        
        if (waypointQueue != null)
        {
            waypointQueue.ClearWaypoints();
        }
    }
}