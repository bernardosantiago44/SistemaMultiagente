using UnityEngine;

/// <summary>
/// Simple component to demonstrate Mission system functionality in Unity
/// Attach this to a GameObject in your scene to test the mission loading
/// </summary>
public class MissionDemo : MonoBehaviour
{
    [Header("Mission Demo Settings")]
    [SerializeField] private bool autoLoadOnStart = true;
    [SerializeField] private MissionManager missionManager;
    
    void Start()
    {
        // Subscribe to mission loaded events
        MissionManager.OnMissionLoaded += OnMissionLoadedHandler;
        
        // If no mission manager is assigned, try to find one
        if (missionManager == null)
        {
            missionManager = FindFirstObjectByType<MissionManager>();
        }
        
        // Create a mission manager if none exists
        if (missionManager == null && autoLoadOnStart)
        {
            GameObject managerObj = new GameObject("MissionManager");
            missionManager = managerObj.AddComponent<MissionManager>();
            Debug.Log("Created MissionManager automatically");
        }
        
        if (autoLoadOnStart && missionManager != null)
        {
            Debug.Log("=== Mission Demo Started ===");
            Debug.Log("Attempting to load mission from JSON file...");
        }
    }
    
    void OnDestroy()
    {
        // Unsubscribe from events
        MissionManager.OnMissionLoaded -= OnMissionLoadedHandler;
    }
    
    /// <summary>
    /// Event handler for when a mission is loaded
    /// </summary>
    /// <param name="mission">The loaded mission</param>
    private void OnMissionLoadedHandler(Mission mission)
    {
        Debug.Log("=== MISSION LOADED EVENT RECEIVED ===");
        Debug.Log($"Mission Description: {mission.Description}");
        Debug.Log($"GPS Coordinates: Latitude {mission.Latitude:F6}, Longitude {mission.Longitude:F6}");
        Debug.Log($"Mission Valid: {mission.IsValid()}");
        Debug.Log($"Full Mission Info: {mission}");
        Debug.Log("=====================================");
    }
    
    /// <summary>
    /// Public method to manually load mission (can be called from Inspector)
    /// </summary>
    [ContextMenu("Load Mission")]
    public void LoadMission()
    {
        if (missionManager != null)
        {
            Debug.Log("Manually loading mission...");
            missionManager.LoadMissionFromJson();
        }
        else
        {
            Debug.LogError("No MissionManager assigned!");
        }
    }
    
    /// <summary>
    /// Test method to create and load a mission manually
    /// </summary>
    [ContextMenu("Test Manual Mission")]
    public void TestManualMission()
    {
        if (missionManager != null)
        {
            Mission testMission = new Mission(
                "Manual test mission created in Unity", 
                25.7617f,  // Dubai latitude
                -80.1918f  // Miami longitude (mixed up intentionally for demo)
            );
            
            Debug.Log("Setting manual mission...");
            missionManager.SetMission(testMission);
        }
        else
        {
            Debug.LogError("No MissionManager assigned!");
        }
    }
    
    /// <summary>
    /// Test JSON string loading
    /// </summary>
    [ContextMenu("Test JSON String")]
    public void TestJsonString()
    {
        if (missionManager != null)
        {
            string testJson = @"{
                ""description"": ""JSON String Test Mission - Patrol perimeter around target zone"",
                ""latitude"": 40.748817,
                ""longitude"": -73.985428
            }";
            
            Debug.Log("Loading mission from JSON string...");
            missionManager.LoadMissionFromJsonString(testJson);
        }
        else
        {
            Debug.LogError("No MissionManager assigned!");
        }
    }
}