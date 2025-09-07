using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class CameraDetection
{
    public Transform target;
    public Vector3 position;
    public float distance;
    public float confidence; // 0.0 to 1.0 based on noise simulation
    public string detectedTag;
    
    public CameraDetection(Transform target, Vector3 position, float distance, float confidence, string tag)
    {
        this.target = target;
        this.position = position;
        this.distance = distance;
        this.confidence = confidence;
        this.detectedTag = tag;
    }
}

[System.Serializable]
public class CameraFrame
{
    public List<CameraDetection> detections;
    public float timestamp;
    public Vector3 sensorPosition;
    public Vector3 sensorForward;
    
    public CameraFrame(Vector3 sensorPos, Vector3 sensorFwd)
    {
        detections = new List<CameraDetection>();
        timestamp = Time.time;
        sensorPosition = sensorPos;
        sensorForward = sensorFwd;
    }
}

[DisallowMultipleComponent]
public class CameraSensor : MonoBehaviour
{
    [Header("Configuration")]
    [SerializeField] private SensorProfile_Camera sensorProfile;
    
    [Header("Debug")]
    [SerializeField] private bool showDebugGizmos = true;
    [SerializeField] private bool logDetections = false;
    
    // Events
    public System.Action<CameraFrame> OnCameraFrame;
    
    // Internal state
    private Coroutine sensorCoroutine;
    private List<CameraDetection> lastDetections = new List<CameraDetection>();
    
    // Performance tracking
    private int frameCount = 0;
    private float lastFrameTime = 0f;
    private float actualFramerate = 0f;
    
    private void Start()
    {
        if (sensorProfile == null)
        {
            Debug.LogError($"[CameraSensor] {gameObject.name}: SensorProfile_Camera is required!", this);
            enabled = false;
            return;
        }
        
        StartSensor();
    }
    
    private void OnEnable()
    {
        if (sensorProfile != null && sensorCoroutine == null)
            StartSensor();
    }
    
    private void OnDisable()
    {
        StopSensor();
    }
    
    public void StartSensor()
    {
        if (sensorProfile == null) return;
        
        StopSensor();
        sensorCoroutine = StartCoroutine(SensorUpdateLoop());
        
        if (logDetections)
            Debug.Log($"[CameraSensor] {gameObject.name}: Started with {sensorProfile.updateFrequency}Hz update rate");
    }
    
    public void StopSensor()
    {
        if (sensorCoroutine != null)
        {
            StopCoroutine(sensorCoroutine);
            sensorCoroutine = null;
        }
    }
    
    private IEnumerator SensorUpdateLoop()
    {
        float updateInterval = 1f / sensorProfile.updateFrequency;
        
        while (true)
        {
            float frameStart = Time.time;
            
            // Perform detection
            CameraFrame frame = PerformDetection();
            
            // Publish frame
            OnCameraFrame?.Invoke(frame);
            
            // Update performance metrics
            UpdateFramerateTracking(frameStart);
            
            // Wait for next update
            yield return new WaitForSeconds(updateInterval);
        }
    }
    
    private CameraFrame PerformDetection()
    {
        CameraFrame frame = new CameraFrame(transform.position, transform.forward);
        lastDetections.Clear();
        
        // Find all potential targets in range using OverlapSphere
        Collider[] nearbyColliders = Physics.OverlapSphere(transform.position, sensorProfile.viewRange);
        
        foreach (Collider col in nearbyColliders)
        {
            // Skip self
            if (col.transform == transform) continue;
            
            // Check if object has the target tag
            if (!col.CompareTag(sensorProfile.targetTag)) continue;
            
            Vector3 targetPosition = col.bounds.center;
            Vector3 directionToTarget = (targetPosition - transform.position).normalized;
            float distanceToTarget = Vector3.Distance(transform.position, targetPosition);
            
            // Check if target is within field of view
            float angleToTarget = Vector3.Angle(transform.forward, directionToTarget);
            if (angleToTarget > sensorProfile.fieldOfView / 2f) continue;
            
            // Check line of sight using raycast
            if (!HasLineOfSight(targetPosition, distanceToTarget)) continue;
            
            // Apply noise simulation
            float detectionRoll = Random.Range(0f, 1f);
            if (detectionRoll > sensorProfile.detectionAccuracy) continue;
            
            // Create detection with confidence based on distance and angle
            float distanceConfidence = 1f - (distanceToTarget / sensorProfile.viewRange);
            float angleConfidence = 1f - (angleToTarget / (sensorProfile.fieldOfView / 2f));
            float overallConfidence = (distanceConfidence + angleConfidence) / 2f;
            
            CameraDetection detection = new CameraDetection(
                col.transform, 
                targetPosition, 
                distanceToTarget, 
                overallConfidence * sensorProfile.detectionAccuracy,
                sensorProfile.targetTag
            );
            
            frame.detections.Add(detection);
            lastDetections.Add(detection);
        }
        
        // Add false positives based on noise settings
        AddFalsePositives(frame);
        
        if (logDetections && frame.detections.Count > 0)
        {
            Debug.Log($"[CameraSensor] {gameObject.name}: Detected {frame.detections.Count} objects");
        }
        
        return frame;
    }
    
