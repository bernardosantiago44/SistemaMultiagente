# Camera Sensor System

This document describes the Camera Sensor simulation system implemented for the multi-agent Unity project.

## Overview

The Camera Sensor provides simulated camera detection capabilities without actual image processing. Instead, it uses Unity's physics system to detect objects within a configurable field of view and range.

## Components

### 1. SensorProfile_Camera (ScriptableObject)

Configurable sensor parameters stored as a Unity asset.

**Parameters:**
- **Field of View**: Angular vision cone (1-180°)
- **View Range**: Maximum detection distance (1-100m)
- **Target Tag**: Unity tag to detect (e.g., "Player", "Target")
- **Update Frequency**: Sensor refresh rate (1-60 Hz)
- **Detection Accuracy**: Probability of successful detection (0.0-1.0)
- **False Positive Rate**: Chance of false detections per frame (0.0-0.1)
- **Obstacle Layer Mask**: Layers that block line of sight

### 2. CameraSensor (MonoBehaviour)

Main sensor component that performs physics-based detection.

**Key Features:**
- Physics-based detection using `OverlapSphere` + `Raycast`
- Line-of-sight verification
- Noise simulation (missed detections, false positives)
- Event-driven architecture with `OnCameraFrame`
- Performance tracking and debug visualization

**Events:**
- `OnCameraFrame(CameraFrame frame)`: Published at configured frequency

### 3. CameraFrame Data Structure

Contains detection results for a single sensor update:

```csharp
public class CameraFrame
{
    public List<CameraDetection> detections;
    public float timestamp;
    public Vector3 sensorPosition;
    public Vector3 sensorForward;
}

public class CameraDetection
{
    public Transform target;        // Detected object (null for false positives)
    public Vector3 position;        // World position
    public float distance;          // Distance from sensor
    public float confidence;        // Detection confidence (0.0-1.0)
    public string detectedTag;      // Object tag
}
```

## Setup Instructions

### 1. Create Sensor Profile Asset

1. In Unity Project window, right-click in `Assets/ScriptableObjects/`
2. Select **Create > MultiAgent > Sensor Profile > Camera**
3. Name it (e.g., "DroneCameraSensor")
4. Configure parameters in Inspector

### 2. Add Sensor to Drone

1. Select your drone GameObject
2. Add Component: **CameraSensor**
3. Assign the SensorProfile_Camera asset to **Sensor Profile** field
4. Configure debug options if needed

### 3. Subscribe to Detection Events

```csharp
public class MyDetectionHandler : MonoBehaviour
{
    private CameraSensor sensor;
    
    void Start()
    {
        sensor = GetComponent<CameraSensor>();
        sensor.OnCameraFrame += ProcessDetections;
    }
    
    void ProcessDetections(CameraFrame frame)
    {
        foreach (var detection in frame.detections)
        {
            Debug.Log($"Detected {detection.detectedTag} at {detection.distance:F1}m");
        }
    }
}
```

## Testing

### Using CameraSensorTest

1. Add **CameraSensorTest** component to drone with **DroneController** and **CameraSensor**
2. Enable **Auto Test** for automated testing
3. Use keyboard controls:
   - **T**: Start manual test
   - **R**: Reset drone position
   - **L**: Land drone

### Manual Testing

1. Create objects with the target tag in the scene
2. Position drone using DroneController
3. Observe detections in Console or through event handlers
4. Use Scene view gizmos to visualize FOV and detections

## Configuration Examples

### High-Precision Sensor
```
Field of View: 45°
View Range: 30m
Update Frequency: 30Hz
Detection Accuracy: 0.98
False Positive Rate: 0.005
```

### Wide-Area Scanner
```
Field of View: 120°
View Range: 100m
Update Frequency: 5Hz
Detection Accuracy: 0.85
False Positive Rate: 0.02
```

### Close-Range Tracker
```
Field of View: 90°
View Range: 15m
Update Frequency: 60Hz
Detection Accuracy: 0.99
False Positive Rate: 0.001
```

## Performance Considerations

- **Update Frequency**: Higher rates increase CPU usage
- **View Range**: Larger ranges require more physics queries
- **FOV**: Wider angles affect line-of-sight raycast counts
- **Object Count**: More potential targets increase processing time

## Debug Features

### Visual Gizmos (Editor Only)
- Cyan sphere: Detection range
- Yellow lines: Field of view boundaries
- Green spheres: Successful detections
- Red cubes: False positive detections

### Performance Monitoring
- `GetActualFramerate()`: Measured update rate
- `GetDetectionCount()`: Current detection count
- Console logging: Enable in CameraSensor component

## Integration with DroneController

The sensor is designed to work seamlessly with the existing DroneController:

```csharp
// Example: Drone follows detected targets
public class TargetFollower : MonoBehaviour
{
    private DroneController drone;
    private CameraSensor sensor;
    
    void Start()
    {
        drone = GetComponent<DroneController>();
        sensor = GetComponent<CameraSensor>();
        sensor.OnCameraFrame += OnDetection;
    }
    
    void OnDetection(CameraFrame frame)
    {
        if (frame.detections.Count > 0)
        {
            var closest = frame.detections[0]; // Get closest detection
            Vector3 targetPos = new Vector3(closest.position.x, drone.transform.position.y, closest.position.z);
            drone.GoTo(targetPos);
        }
    }
}
```

## Troubleshooting

**No detections appearing:**
- Check target objects have correct tag
- Verify sensor profile is assigned
- Ensure objects are within range and FOV
- Check obstacle layer mask settings

**Performance issues:**
- Reduce update frequency
- Decrease view range
- Lower FOV angle
- Optimize obstacle layer mask

**False detections:**
- Reduce false positive rate
- Check line-of-sight raycast setup
- Verify target tag uniqueness

## Future Enhancements

- Multi-target tracking with IDs
- Detection history and persistence
- Integration with behavior trees
- Advanced noise models
- Performance profiling tools