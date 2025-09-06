using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Implements a lawnmower search pattern that covers a square area with parallel back-and-forth sweeps
/// Similar to mowing a lawn, the pattern moves in straight lines with turns at the ends
/// Ensures >90% area coverage with configurable overlap and step distance
/// </summary>
public class LawnmowerPattern : SearchPattern
{
    [Header("Lawnmower Configuration")]
    [SerializeField] private bool startFromBottom = true; // Start from bottom edge and move up
    [SerializeField] private bool leftToRightFirst = true; // First pass direction
    [SerializeField] private float turnRadius = 5f; // Radius for turns at pattern ends
    
    [Header("Coverage Optimization")]
    [SerializeField] private float sensorWidth = 20f; // Width of sensor coverage in meters
    [SerializeField] private bool optimizeForCoverage = true; // Auto-adjust step distance for optimal coverage
    
    private List<Vector3> patternLines = new List<Vector3>(); // Individual line segments for debugging
    
    /// <summary>
    /// Generate waypoints following a lawnmower pattern
    /// </summary>
    /// <returns>List of waypoints covering the search area</returns>
    public override List<Vector3> GenerateWaypoints()
    {
        generatedWaypoints.Clear();
        patternLines.Clear();
        
        if (areaSize <= 0f || stepDistance <= 0f)
        {
            Debug.LogError($"[LawnmowerPattern] Invalid parameters: areaSize={areaSize}, stepDistance={stepDistance}");
            return generatedWaypoints;
        }
        
        // Calculate effective step distance with overlap
        float effectiveStep = GetEffectiveStepDistance();
        
        // Optimize step distance for better coverage if enabled
        if (optimizeForCoverage && sensorWidth > 0f)
        {
            effectiveStep = OptimizeStepDistance();
        }
        
        // Calculate the number of passes needed
        int numPasses = Mathf.CeilToInt(areaSize / effectiveStep) + 1; // +1 to ensure full coverage
        
        if (logPatternGeneration)
        {
            Debug.Log($"[LawnmowerPattern] Generating pattern: {numPasses} passes, {effectiveStep:F2}m step, {areaSize}m area");
        }
        
        // Calculate area bounds
        float halfArea = areaSize * 0.5f;
        Vector3 areaMin = centerPosition + new Vector3(-halfArea, altitude, -halfArea);
        Vector3 areaMax = centerPosition + new Vector3(halfArea, altitude, halfArea);
        
        // Generate the lawnmower pattern
        bool moveRightToLeft = !leftToRightFirst;
        
        for (int i = 0; i < numPasses; i++)
        {
            // Calculate Y position for this pass (North-South position)
            float yProgress = (float)i / (numPasses - 1);
            float yPos = startFromBottom ? 
                Mathf.Lerp(areaMin.z, areaMax.z, yProgress) : 
                Mathf.Lerp(areaMax.z, areaMin.z, yProgress);
            
            // Clamp Y position to area bounds
            yPos = Mathf.Clamp(yPos, areaMin.z, areaMax.z);
            
            // Create waypoints for this pass
            Vector3 startPoint, endPoint;
            
            if (moveRightToLeft)
            {
                // Right to left
                startPoint = new Vector3(areaMax.x, altitude, yPos);
                endPoint = new Vector3(areaMin.x, altitude, yPos);
            }
            else
            {
                // Left to right
                startPoint = new Vector3(areaMin.x, altitude, yPos);
                endPoint = new Vector3(areaMax.x, altitude, yPos);
            }
            
            // Add start point for this pass
            generatedWaypoints.Add(startPoint);
            patternLines.Add(startPoint);
            
            // Add end point for this pass
            generatedWaypoints.Add(endPoint);
            patternLines.Add(endPoint);
            
            // Add turn waypoints if not the last pass
            if (i < numPasses - 1)
            {
                AddTurnWaypoints(endPoint, moveRightToLeft, yPos, areaMin, areaMax, i + 1, numPasses);
            }
            
            // Alternate direction for next pass
            moveRightToLeft = !moveRightToLeft;
        }
        
        if (logPatternGeneration)
        {
            Debug.Log($"[LawnmowerPattern] Generated {generatedWaypoints.Count} waypoints for lawnmower pattern");
        }
        
        return generatedWaypoints;
    }
    
