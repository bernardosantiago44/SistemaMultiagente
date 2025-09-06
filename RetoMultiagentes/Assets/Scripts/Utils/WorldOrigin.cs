using UnityEngine;

/// <summary>
/// ScriptableObject to store GPS origin coordinates for world mapping
/// </summary>
[CreateAssetMenu(fileName = "WorldOrigin", menuName = "GPS/World Origin", order = 1)]
public class WorldOrigin : ScriptableObject
{
    [Header("GPS Origin Coordinates")]
    [SerializeField] private Vector2 originGps = new Vector2(40.7128f, -74.0060f); // Default to NYC
    
    [Header("Metadata")]
    [SerializeField] private string locationName = "New York City";
    [SerializeField] private string description = "GPS origin point for world coordinate mapping";
    
    /// <summary>
    /// GPS origin coordinates (latitude, longitude)
    /// </summary>
    public Vector2 OriginGps 
    { 
        get => originGps; 
        set => originGps = value; 
    }
    
    /// <summary>
    /// Origin latitude in degrees
    /// </summary>
    public float OriginLatitude 
    { 
        get => originGps.x; 
        set => originGps.x = value; 
    }
    
    /// <summary>
    /// Origin longitude in degrees
    /// </summary>
    public float OriginLongitude 
    { 
        get => originGps.y; 
        set => originGps.y = value; 
    }
    
    /// <summary>
    /// Human-readable location name
    /// </summary>
    public string LocationName 
    { 
        get => locationName; 
        set => locationName = value; 
    }
    
    /// <summary>
    /// Description of this origin point
    /// </summary>
    public string Description 
    { 
        get => description; 
        set => description = value; 
    }
    
    /// <summary>
    /// Validates if the GPS coordinates are within reasonable bounds
    /// </summary>
    /// <returns>True if coordinates are valid</returns>
    public bool IsValid()
    {
        return GpsMapper.IsValidGpsCoordinates(originGps);
    }
    
    /// <summary>
    /// Returns a formatted string representation of the origin
    /// </summary>
    /// <returns>Origin location and GPS coordinates</returns>
    public override string ToString()
    {
        return $"{locationName}: ({OriginLatitude:F6}, {OriginLongitude:F6})";
    }
    
    /// <summary>
    /// Converts GPS coordinates to Unity position using this origin
    /// </summary>
    /// <param name="gpsCoords">GPS coordinates to convert</param>
    /// <returns>Unity world position</returns>
    public Vector3 GpsToUnityPosition(Vector2 gpsCoords)
    {
        return GpsMapper.GpsToUnityPosition(gpsCoords, originGps);
    }
    
    /// <summary>
    /// Converts Unity position back to GPS coordinates using this origin
    /// </summary>
    /// <param name="unityPosition">Unity world position</param>
    /// <returns>GPS coordinates</returns>
    public Vector2 UnityPositionToGps(Vector3 unityPosition)
    {
        return GpsMapper.UnityPositionToGps(unityPosition, originGps);
    }
}