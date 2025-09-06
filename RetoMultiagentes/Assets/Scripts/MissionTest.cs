using UnityEngine;

/// <summary>
/// Test script to demonstrate and verify Mission and MissionManager functionality
/// </summary>
public class MissionTest : MonoBehaviour
{
    [Header("Test Configuration")]
    [SerializeField] private bool runTestsOnStart = true;
    
    void Start()
    {
        if (runTestsOnStart)
        {
            RunTests();
        }
        
        // Subscribe to mission loaded events
        MissionManager.OnMissionLoaded += OnMissionLoaded;
    }
    
    void OnDestroy()
    {
        // Unsubscribe from events
        MissionManager.OnMissionLoaded -= OnMissionLoaded;
    }
    
    /// <summary>
    /// Event handler for when a mission is loaded
    /// </summary>
    /// <param name="mission">The loaded mission</param>
    private void OnMissionLoaded(Mission mission)
    {
        Debug.Log($"[MissionTest] Mission Loaded Event Received: {mission}");
    }
    
    /// <summary>
    /// Runs various tests for Mission and MissionManager functionality
    /// </summary>
    public void RunTests()
    {
        Debug.Log("[MissionTest] Starting Mission System Tests...");
        
        TestMissionCreation();
        TestMissionValidation();
        TestJsonParsing();
        
        Debug.Log("[MissionTest] Mission System Tests Completed");
    }
    
    /// <summary>
    /// Tests mission creation and properties
    /// </summary>
    private void TestMissionCreation()
    {
        Debug.Log("[MissionTest] Testing Mission Creation...");
        
        // Test default constructor
        Mission mission1 = new Mission();
        Debug.Log($"Default Mission: {mission1}");
        
        // Test parameterized constructor
        Mission mission2 = new Mission("Test mission with coordinates", 40.7128f, -74.0060f);
        Debug.Log($"Parameterized Mission: {mission2}");
        
        // Test Vector2 constructor
        Mission mission3 = new Mission("Vector2 mission", new Vector2(51.5074f, -0.1278f));
        Debug.Log($"Vector2 Mission: {mission3}");
        
        // Test property access
        mission1.Description = "Updated description";
        mission1.Latitude = 35.6762f;
        mission1.Longitude = 139.6503f;
        Debug.Log($"Updated Mission: {mission1}");
    }
    
    /// <summary>
    /// Tests mission validation
    /// </summary>
    private void TestMissionValidation()
    {
        Debug.Log("[MissionTest] Testing Mission Validation...");
        
        // Test valid mission
        Mission validMission = new Mission("Valid mission description", 0, 0);
        Debug.Log($"Valid Mission: {validMission.IsValid()} - {validMission}");
        
        // Test invalid missions
        Mission invalidMission1 = new Mission("", 0, 0);
        Debug.Log($"Empty Description Mission Valid: {invalidMission1.IsValid()}");
        
        Mission invalidMission2 = new Mission(null, 0, 0);
        Debug.Log($"Null Description Mission Valid: {invalidMission2.IsValid()}");
        
        Mission invalidMission3 = new Mission("   ", 0, 0);
        Debug.Log($"Whitespace Description Mission Valid: {invalidMission3.IsValid()}");
    }
    
    /// <summary>
    /// Tests JSON parsing functionality
    /// </summary>
    private void TestJsonParsing()
    {
        Debug.Log("[MissionTest] Testing JSON Parsing...");
        
        // Test valid JSON
        string validJson = @"{
            ""description"": ""JSON Test Mission"",
            ""latitude"": 48.8566,
            ""longitude"": 2.3522
        }";
        
        // Find MissionManager in scene
        MissionManager missionManager = FindFirstObjectByType<MissionManager>();
        if (missionManager != null)
        {
            missionManager.LoadMissionFromJsonString(validJson);
        }
        else
        {
            Debug.LogWarning("[MissionTest] No MissionManager found in scene for JSON test");
        }
        
        // Test invalid JSON
        string invalidJson = @"{
            ""description"": """",
            ""latitude"": 0,
            ""longitude"": 0
        }";
        
        if (missionManager != null)
        {
            missionManager.LoadMissionFromJsonString(invalidJson);
        }
    }
}