    private bool HasLineOfSight(Vector3 targetPosition, float maxDistance)
    {
        Vector3 direction = (targetPosition - transform.position).normalized;
        
        // Raycast from sensor position towards target
        if (Physics.Raycast(transform.position, direction, out RaycastHit hit, maxDistance, sensorProfile.obstacleLayerMask))
        {
            // Check if we hit the target or an obstacle
            float hitDistance = Vector3.Distance(transform.position, hit.point);
            float targetDistance = Vector3.Distance(transform.position, targetPosition);
            
            // If we hit something closer than the target, it's blocked
            return hitDistance >= targetDistance - 0.1f; // Small tolerance
        }
        
        // No obstacles detected
        return true;
    }
    
    private void AddFalsePositives(CameraFrame frame)
    {
        float falsePositiveRoll = Random.Range(0f, 1f);
        if (falsePositiveRoll <= sensorProfile.falsePositiveRate)
        {
            // Generate a random false positive within FOV and range
            float randomAngle = Random.Range(-sensorProfile.fieldOfView / 2f, sensorProfile.fieldOfView / 2f);
            float randomDistance = Random.Range(sensorProfile.viewRange * 0.3f, sensorProfile.viewRange);
            
            Vector3 randomDirection = Quaternion.AngleAxis(randomAngle, transform.up) * transform.forward;
            Vector3 falsePosition = transform.position + randomDirection * randomDistance;
            
            CameraDetection falseDetection = new CameraDetection(
                null, // No actual target
                falsePosition,
                randomDistance,
                Random.Range(0.1f, 0.6f), // Low confidence for false positives
                sensorProfile.targetTag + "_FALSE"
            );
            
            frame.detections.Add(falseDetection);
        }
    }
    
    private void UpdateFramerateTracking(float frameStart)
    {
        frameCount++;
        float frameTime = Time.time - frameStart;
        
        if (Time.time - lastFrameTime >= 1f)
        {
            actualFramerate = frameCount / (Time.time - lastFrameTime);
            frameCount = 0;
            lastFrameTime = Time.time;
        }
    }
    
    // Public API
    public SensorProfile_Camera GetSensorProfile() => sensorProfile;
    public void SetSensorProfile(SensorProfile_Camera profile)
    {
        sensorProfile = profile;
        if (gameObject.activeInHierarchy && enabled)
        {
            StartSensor(); // Restart with new settings
        }
    }
    
    public List<CameraDetection> GetLastDetections() => new List<CameraDetection>(lastDetections);
    public float GetActualFramerate() => actualFramerate;
    public int GetDetectionCount() => lastDetections.Count;
    
#if UNITY_EDITOR
    private void OnDrawGizmosSelected()
    {
        if (!showDebugGizmos || sensorProfile == null) return;
        
        // Draw detection range
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, sensorProfile.viewRange);
        
        // Draw field of view
        Gizmos.color = Color.yellow;
        float halfFOV = sensorProfile.fieldOfView / 2f;
        Vector3 leftBoundary = Quaternion.AngleAxis(-halfFOV, transform.up) * transform.forward * sensorProfile.viewRange;
        Vector3 rightBoundary = Quaternion.AngleAxis(halfFOV, transform.up) * transform.forward * sensorProfile.viewRange;
        
        Gizmos.DrawLine(transform.position, transform.position + leftBoundary);
        Gizmos.DrawLine(transform.position, transform.position + rightBoundary);
        
        // Draw arc for FOV
        int arcSegments = 20;
        for (int i = 0; i < arcSegments; i++)
        {
            float angle1 = Mathf.Lerp(-halfFOV, halfFOV, (float)i / arcSegments);
            float angle2 = Mathf.Lerp(-halfFOV, halfFOV, (float)(i + 1) / arcSegments);
            
            Vector3 point1 = transform.position + Quaternion.AngleAxis(angle1, transform.up) * transform.forward * sensorProfile.viewRange;
            Vector3 point2 = transform.position + Quaternion.AngleAxis(angle2, transform.up) * transform.forward * sensorProfile.viewRange;
            
            Gizmos.DrawLine(point1, point2);
        }
        
        // Draw detections
        if (Application.isPlaying && lastDetections != null)
        {
            foreach (var detection in lastDetections)
            {
                if (detection.target != null)
                {
                    Gizmos.color = Color.green;
                    Gizmos.DrawWireSphere(detection.position, 0.5f);
                    Gizmos.DrawLine(transform.position, detection.position);
                }
                else
                {
                    // False positive
                    Gizmos.color = Color.red;
                    Gizmos.DrawWireCube(detection.position, Vector3.one * 0.3f);
                }
            }
        }
    }
#endif
}