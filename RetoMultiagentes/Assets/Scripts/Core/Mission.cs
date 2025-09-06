using System;
using UnityEngine;

[Serializable]
public class Mission
{
    [SerializeField] private string description;
    [SerializeField] private Vector2 gpsCoordinates; // Using Vector2 for GPS (latitude, longitude)
    
    public string Description 
    { 
        get => description; 
        set => description = value; 
    }
    
    public Vector2 GpsCoordinates 
    { 
        get => gpsCoordinates; 
        set => gpsCoordinates = value; 
    }
    
    // Latitude property for easier access
    public float Latitude 
    { 
        get => gpsCoordinates.x; 
        set => gpsCoordinates.x = value; 
    }
    
    // Longitude property for easier access
    public float Longitude 
    { 
        get => gpsCoordinates.y; 
        set => gpsCoordinates.y = value; 
    }
    
    public Mission()
    {
        description = string.Empty;
        gpsCoordinates = Vector2.zero;
    }
    
    public Mission(string description, float latitude, float longitude)
    {
        this.description = description;
        this.gpsCoordinates = new Vector2(latitude, longitude);
    }
    
    public Mission(string description, Vector2 gpsCoordinates)
    {
        this.description = description;
        this.gpsCoordinates = gpsCoordinates;
    }
    
    /// <summary>
    /// Validates if the mission has all required fields
    /// </summary>
    /// <returns>True if mission is valid, false otherwise</returns>
    public bool IsValid()
    {
        return !string.IsNullOrEmpty(description) && 
               !string.IsNullOrWhiteSpace(description);
    }
    
    /// <summary>
    /// Returns a formatted string representation of the mission
    /// </summary>
    /// <returns>Mission description and GPS coordinates</returns>
    public override string ToString()
    {
        return $"Mission: {description} | GPS: ({Latitude:F6}, {Longitude:F6})";
    }
}