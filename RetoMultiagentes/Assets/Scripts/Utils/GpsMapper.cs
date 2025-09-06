using UnityEngine;

/// <summary>
/// Utility class for converting GPS coordinates to Unity world positions
/// Uses simple equirectangular projection without Earth curvature corrections
/// </summary>
public static class GpsMapper
{
    // Earth's radius and circumference constants
    private const float EARTH_RADIUS_METERS = 6378137f; // WGS84 equatorial radius
    private const float METERS_PER_DEGREE_LAT = 111320f; // Approximately 111.32 km per degree of latitude
    
    /// <summary>
    /// Converts GPS coordinates to Unity world position relative to a GPS origin
    /// </summary>
    /// <param name="gpsCoords">GPS coordinates (latitude, longitude) to convert</param>
    /// <param name="originGps">GPS origin coordinates (latitude, longitude)</param>
    /// <returns>Unity world position as Vector3 (X=East, Z=North, Y=0)</returns>
    public static Vector3 GpsToUnityPosition(Vector2 gpsCoords, Vector2 originGps)
    {
        return GpsToUnityPosition(gpsCoords.x, gpsCoords.y, originGps.x, originGps.y);
    }
    
    /// <summary>
    /// Converts GPS coordinates to Unity world position relative to a GPS origin
    /// </summary>
    /// <param name="latitude">Target latitude in degrees</param>
    /// <param name="longitude">Target longitude in degrees</param>
    /// <param name="originLatitude">Origin latitude in degrees</param>
    /// <param name="originLongitude">Origin longitude in degrees</param>
    /// <returns>Unity world position as Vector3 (X=East, Z=North, Y=0)</returns>
    public static Vector3 GpsToUnityPosition(float latitude, float longitude, float originLatitude, float originLongitude)
    {
        // Calculate difference in degrees
        float deltaLat = latitude - originLatitude;
        float deltaLon = longitude - originLongitude;
        
        // Convert latitude difference to meters (constant conversion)
        float northMeters = deltaLat * METERS_PER_DEGREE_LAT;
        
        // Convert longitude difference to meters (adjusted by cosine of latitude)
        float avgLatitudeRad = Mathf.Deg2Rad * (latitude + originLatitude) * 0.5f;
        float eastMeters = deltaLon * METERS_PER_DEGREE_LAT * Mathf.Cos(avgLatitudeRad);
        
        // Return Unity position: X = East, Z = North, Y = 0 (ground level)
        return new Vector3(eastMeters, 0f, northMeters);
    }
    
    /// <summary>
    /// Converts Unity world position back to GPS coordinates relative to a GPS origin
    /// </summary>
    /// <param name="unityPosition">Unity world position</param>
    /// <param name="originGps">GPS origin coordinates (latitude, longitude)</param>
    /// <returns>GPS coordinates as Vector2 (latitude, longitude)</returns>
    public static Vector2 UnityPositionToGps(Vector3 unityPosition, Vector2 originGps)
    {
        return UnityPositionToGps(unityPosition, originGps.x, originGps.y);
    }
    
    /// <summary>
    /// Converts Unity world position back to GPS coordinates relative to a GPS origin
    /// </summary>
    /// <param name="unityPosition">Unity world position</param>
    /// <param name="originLatitude">Origin latitude in degrees</param>
    /// <param name="originLongitude">Origin longitude in degrees</param>
    /// <returns>GPS coordinates as Vector2 (latitude, longitude)</returns>
    public static Vector2 UnityPositionToGps(Vector3 unityPosition, float originLatitude, float originLongitude)
    {
        // Extract meters from Unity position
        float eastMeters = unityPosition.x;
        float northMeters = unityPosition.z;
        
        // Convert north meters back to latitude degrees
        float deltaLat = northMeters / METERS_PER_DEGREE_LAT;
        float latitude = originLatitude + deltaLat;
        
        // Convert east meters back to longitude degrees (adjusted by cosine of latitude)
        float avgLatitudeRad = Mathf.Deg2Rad * (latitude + originLatitude) * 0.5f;
        float deltaLon = eastMeters / (METERS_PER_DEGREE_LAT * Mathf.Cos(avgLatitudeRad));
        float longitude = originLongitude + deltaLon;
        
        return new Vector2(latitude, longitude);
    }
    
    /// <summary>
    /// Calculates the distance between two GPS coordinates in meters
    /// </summary>
    /// <param name="gps1">First GPS coordinate (latitude, longitude)</param>
    /// <param name="gps2">Second GPS coordinate (latitude, longitude)</param>
    /// <returns>Distance in meters</returns>
    public static float CalculateGpsDistance(Vector2 gps1, Vector2 gps2)
    {
        Vector3 unityPos1 = GpsToUnityPosition(gps1, Vector2.zero);
        Vector3 unityPos2 = GpsToUnityPosition(gps2, Vector2.zero);
        return Vector3.Distance(unityPos1, unityPos2);
    }
    
    /// <summary>
    /// Validates if GPS coordinates are within reasonable bounds
    /// </summary>
    /// <param name="gpsCoords">GPS coordinates to validate</param>
    /// <returns>True if coordinates are valid</returns>
    public static bool IsValidGpsCoordinates(Vector2 gpsCoords)
    {
        return gpsCoords.x >= -90f && gpsCoords.x <= 90f &&    // Latitude bounds
               gpsCoords.y >= -180f && gpsCoords.y <= 180f;    // Longitude bounds
    }
}