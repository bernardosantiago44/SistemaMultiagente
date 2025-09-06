# Mission System Documentation

## Overview
The Mission System provides functionality to load missions with textual descriptions and GPS coordinates from JSON files or strings. It includes validation and event publishing capabilities.

## Components

### Mission.cs (POCO)
A Plain Old CLR Object that represents a mission with:
- **Description**: Text description of the mission
- **GPS Coordinates**: Latitude and longitude as Vector2
- **Validation**: Checks for required fields
- **Properties**: Easy access to Latitude and Longitude

### MissionManager.cs
MonoBehaviour that handles mission loading:
- **JSON File Loading**: Loads from `Assets/Config/sample_mission.json` by default
- **JSON String Loading**: Loads from provided JSON string
- **Event Publishing**: Publishes `OnMissionLoaded` event when mission is successfully loaded
- **Validation**: Ensures missions have required fields before loading
- **Console Logging**: Displays mission details in Unity console

### Sample JSON Format
```json
{
  "description": "Mission description here",
  "latitude": 19.432608,
  "longitude": -99.133209
}
```

## Usage

### Basic Setup
1. Add `MissionManager` component to a GameObject in your scene
2. Configure the JSON file path in the Inspector (defaults to `Assets/Config/sample_mission.json`)
3. Enable "Load On Start" to automatically load mission when scene starts
4. Enable "Log To Console" to see mission details in Unity console

### Loading Missions
```csharp
// Get reference to MissionManager
MissionManager missionManager = FindObjectOfType<MissionManager>();

// Load from file
missionManager.LoadMissionFromJson();

// Load from JSON string
string jsonString = "{\"description\":\"Test mission\",\"latitude\":40.7128,\"longitude\":-74.0060}";
missionManager.LoadMissionFromJsonString(jsonString);

// Set mission directly
Mission mission = new Mission("Direct mission", 51.5074f, -0.1278f);
missionManager.SetMission(mission);
```

### Event Handling
```csharp
// Subscribe to mission loaded events
MissionManager.OnMissionLoaded += OnMissionLoaded;

private void OnMissionLoaded(Mission mission)
{
    Debug.Log($"Mission loaded: {mission}");
    Debug.Log($"GPS: {mission.Latitude}, {mission.Longitude}");
}
```

### Mission Validation
```csharp
Mission mission = new Mission("Test", 0, 0);
if (mission.IsValid())
{
    // Mission has valid description
}
```

## Testing
Use the included `MissionDemo.cs` and `MissionTest.cs` components to test functionality:
- **MissionDemo**: Simple Unity component with context menu options
- **MissionTest**: Comprehensive testing script for all functionality

## Files Created
- `/Assets/Scripts/Core/Mission.cs` - Mission POCO class
- `/Assets/Scripts/Core/MissionManager.cs` - Mission loading manager
- `/Assets/Config/sample_mission.json` - Sample mission data
- `/Assets/Scripts/MissionDemo.cs` - Demo component
- `/Assets/Scripts/MissionTest.cs` - Test component

## Requirements Met
✅ Load mission with textual description and GPS coordinates
✅ JSON loading capability
✅ Event publishing ("MissionLoaded")
✅ Console logging (description + GPS)
✅ Simple validation (required fields)
✅ POCO design pattern
✅ Inspector integration