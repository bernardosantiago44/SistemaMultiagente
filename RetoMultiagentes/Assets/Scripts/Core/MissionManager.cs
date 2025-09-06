using System;
using System.IO;
using UnityEngine;

public class MissionManager : MonoBehaviour
{
    [Header("Mission Configuration")]
    [SerializeField] private string missionJsonPath = "Assets/Config/sample_mission.json";
    [SerializeField] private Mission currentMission;
    
    // Event for when a mission is loaded
    public static event Action<Mission> OnMissionLoaded;
    
    [Header("Debug")]
    [SerializeField] private bool loadOnStart = true;
    [SerializeField] private bool logToConsole = true;
    [SerializeField] private bool verboseLogging = false;
    
    void Start()
    {
        if (loadOnStart)
        {
            LoadMissionFromJson();
        }
    }
    
    /// <summary>
    /// Loads mission from JSON file
    /// </summary>
    public void LoadMissionFromJson()
    {
        try
        {
            // Try different path strategies for Unity
            string fullPath;
            
            // Strategy 1: Relative to project root
            fullPath = Path.Combine(Application.dataPath, "..", missionJsonPath);
            
            // Strategy 2: If first doesn't work, try relative to Assets folder
            if (!File.Exists(fullPath))
            {
                string assetPath = missionJsonPath.Replace("Assets/", "");
                fullPath = Path.Combine(Application.dataPath, assetPath);
            }
            
            // Strategy 3: Direct path in streaming assets (for builds)
            if (!File.Exists(fullPath))
            {
                fullPath = Path.Combine(Application.streamingAssetsPath, "sample_mission.json");
            }
            
            if (!File.Exists(fullPath))
            {
                if (verboseLogging)
                {
                    Debug.LogError($"Mission file not found. Tried paths:");
                    Debug.LogError($"  1. {Path.Combine(Application.dataPath, "..", missionJsonPath)}");
                    Debug.LogError($"  2. {Path.Combine(Application.dataPath, missionJsonPath.Replace("Assets/", ""))}");
                    Debug.LogError($"  3. {Path.Combine(Application.streamingAssetsPath, "sample_mission.json")}");
                }
                else
                {
                    Debug.LogError($"Mission file not found at any expected location. Enable 'Verbose Logging' for details.");
                }
                return;
            }
            
            if (verboseLogging)
            {
                Debug.Log($"Loading mission from: {fullPath}");
            }
            
            string jsonContent = File.ReadAllText(fullPath);
            
            if (string.IsNullOrEmpty(jsonContent))
            {
                Debug.LogError("Mission file is empty");
                return;
            }
            
            MissionData missionData = JsonUtility.FromJson<MissionData>(jsonContent);
            
            if (missionData == null)
            {
                Debug.LogError("Failed to parse mission JSON");
                return;
            }
            
            // Create mission from loaded data
            currentMission = new Mission(missionData.description, missionData.latitude, missionData.longitude);
            
            // Validate mission
            if (!currentMission.IsValid())
            {
                Debug.LogError("Loaded mission is invalid - missing required fields");
                return;
            }
            
            // Log to console if enabled
            if (logToConsole)
            {
                Debug.Log($"Mission Loaded: {currentMission}");
            }
            
            // Publish mission loaded event
            OnMissionLoaded?.Invoke(currentMission);
            
            Debug.Log("Mission successfully loaded and validated");
        }
        catch (Exception ex)
        {
            Debug.LogError($"Error loading mission: {ex.Message}");
        }
    }
    
    /// <summary>
    /// Loads mission from provided JSON string
    /// </summary>
    /// <param name="jsonString">JSON string containing mission data</param>
    public void LoadMissionFromJsonString(string jsonString)
    {
        try
        {
            if (string.IsNullOrEmpty(jsonString))
            {
                Debug.LogError("JSON string is null or empty");
                return;
            }
            
            MissionData missionData = JsonUtility.FromJson<MissionData>(jsonString);
            
            if (missionData == null)
            {
                Debug.LogError("Failed to parse mission JSON string");
                return;
            }
            
            currentMission = new Mission(missionData.description, missionData.latitude, missionData.longitude);
            
            if (!currentMission.IsValid())
            {
                Debug.LogError("Parsed mission is invalid - missing required fields");
                return;
            }
            
            if (logToConsole)
            {
                Debug.Log($"Mission Loaded from String: {currentMission}");
            }
            
            OnMissionLoaded?.Invoke(currentMission);
            
            Debug.Log("Mission successfully loaded from JSON string");
        }
        catch (Exception ex)
        {
            Debug.LogError($"Error loading mission from JSON string: {ex.Message}");
        }
    }
    
    /// <summary>
    /// Sets a mission directly
    /// </summary>
    /// <param name="mission">Mission to set</param>
    public void SetMission(Mission mission)
    {
        if (mission == null)
        {
            Debug.LogError("Cannot set null mission");
            return;
        }
        
        if (!mission.IsValid())
        {
            Debug.LogError("Cannot set invalid mission - missing required fields");
            return;
        }
        
        currentMission = mission;
        
        if (logToConsole)
        {
            Debug.Log($"Mission Set: {currentMission}");
        }
        
        OnMissionLoaded?.Invoke(currentMission);
    }
    
    /// <summary>
    /// Gets the current mission
    /// </summary>
    /// <returns>Current mission or null if none loaded</returns>
    public Mission GetCurrentMission()
    {
        return currentMission;
    }
    
    /// <summary>
    /// Checks if a mission is currently loaded
    /// </summary>
    /// <returns>True if mission is loaded and valid</returns>
    public bool HasValidMission()
    {
        return currentMission != null && currentMission.IsValid();
    }
}

/// <summary>
/// Data structure for JSON serialization of mission data
/// </summary>
[Serializable]
public class MissionData
{
    public string description;
    public float latitude;
    public float longitude;
}