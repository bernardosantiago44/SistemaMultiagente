using UnityEngine;

/// <summary>
/// Test script to validate GPS mapping functionality and round-trip conversions
/// </summary>
public class GpsMapperTest : MonoBehaviour
{
    [Header("Test Configuration")]
    [SerializeField] private bool runTestsOnStart = true;
    [SerializeField] private WorldOrigin worldOrigin;
    
    [Header("Test GPS Coordinates")]
    [SerializeField] private Vector2 testGpsCoords = new Vector2(40.758896f, -73.985130f); // Times Square NYC
    
    void Start()
    {
        if (runTestsOnStart)
        {
            RunTests();
        }
    }
    
    /// <summary>
    /// Runs all GPS mapping tests
    /// </summary>
    public void RunTests()
    {
        Debug.Log("[GpsMapperTest] Starting GPS Mapper Tests...");
        
        TestBasicConversion();
        TestRoundTripConversion();
        TestDistanceCalculation();
        TestValidation();
        TestWorldOriginIntegration();
        
        Debug.Log("[GpsMapperTest] GPS Mapper Tests Completed");
    }
    
    /// <summary>
    /// Tests basic GPS to Unity position conversion
    /// </summary>
    private void TestBasicConversion()
    {
        Debug.Log("[GpsMapperTest] Testing Basic GPS Conversion...");
        
        // Test GPS origin (should map to Unity origin)
        Vector2 origin = new Vector2(40.7128f, -74.0060f); // NYC
        Vector3 originUnity = GpsMapper.GpsToUnityPosition(origin, origin);
        Debug.Log($"Origin GPS {origin} -> Unity {originUnity}");
        
        // Test point 1 degree north
        Vector2 northPoint = new Vector2(41.7128f, -74.0060f);
        Vector3 northUnity = GpsMapper.GpsToUnityPosition(northPoint, origin);
        Debug.Log($"1° North GPS {northPoint} -> Unity {northUnity}");
        Debug.Log($"1° Latitude = {northUnity.z:F1} meters north (expected ~111,320m)");
        
        // Test point 1 degree east
        Vector2 eastPoint = new Vector2(40.7128f, -73.0060f);
        Vector3 eastUnity = GpsMapper.GpsToUnityPosition(eastPoint, origin);
        Debug.Log($"1° East GPS {eastPoint} -> Unity {eastUnity}");
        Debug.Log($"1° Longitude = {eastUnity.x:F1} meters east (expected ~85,000m at NYC latitude)");
    }
    
    /// <summary>
    /// Tests round-trip conversion accuracy (GPS -> Unity -> GPS)
    /// </summary>
    private void TestRoundTripConversion()
    {
        Debug.Log("[GpsMapperTest] Testing Round-Trip Conversion...");
        
        Vector2 origin = new Vector2(40.7128f, -74.0060f); // NYC
        Vector2 originalGps = testGpsCoords;
        
        // GPS -> Unity -> GPS
        Vector3 unityPos = GpsMapper.GpsToUnityPosition(originalGps, origin);
        Vector2 convertedGps = GpsMapper.UnityPositionToGps(unityPos, origin);
        
        Debug.Log($"Original GPS: {originalGps}");
        Debug.Log($"Unity Position: {unityPos}");
        Debug.Log($"Converted Back GPS: {convertedGps}");
        
        // Check accuracy
        float latError = Mathf.Abs(originalGps.x - convertedGps.x);
        float lonError = Mathf.Abs(originalGps.y - convertedGps.y);
        
        Debug.Log($"Round-trip error: Lat={latError:F8}°, Lon={lonError:F8}°");
        
        bool roundTripSuccess = latError < 0.000001f && lonError < 0.000001f;
        Debug.Log($"Round-trip test: {(roundTripSuccess ? "PASSED" : "FAILED")}");
    }
    
    /// <summary>
    /// Tests distance calculation functionality
    /// </summary>
    private void TestDistanceCalculation()
    {
        Debug.Log("[GpsMapperTest] Testing Distance Calculation...");
        
        Vector2 nyc = new Vector2(40.7128f, -74.0060f);
        Vector2 timesSquare = new Vector2(40.758896f, -73.985130f);
        
        float distance = GpsMapper.CalculateGpsDistance(nyc, timesSquare);
        Debug.Log($"Distance from NYC to Times Square: {distance:F1} meters");
        
        // Test with known coordinates
        Vector2 origin = Vector2.zero;
        Vector2 oneDegreeNorth = new Vector2(1f, 0f);
        float oneDegreeDistance = GpsMapper.CalculateGpsDistance(origin, oneDegreeNorth);
        Debug.Log($"Distance for 1° latitude: {oneDegreeDistance:F1} meters (expected ~111,320m)");
    }
    
    /// <summary>
    /// Tests GPS coordinate validation
    /// </summary>
    private void TestValidation()
    {
        Debug.Log("[GpsMapperTest] Testing GPS Validation...");
        
        // Valid coordinates
        Vector2 validGps = new Vector2(40.7128f, -74.0060f);
        Debug.Log($"Valid GPS {validGps}: {GpsMapper.IsValidGpsCoordinates(validGps)}");
        
        // Invalid coordinates
        Vector2 invalidLat = new Vector2(91f, 0f); // Latitude > 90
        Vector2 invalidLon = new Vector2(0f, 181f); // Longitude > 180
        
        Debug.Log($"Invalid Lat {invalidLat}: {GpsMapper.IsValidGpsCoordinates(invalidLat)}");
        Debug.Log($"Invalid Lon {invalidLon}: {GpsMapper.IsValidGpsCoordinates(invalidLon)}");
    }
    
    /// <summary>
    /// Tests integration with WorldOrigin ScriptableObject
    /// </summary>
    private void TestWorldOriginIntegration()
    {
        Debug.Log("[GpsMapperTest] Testing WorldOrigin Integration...");
        
        if (worldOrigin == null)
        {
            Debug.LogWarning("WorldOrigin asset not assigned - skipping integration test");
            return;
        }
        
        Debug.Log($"World Origin: {worldOrigin}");
        Debug.Log($"Origin Valid: {worldOrigin.IsValid()}");
        
        // Test conversion using WorldOrigin
        Vector3 unityPos = worldOrigin.GpsToUnityPosition(testGpsCoords);
        Vector2 backToGps = worldOrigin.UnityPositionToGps(unityPos);
        
        Debug.Log($"Test GPS {testGpsCoords} -> Unity {unityPos} -> GPS {backToGps}");
        
        float error = Vector2.Distance(testGpsCoords, backToGps);
        Debug.Log($"WorldOrigin round-trip error: {error:F8}° ({(error < 0.000001f ? "PASSED" : "FAILED")})");
    }
}