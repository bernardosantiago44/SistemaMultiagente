# Navigation System Documentation

## Overview
The Navigation System provides functionality for autonomous drone movement towards target positions using GPS coordinates or Unity world positions. It integrates with the existing DroneController and GpsMapper components to provide complete navigation capabilities.

## Components

### Navigator.cs
Main navigation controller that handles:
- **GPS Navigation**: Converts GPS coordinates to Unity world positions and navigates to them
- **World Position Navigation**: Direct navigation to Unity world coordinates
- **Mission Integration**: Automatically starts navigation when missions are loaded
- **Distance Validation**: Warns when targets are less than 150m (requirement) but proceeds
- **Target Tracking**: Monitors distance to target and detects when reached
- **Event System**: Provides navigation lifecycle events
- **Logging**: Outputs "En tránsito" message as required

### WaypointQueue.cs
Queue system for managing multiple navigation waypoints:
- **Queue Management**: Add, remove, peek operations for waypoints
- **Target Detection**: Configurable threshold for detecting when waypoints are reached
- **Visual Debugging**: Gizmos for visualizing waypoints and paths in Unity editor
- **Event Logging**: Detailed logging of waypoint operations

### NavigationTest.cs
Comprehensive test suite for navigation functionality:
- **Component Testing**: Validates all navigation components
- **GPS Testing**: Tests GPS coordinate conversion and validation
- **Integration Testing**: Tests Navigator-DroneController integration
- **Distance Testing**: Validates minimum distance requirements
- **Event Testing**: Tests navigation event system

### NavigationDemo.cs
Interactive demo script for testing navigation:
- **GUI Controls**: Simple in-game UI for testing navigation features
- **Context Menu**: Unity editor context menu commands
- **Demo Scenarios**: Pre-configured test scenarios
- **Status Display**: Shows current navigation status

## Usage

### Basic Setup
1. Add `Navigator` component to your drone GameObject
2. Add `WaypointQueue` component if using waypoint navigation
3. Ensure `DroneController` and `GpsMapper` components are available
4. Configure GPS origin coordinates in Navigator inspector
5. Set target reach threshold and navigation parameters

### GPS Navigation
```csharp
// Get Navigator reference
Navigator navigator = GetComponent<Navigator>();

// Set GPS origin (usually done once)
navigator.SetGpsOrigin(new Vector2(19.432608f, -99.133209f)); // Mexico City

// Navigate to GPS coordinates
Vector2 target = new Vector2(19.442608f, -99.121209f);
navigator.GoToGpsCoordinates(target);
```

### World Position Navigation
```csharp
// Navigate to Unity world position
Vector3 worldTarget = new Vector3(200f, 50f, 200f);
navigator.GoToWorldPosition(worldTarget);
```

### Waypoint Navigation
```csharp
// Add multiple waypoints
navigator.AddWaypoint(new Vector3(100f, 30f, 0f));
navigator.AddWaypoint(new Vector3(200f, 40f, 100f));
navigator.AddWaypoint(new Vector3(300f, 50f, 200f));

// Or add GPS waypoints
navigator.AddGpsWaypoint(new Vector2(19.435f, -99.130f));
navigator.AddGpsWaypoint(new Vector2(19.440f, -99.125f));
```

### Event Handling
```csharp
// Subscribe to navigation events
navigator.OnNavigationStarted += OnNavigationStarted;
navigator.OnTargetReached += OnTargetReached;
navigator.OnNavigationCompleted += OnNavigationCompleted;

private void OnNavigationStarted(Vector3 target)
{
    Debug.Log($"Navigation started to: {target}");
}

private void OnTargetReached(Vector3 target)
{
    Debug.Log($"Target reached: {target}");
}

private void OnNavigationCompleted()
{
    Debug.Log("Navigation completed");
}
```

### Mission Integration
The Navigator automatically integrates with the Mission system:
```csharp
// Navigation will start automatically when a mission is loaded
MissionManager missionManager = FindObjectOfType<MissionManager>();
missionManager.LoadMissionFromJson();
// Navigator will automatically navigate to mission GPS coordinates
```

## Configuration

### Navigator Configuration
- **minimumDistance**: 150m - warns if target is closer but proceeds (requirement)
- **targetReachThreshold**: 5.0m - distance at which target is considered reached
- **navigationSpeed**: 10 m/s - target navigation speed (not yet fully implemented)
- **targetAltitude**: 50m AGL - default altitude for navigation targets
- **gpsOrigin**: GPS origin point for coordinate conversion

### WaypointQueue Configuration
- **reachThreshold**: 2.0m - distance at which waypoints are considered reached
- **autoRemoveReached**: true - automatically remove reached waypoints from queue
- **logWaypointOperations**: true - enable logging of waypoint operations
- **visualizeWaypoints**: true - show waypoints in Unity editor

## DroneController Integration

The Navigator integrates with DroneController by:
1. Calling `droneController.GoTo(worldPosition)` to set target
2. DroneController handles the actual movement physics
3. Navigator monitors progress and handles waypoint transitions

### Enhanced DroneController Features
- **GoTo Method**: Accepts world position targets from Navigator
- **Automatic Navigation**: Switches between manual and automatic control
- **Target Tracking**: Tracks current target position and status
- **Simple Movement**: Proportional control for horizontal and vertical movement

## Testing

### Running Tests
1. Add `NavigationTest` component to any GameObject
2. Enable "Run Tests On Start" in inspector
3. Play scene to run all tests automatically
4. Check console for test results

### Demo Testing
1. Add `NavigationDemo` component to any GameObject
2. Use context menu items in Unity editor or
3. Use GUI buttons during play mode
4. Test various navigation scenarios

### Manual Testing
1. Create a scene with a drone GameObject
2. Add Navigator, DroneController, and WaypointQueue components
3. Set appropriate parameters in inspectors
4. Use NavigationDemo for interactive testing
5. Monitor console output for "En tránsito" messages

## Requirements Met

✅ **Move drone towards target position**: Navigator integrates with DroneController for movement
✅ **Minimum 150m distance handling**: Warns but proceeds when target < 150m
✅ **Use GpsMapper**: GPS coordinates converted using GpsMapper utility
✅ **Use DroneController**: Integrates with existing DroneController for movement
✅ **Log "En tránsito"**: Outputs required message when navigation starts
✅ **Simple implementation**: Follows KISS principle with basic proportional control

## Architecture

```
Mission System → Navigator → DroneController
     ↓              ↓            ↓
GPS Coords → GpsMapper → Unity World Pos → Physics Movement
                ↓
           WaypointQueue → Multiple Targets
```

## Files Structure
```
/Assets/Scripts/Navigation/
├── Navigator.cs          # Main navigation controller
├── WaypointQueue.cs      # Waypoint queue management
├── NavigationTest.cs     # Test suite
└── NavigationDemo.cs     # Demo and testing utilities

/Assets/Scripts/
└── NavigationDemo.cs     # Root level demo script
```

## Future Enhancements
- **PID Controllers**: More sophisticated movement control
- **Obstacle Avoidance**: Integration with collision detection
- **Path Planning**: Advanced routing algorithms
- **Speed Control**: Dynamic speed adjustment based on distance
- **Altitude Management**: Sophisticated altitude control