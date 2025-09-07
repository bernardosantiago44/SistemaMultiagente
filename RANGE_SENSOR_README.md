# Range Sensor System (Ultrasonic/LiDAR)

This document describes the Range Sensor simulation system implemented for terrain-relative altitude control in the multi-agent Unity project.

## Overview

The Range Sensor simulates ultrasonic or LiDAR distance sensors that measure the distance to the nearest obstacle or ground surface. It provides accurate distance readings with realistic noise and saturation characteristics, making it ideal for altitude hold and terrain-following applications.

## Components

### 1. SensorProfile_Range (ScriptableObject)

Configurable sensor parameters stored as a Unity asset.

**Parameters:**
- **Max Range**: Maximum detection distance (0.1-200m)
- **Noise Magnitude**: Gaussian noise added to measurements (0-5m)
- **Update Frequency**: Sensor refresh rate (1-100 Hz)
- **Obstacle Layer Mask**: Layers that the sensor can detect
- **Ray Direction**: Sensor ray direction relative to transform (default: down)

**Default Values:**
- Max Range: 100m
- Noise Magnitude: 0.05m (±5cm)
- Update Frequency: 20Hz
- Direction: Vector3.down (pointing downward)

### 2. RangeSensor (MonoBehaviour)

Main sensor component that performs raycast-based distance measurement.

**Key Features:**
- Physics-based distance measurement using `Physics.Raycast`
- Gaussian noise simulation for realistic sensor behavior
- Saturation when no objects detected within range
- Configurable update frequency independent of frame rate
- Real-time performance tracking and debug logging

**Public API:**
```csharp
// Basic distance access
float GetDistance()                    // Current distance reading
bool IsInRange()                      // True if valid reading (not saturated)
RangeReading LastReading              // Complete reading with metadata

// Events
System.Action<RangeReading> OnRangeReading  // Triggered on each sensor update

// Configuration
void SetSensorProfile(SensorProfile_Range profile)
SensorProfile_Range GetSensorProfile()
```

### 3. RangeReading (Data Structure)

Complete sensor reading with metadata:
```csharp
public class RangeReading
{
    public float distance;           // Measured distance (with noise)
    public bool hasHit;             // True if obstacle detected
    public Vector3 hitPoint;        // World position of hit
    public Vector3 hitNormal;       // Surface normal at hit point
    public Transform hitTransform;   // Hit object's transform
    public float timestamp;         // Time of measurement
    public float confidence;        // Reading confidence (0.0-1.0)
}
```

## Integration with Flight Control

### AltitudeHold Enhancement

The Range Sensor integrates with the existing AltitudeHold system to enable terrain-relative altitude control:

**Enhanced Constructor:**
```csharp
// Traditional absolute altitude control
AltitudeHold altController = new AltitudeHold(flightProfile);

// Terrain-relative altitude control
AltitudeHold altController = new AltitudeHold(flightProfile, rangeSensor);
```

**Runtime Mode Switching:**
```csharp
// Switch between absolute and terrain-relative modes
altController.SetTerrainRelativeMode(true);   // Enable terrain following
altController.SetTerrainRelativeMode(false);  // Use absolute altitude
```

### DroneController Integration

The DroneController automatically integrates Range Sensors found on the same GameObject:

**Automatic Setup:**
- Range Sensor is auto-detected if present on drone
- AltitudeHold uses Range Sensor when terrain-relative mode is enabled
- Seamless fallback to absolute mode when sensor is out of range

**Manual Configuration:**
```csharp
// Assign range sensor manually
droneController.SetRangeSensor(rangeSensor);

// Enable terrain-relative altitude control
droneController.SetTerrainRelativeMode(true);

// Check current mode
bool isTerrainRelative = droneController.IsUsingTerrainRelativeAltitude();

// Get sensor reading
float distanceToGround = droneController.GetRangeSensorDistance();
```

## Setup Instructions

### Basic Setup

1. **Create Sensor Profile:**
   - Right-click in Project → Create → MultiAgent → Sensor Profile → Range
   - Configure max range, noise, and update frequency
   - Save as asset (e.g., "SensorProfile_Range.asset")