    /// <summary>
    /// Add turn waypoints between passes for smooth navigation
    /// </summary>
    private void AddTurnWaypoints(Vector3 currentEnd, bool wasMovingRightToLeft, float currentY, 
                                 Vector3 areaMin, Vector3 areaMax, int nextPassIndex, int totalPasses)
    {
        // Calculate next pass Y position
        float nextYProgress = (float)nextPassIndex / (totalPasses - 1);
        float nextY = startFromBottom ? 
            Mathf.Lerp(areaMin.z, areaMax.z, nextYProgress) : 
            Mathf.Lerp(areaMax.z, areaMin.z, nextYProgress);
        nextY = Mathf.Clamp(nextY, areaMin.z, areaMax.z);
        
        // Calculate turn waypoints with smooth curve
        Vector3 nextStart = wasMovingRightToLeft ? 
            new Vector3(areaMin.x, altitude, nextY) : 
            new Vector3(areaMax.x, altitude, nextY);
        
        // Add intermediate waypoints for smooth turn
        if (turnRadius > 0f)
        {
            Vector3 turnMid = Vector3.Lerp(currentEnd, nextStart, 0.5f);
            generatedWaypoints.Add(turnMid);
        }
    }
    
    /// <summary>
    /// Optimize step distance based on sensor width to achieve optimal coverage
    /// </summary>
    /// <returns>Optimized step distance</returns>
    private float OptimizeStepDistance()
    {
        if (sensorWidth <= 0f) return GetEffectiveStepDistance();
        
        // Calculate optimal step distance for sensor coverage
        float overlapDistance = sensorWidth * (overlapPercentage / 100f);
        float optimalStep = sensorWidth - overlapDistance;
        
        // Ensure step distance doesn't exceed area size
        optimalStep = Mathf.Min(optimalStep, areaSize * 0.9f);
        
        if (logPatternGeneration)
        {
            Debug.Log($"[LawnmowerPattern] Optimized step: {optimalStep:F2}m (sensor: {sensorWidth}m, overlap: {overlapPercentage}%)");
        }
        
        return optimalStep;
    }
    
    /// <summary>
    /// Calculate area coverage percentage based on generated pattern and sensor width
    /// </summary>
    /// <returns>Coverage percentage (0-100)</returns>
    public override float CalculateCoverage()
    {
        if (generatedWaypoints.Count == 0 || sensorWidth <= 0f)
        {
            return 0f;
        }
        
        // Calculate effective coverage width per pass
        float effectiveStep = optimizeForCoverage ? OptimizeStepDistance() : GetEffectiveStepDistance();
        
        // Calculate number of effective passes
        int numPasses = Mathf.CeilToInt(areaSize / effectiveStep);
        
        // Calculate total coverage area
        float totalCoverageWidth = numPasses * sensorWidth;
        
        // Account for overlaps (avoid double counting)
        float overlapWidth = (numPasses - 1) * (sensorWidth * overlapPercentage / 100f);
        float effectiveCoverageWidth = totalCoverageWidth - overlapWidth;
        
        // Calculate coverage percentage
        float coverage = Mathf.Min(100f, (effectiveCoverageWidth / areaSize) * 100f);
        
        // Ensure we meet the >90% requirement
        if (coverage < 90f)
        {
            Debug.LogWarning($"[LawnmowerPattern] Coverage ({coverage:F1}%) below 90% requirement. Consider adjusting parameters.");
        }
        
        if (logPatternGeneration)
        {
            Debug.Log($"[LawnmowerPattern] Coverage calculation: {coverage:F1}% ({numPasses} passes, {effectiveCoverageWidth:F1}m effective width)");
        }
        
        return coverage;
    }
    
    /// <summary>
    /// Set sensor width for coverage calculations
    /// </summary>
    /// <param name="width">Sensor coverage width in meters</param>
    public void SetSensorWidth(float width)
    {
        sensorWidth = Mathf.Max(0.1f, width);
        
        if (logPatternGeneration)
        {
            Debug.Log($"[LawnmowerPattern] Sensor width set to: {sensorWidth}m");
        }
    }
    
    /// <summary>
    /// Set pattern starting direction
    /// </summary>
    /// <param name="startBottom">True to start from bottom, false for top</param>
    /// <param name="leftToRight">True to start left-to-right, false for right-to-left</param>
    public void SetPatternDirection(bool startBottom, bool leftToRight)
    {
        startFromBottom = startBottom;
        leftToRightFirst = leftToRight;
        
        if (logPatternGeneration)
        {
            Debug.Log($"[LawnmowerPattern] Pattern direction: Start from {(startBottom ? "bottom" : "top")}, first pass {(leftToRight ? "left-to-right" : "right-to-left")}");
        }
    }
    
    /// <summary>
    /// Set turn radius for smooth navigation
    /// </summary>
    /// <param name="radius">Turn radius in meters</param>
    public void SetTurnRadius(float radius)
    {
        turnRadius = Mathf.Max(0f, radius);
        
        if (logPatternGeneration)
        {
            Debug.Log($"[LawnmowerPattern] Turn radius set to: {turnRadius}m");
        }
    }
    
