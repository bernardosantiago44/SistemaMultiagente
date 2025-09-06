using UnityEngine;

[CreateAssetMenu(fileName = "NewFlightProfile", menuName = "MultiAgent/Flight Profile")]
public class FlightProfile : ScriptableObject
{
    [Header("Basic Flight Parameters")]
    public float massKg = 1.2f;
    public float maxTiltDeg = 25f;
    public float maxClimbRate = 3.0f;
    public float maxDescentRate = 2.0f;
    public float lateralAccel = 8.0f;
    public float yawRateDeg = 90f;

    [Header("Altitude Control")]
    [Tooltip("Target altitude in meters")]
    public float targetAltitude = 20.0f;
    
    [Tooltip("Proportional gain for altitude control")]
    public float altitudeKp = 1.0f;
    
    [Tooltip("Integral gain for altitude control")]
    public float altitudeKi = 0.1f;
    
    [Tooltip("Derivative gain for altitude control")]
    public float altitudeKd = 0.5f;
    
    [Tooltip("Maximum vertical thrust in Newtons")]
    public float maxVerticalThrust = 20.0f;

    [Header("Velocity Control")]
    [Tooltip("Target horizontal speed in m/s")]
    public float targetSpeed = 10.0f;
    
    [Tooltip("Proportional gain for velocity control")]
    public float velocityKp = 1.0f;
    
    [Tooltip("Integral gain for velocity control")]
    public float velocityKi = 0.1f;
    
    [Tooltip("Derivative gain for velocity control")]
    public float velocityKd = 0.5f;
    
    [Tooltip("Maximum horizontal thrust in Newtons")]
    public float maxForwardThrust = 15.0f;
}