2. **Add Sensor to Drone:**
   - Add RangeSensor component to drone GameObject
   - Assign the sensor profile to the RangeSensor component
   - Ensure sensor is positioned appropriately (usually pointing downward)

3. **Configure DroneController:**
   - RangeSensor will be auto-detected if on same GameObject
   - Enable "Use Terrain Relative Altitude" in inspector
   - Or use `SetTerrainRelativeMode(true)` in code

### Advanced Configuration

**Custom Ray Direction:**
- Modify `rayDirection` in sensor profile for non-downward sensors
- Useful for obstacle avoidance or side-looking applications

**Layer Filtering:**
- Configure `obstacleLayerMask` to detect specific object types
- Exclude unwanted colliders (other drones, effects, etc.)

**Noise Tuning:**
- Increase `noiseMagnitude` for less accurate sensors
- Set to 0 for perfect measurements (testing/debugging)

## Test Scripts

### 1. RangeSensorTest

Basic sensor testing:
- Validates sensor readings and performance
- Displays real-time sensor data
- Tests integration with DroneController

### 2. TerrainFollowingTest

Advanced terrain-following demonstration:
- Switches between altitude control modes during flight
- Waypoint navigation with terrain-relative altitude
- Performance comparison between modes

**Features:**
- Automatic mode toggling every 15 seconds
- Waypoint navigation with altitude adaptation
- Real-time status display and logging

## Debug Visualization

### Gizmos (Scene View)

**RangeSensor Gizmos:**
- **Yellow Ray**: Sensor range when not playing
- **Green Ray**: Valid hit detected
- **Red Ray**: No hit (saturated reading)
- **Red Sphere**: Hit point location
- **Blue Ray**: Surface normal at hit point

**DroneController Gizmos:**
- **Yellow Ray**: Traditional AGL measurement
- **Blue/Green Ray**: Range sensor reading
- **Green Cube**: Ground point in terrain-relative mode

### Runtime Debugging

Enable debug logging in RangeSensor:
- Set `logReadings = true` in inspector
- Monitor console for sensor readings and performance
- Track framerate and reading statistics

## Performance Considerations

**Update Frequency:**
- Higher frequency = more responsive but more CPU intensive
- 20Hz recommended for altitude control
- 5-10Hz sufficient for obstacle avoidance

**Physics Performance:**
- Raycast operations are lightweight
- Layer masking reduces unnecessary collision checks
- Sensor automatically handles frame rate independence

## Common Use Cases

### 1. Altitude Hold Above Ground
```csharp
// Set target altitude to 3 meters above ground
flightProfile.targetAltitude = 3.0f;
droneController.SetTerrainRelativeMode(true);
droneController.TakeOff(3.0f);
```

### 2. Terrain Following Navigation
```csharp
// Navigate while maintaining constant height above terrain
droneController.SetTerrainRelativeMode(true);
droneController.GoTo(new Vector3(x, desiredHeightAboveGround, z));
```

### 3. Landing Detection
```csharp
// Monitor sensor for landing
if (rangeSensor.GetDistance() < 0.1f && rangeSensor.IsInRange())
{
    // Very close to ground - initiate landing sequence
}
```

### 4. Obstacle Avoidance (Future Enhancement)
- Configure sensor with horizontal ray direction
- Use for side obstacle detection
- Integrate with navigation system for path planning

## Troubleshooting

**Sensor Not Working:**
- Check that sensor profile is assigned
- Verify obstacle layer mask includes target objects
- Ensure sensor is enabled and started

**Erratic Readings:**
- Reduce noise magnitude in sensor profile
- Check for interfering colliders in ray path
- Verify update frequency is appropriate

**Terrain Following Issues:**
- Confirm terrain-relative mode is enabled
- Check that range sensor has valid readings
- Verify target altitude is within sensor range

**Performance Issues:**
- Reduce sensor update frequency
- Optimize layer mask to exclude unnecessary objects
- Check for excessive debug logging

## Future Enhancements

**Potential Improvements:**
- Multi-ray sensors for wider coverage
- Sensor fusion with multiple range sensors
- Integration with obstacle avoidance system
- Real-time noise profile adaptation
- Sensor degradation/failure simulation