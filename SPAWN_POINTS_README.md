# Person Spawn Points System

This system provides a data-driven approach for spawning persons at predefined locations within the Unity scene.

## Components

### 1. Spawnpoints.cs (Static Class)
- **Purpose**: Provides static access to spawn point coordinates loaded from JSON
- **Location**: `Assets/Scripts/Spawnpoints.cs`
- **Key Methods**:
  - `GetRandomSpawnPoint()` - Returns a random Vector3 spawn position
  - `GetAllSpawnPoints()` - Returns all available spawn positions
  - `GetSpawnPointCount()` - Returns total number of spawn points
  - `GetSpawnPointByIndex(int index)` - Returns specific spawn point by index
  - `ReloadSpawnPoints()` - Forces reload from JSON file

### 2. PersonSpawner.cs (MonoBehaviour)
- **Purpose**: Handles actual spawning of person objects at spawn point locations
- **Location**: `Assets/Scripts/PersonSpawner.cs`
- **Features**:
  - Single or multiple person spawning
  - Ground alignment using raycasting
  - Minimum distance enforcement between spawns
  - Debug visualization with gizmos
- **Key Methods**:
  - `SpawnPerson()` - Spawn single person at random location
  - `SpawnMultiplePersons()` - Spawn multiple persons at different locations
  - `SpawnPersonManually()` - Manual spawn trigger (can be called from UI)

### 3. SpawnpointsTest.cs (MonoBehaviour)
- **Purpose**: Comprehensive testing suite for the spawn points system
- **Location**: `Assets/Scripts/SpawnpointsTest.cs`
- **Tests Include**:
  - JSON loading validation
  - Random selection functionality
  - Index-based access
  - PersonSpawner integration
  - Optional drone reachability testing

## Configuration

### Spawn Points Data
- **File**: `Assets/StreamingAssets/spawn_points.json`
- **Format**:
```json
{
  "spawnLocations": [
    { "id": 1, "x": 85.2, "y": -46.8, "z": -230.9 },
    ...
  ]
}
```
- **Current Count**: 20 spawn points (meets minimum requirement)

### PersonSpawner Configuration
In the Inspector, you can configure:
- **Person Prefab**: The GameObject to spawn
- **Spawn On Start**: Automatically spawn when scene starts
- **Spawn Radius**: Area around spawn point for positioning
- **Multiple Spawning**: Enable spawning multiple persons
- **Number To Spawn**: How many persons to spawn
- **Min Distance Between Spawns**: Minimum separation distance
- **Ground Layer Mask**: Which layers to consider as ground
- **Debug Gizmos**: Show visual indicators in Scene view

## Usage Examples

### Basic Spawning
```csharp
// Get a random spawn point
Vector3 spawnPos = Spawnpoints.GetRandomSpawnPoint();

// Spawn a person using PersonSpawner
PersonSpawner spawner = FindObjectOfType<PersonSpawner>();
GameObject person = spawner.SpawnPerson();
```

### Advanced Usage
```csharp
// Get all spawn points for custom logic
List<Vector3> allPoints = Spawnpoints.GetAllSpawnPoints();

// Check spawn point count
int count = Spawnpoints.GetSpawnPointCount();

// Get specific spawn point
Vector3 specificPoint = Spawnpoints.GetSpawnPointByIndex(5);
```

### Testing
```csharp
// Run tests manually
SpawnpointsTest tester = FindObjectOfType<SpawnpointsTest>();
tester.RunTests();
```

## Integration with DroneController

The spawn points are positioned to ensure the drone can reach them within a defined threshold. The `SpawnpointsTest` can verify drone reachability by:
1. Calculating distances from drone position to each spawn point
2. Counting points within the specified threshold
3. Reporting reachability statistics

## Requirements Met

✅ **At least 20 coordinates (x, y, z)**: 20 spawn points defined  
✅ **Static class access**: `Spawnpoints.cs` provides static methods  
✅ **Random selection**: `GetRandomSpawnPoint()` selects randomly  
✅ **DroneController integration**: Spawn points positioned for drone accessibility  
✅ **JSON-based configuration**: Data-driven approach using `spawn_points.json`  
✅ **PersonSpawner implementation**: Complete spawning system  
✅ **Testing framework**: Comprehensive test suite included  

## File Structure
```
Assets/
├── Scripts/
│   ├── Spawnpoints.cs          # Static class for spawn point access
│   ├── PersonSpawner.cs        # Person spawning MonoBehaviour
│   └── SpawnpointsTest.cs      # Testing and validation
└── StreamingAssets/
    └── spawn_points.json       # Spawn point coordinate data
```