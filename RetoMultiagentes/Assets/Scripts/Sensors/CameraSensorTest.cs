using UnityEngine;

/// <summary>
/// Simple test script to demonstrate CameraSensor integration with DroneController
/// Attach this to a GameObject with DroneController and CameraSensor components
/// </summary>
public class CameraSensorTest : MonoBehaviour
{
    [Header("Components")]
    [SerializeField] private DroneController droneController;
    [SerializeField] private CameraSensor cameraSensor;
    
    [Header("Test Settings")]
    [SerializeField] private bool enableAutoTest = true;
    [SerializeField] private float testDuration = 30f;
    
    // Test state
    private float testStartTime;
    private int totalDetections = 0;
    private int frameCount = 0;
    
    private void Start()
    {
        // Auto-find components if not assigned
        if (droneController == null)
            droneController = GetComponent<DroneController>();
        if (cameraSensor == null)
            cameraSensor = GetComponent<CameraSensor>();
        
        // Validate required components
        if (droneController == null)
        {
            Debug.LogError("[CameraSensorTest] DroneController component not found!", this);
            enabled = false;
            return;
        }
        
        if (cameraSensor == null)
        {
            Debug.LogError("[CameraSensorTest] CameraSensor component not found!", this);
            enabled = false;
            return;
        }
        
        // Subscribe to camera events
        cameraSensor.OnCameraFrame += OnCameraFrameReceived;
        
        if (enableAutoTest)
        {
            StartTest();
        }
        
        Debug.Log("[CameraSensorTest] Test initialized. Use keyboard controls or enable auto-test.");
    }
    
    private void OnDestroy()
    {
        if (cameraSensor != null)
            cameraSensor.OnCameraFrame -= OnCameraFrameReceived;
    }
    
    private void StartTest()
    {
        testStartTime = Time.time;
        totalDetections = 0;
        frameCount = 0;
        
        Debug.Log($"[CameraSensorTest] Starting {testDuration}s test...");
        
        // Arm drone and take off for testing
        if (!droneController.IsArmed())
            droneController.Arm();
        
        droneController.TakeOff(5f); // Take off to 5 meters
    }
    
    private void OnCameraFrameReceived(CameraFrame frame)
    {
        frameCount++;
        totalDetections += frame.detections.Count;
        
        // Log significant detections
        if (frame.detections.Count > 0)
        {
            Debug.Log($"[CameraSensorTest] Frame {frameCount}: {frame.detections.Count} detections");
            
            foreach (var detection in frame.detections)
            {
                string targetName = detection.target != null ? detection.target.name : "FALSE_POSITIVE";
                Debug.Log($"  - Target: {targetName}, Distance: {detection.distance:F1}m, Confidence: {detection.confidence:F2}");
            }
        }
        
        // Check if test is complete
        if (enableAutoTest && Time.time - testStartTime >= testDuration)
        {
            CompleteTest();
        }
    }
    
    private void CompleteTest()
    {
        enableAutoTest = false;
        
        float avgDetectionsPerFrame = frameCount > 0 ? (float)totalDetections / frameCount : 0f;
        float actualFramerate = cameraSensor.GetActualFramerate();
        
        Debug.Log($"[CameraSensorTest] Test completed:");
        Debug.Log($"  - Total frames: {frameCount}");
        Debug.Log($"  - Total detections: {totalDetections}");
        Debug.Log($"  - Avg detections/frame: {avgDetectionsPerFrame:F2}");
        Debug.Log($"  - Actual framerate: {actualFramerate:F1} Hz");
        Debug.Log($"  - Target framerate: {cameraSensor.GetSensorProfile().updateFrequency} Hz");
    }
    
    private void Update()
    {
        // Manual test controls
        if (Input.GetKeyDown(KeyCode.T))
        {
            StartTest();
        }
        
        if (Input.GetKeyDown(KeyCode.R))
        {
            // Reset drone position for testing
            droneController.GoTo(new Vector3(0, 5, 0));
        }
        
        if (Input.GetKeyDown(KeyCode.L))
        {
            // Land drone
            droneController.GoTo(new Vector3(transform.position.x, 0.5f, transform.position.z));
        }
    }
    
    private void OnGUI()
    {
        if (!enableAutoTest)
        {
            GUILayout.BeginArea(new Rect(10, 10, 300, 200));
            GUILayout.Label("Camera Sensor Test Controls:");
            GUILayout.Label("T - Start Test");
            GUILayout.Label("R - Reset Position");
            GUILayout.Label("L - Land");
            GUILayout.Space(10);
            
            if (cameraSensor != null)
            {
                GUILayout.Label($"Detections: {cameraSensor.GetDetectionCount()}");
                GUILayout.Label($"Framerate: {cameraSensor.GetActualFramerate():F1} Hz");
                
                var profile = cameraSensor.GetSensorProfile();
                if (profile != null)
                {
                    GUILayout.Label($"FOV: {profile.fieldOfView}°");
                    GUILayout.Label($"Range: {profile.viewRange}m");
                    GUILayout.Label($"Target: {profile.targetTag}");
                }
            }
            
            GUILayout.EndArea();
        }
    }
}