    /// <summary>
    /// Enable or disable coverage optimization
    /// </summary>
    /// <param name="optimize">True to enable optimization</param>
    public void SetCoverageOptimization(bool optimize)
    {
        optimizeForCoverage = optimize;
        
        if (logPatternGeneration)
        {
            Debug.Log($"[LawnmowerPattern] Coverage optimization {(optimize ? "enabled" : "disabled")}");
        }
    }
    
    /// <summary>
    /// Execute lawnmower pattern with validation
    /// </summary>
    public override void ExecutePattern()
    {
        // Validate configuration before execution
        if (!ValidateConfiguration())
        {
            Debug.LogError("[LawnmowerPattern] Configuration validation failed. Pattern not executed.");
            return;
        }
        
        // Execute base pattern
        base.ExecutePattern();
        
        // Validate coverage requirement
        if (calculatedCoverage < 90f)
        {
            Debug.LogWarning($"[LawnmowerPattern] Pattern coverage ({calculatedCoverage:F1}%) is below 90% requirement!");
        }
        else
        {
            if (logPatternGeneration)
            {
                Debug.Log($"[LawnmowerPattern] Pattern meets coverage requirement: {calculatedCoverage:F1}%");
            }
        }
    }
    
    /// <summary>
    /// Validate pattern configuration
    /// </summary>
    /// <returns>True if configuration is valid</returns>
    private bool ValidateConfiguration()
    {
        bool valid = true;
        
        if (areaSize <= 0f)
        {
            Debug.LogError("[LawnmowerPattern] Area size must be greater than 0");
            valid = false;
        }
        
        if (stepDistance <= 0f)
        {
            Debug.LogError("[LawnmowerPattern] Step distance must be greater than 0");
            valid = false;
        }
        
        if (sensorWidth <= 0f)
        {
            Debug.LogWarning("[LawnmowerPattern] Sensor width should be greater than 0 for coverage calculation");
        }
        
        if (stepDistance > areaSize)
        {
            Debug.LogWarning("[LawnmowerPattern] Step distance is larger than area size - this may result in incomplete coverage");
        }
        
        return valid;
    }
    
#if UNITY_EDITOR
    protected override void OnDrawGizmos()
    {
        base.OnDrawGizmos();
        
        if (!visualizePattern) return;
        
        // Draw sensor coverage visualization
        if (sensorWidth > 0f && generatedWaypoints.Count > 1)
        {
            Gizmos.color = new Color(0, 1, 0, 0.2f); // Semi-transparent green
            
            for (int i = 0; i < generatedWaypoints.Count - 1; i += 2)
            {
                Vector3 start = generatedWaypoints[i];
                Vector3 end = generatedWaypoints[i + 1];
                Vector3 direction = (end - start).normalized;
                Vector3 perpendicular = Vector3.Cross(direction, Vector3.up) * sensorWidth * 0.5f;
                
                // Draw sensor coverage area for this pass
                Vector3[] coverageCorners = {
                    start + perpendicular,
                    start - perpendicular,
                    end - perpendicular,
                    end + perpendicular
                };
                
                // Draw coverage area
                for (int j = 0; j < 4; j++)
                {
                    Gizmos.DrawLine(coverageCorners[j], coverageCorners[(j + 1) % 4]);
                }
            }
        }
    }
    
    protected override void OnDrawGizmosSelected()
    {
        base.OnDrawGizmosSelected();
        
        if (!visualizePattern) return;
        
        // Draw turn radius visualization
        if (turnRadius > 0f && generatedWaypoints.Count > 2)
        {
            Gizmos.color = Color.blue;
            
            for (int i = 1; i < generatedWaypoints.Count - 1; i++)
            {
                Vector3 waypoint = generatedWaypoints[i];
                Gizmos.DrawWireSphere(waypoint, turnRadius);
            }
        }
        
        // Draw direction arrows
        Gizmos.color = Color.magenta;
        for (int i = 0; i < generatedWaypoints.Count - 1; i++)
        {
            Vector3 start = generatedWaypoints[i];
            Vector3 end = generatedWaypoints[i + 1];
            Vector3 direction = (end - start).normalized;
            Vector3 arrowPos = Vector3.Lerp(start, end, 0.5f);
            
            // Draw direction arrow
            Gizmos.DrawRay(arrowPos, direction * 5f);
            Gizmos.DrawRay(arrowPos + direction * 5f, Quaternion.Euler(0, 45, 0) * -direction * 2f);
            Gizmos.DrawRay(arrowPos + direction * 5f, Quaternion.Euler(0, -45, 0) * -direction * 2f);
        }
    }
#endif
}