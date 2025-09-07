using UnityEngine;

public class AltitudeHold
{
    private FlightProfile flightProfile;
    private float previousError = 0f;
    private float integralSum = 0f;
    private float lastTime;
    private bool initialized = false;
    
    // Range sensor integration for terrain-relative altitude control
    private RangeSensor rangeSensor;
    private bool useTerrainRelativeAltitude = false;

    public AltitudeHold(FlightProfile profile)
    {
        flightProfile = profile;
        Reset();
    }
    
    /// <summary>
    /// Constructor with optional range sensor for terrain-relative altitude control
    /// </summary>
    public AltitudeHold(FlightProfile profile, RangeSensor rangeSensor)
    {
        flightProfile = profile;
        this.rangeSensor = rangeSensor;
        this.useTerrainRelativeAltitude = (rangeSensor != null);
        Reset();
    }

    public void Reset()
    {
        previousError = 0f;
        integralSum = 0f;
        initialized = false;
    }
    
    /// <summary>
    /// Set or change the range sensor for terrain-relative altitude control
    /// </summary>
    public void SetRangeSensor(RangeSensor sensor)
    {
        rangeSensor = sensor;
        useTerrainRelativeAltitude = (sensor != null);
    }
    
    /// <summary>
    /// Enable or disable terrain-relative altitude control
    /// </summary>
    public void SetTerrainRelativeMode(bool enabled)
    {
        useTerrainRelativeAltitude = enabled && (rangeSensor != null);
    }

    public float GetVerticalThrust(float currentAltitude)
    {
        if (flightProfile == null)
        {
            Debug.LogError("FlightProfile is null in AltitudeHold controller");
            return 0f;
        }

        float currentTime = Time.time;
        
        if (!initialized)
        {
            lastTime = currentTime;
            initialized = true;
            return flightProfile.massKg * Physics.gravity.magnitude; // Return hover thrust on first call
        }

        float deltaTime = currentTime - lastTime;
        if (deltaTime <= 0f) return flightProfile.massKg * Physics.gravity.magnitude;

        // Determine current altitude based on mode
        float effectiveCurrentAltitude = GetEffectiveAltitude(currentAltitude);
        
        // Calculate error
        float error = flightProfile.targetAltitude - effectiveCurrentAltitude;

        // Proportional term
        float proportional = flightProfile.altitudeKp * error;

        // Integral term (with windup prevention)
        integralSum += error * deltaTime;
        // Clamp integral to prevent windup
        float maxIntegral = flightProfile.maxVerticalThrust / (flightProfile.altitudeKi + 0.001f);
        integralSum = Mathf.Clamp(integralSum, -maxIntegral, maxIntegral);
        float integral = flightProfile.altitudeKi * integralSum;

        // Derivative term
        float derivative = flightProfile.altitudeKd * (error - previousError) / deltaTime;

        // Calculate total thrust adjustment
        float pidOutput = proportional + integral + derivative;

        // Base thrust to counteract gravity (hover thrust)
        float hoverThrust = flightProfile.massKg * Physics.gravity.magnitude;

        // Total thrust command
        float totalThrust = hoverThrust + pidOutput;

        // Apply thrust limits
        totalThrust = Mathf.Clamp(totalThrust, 0f, flightProfile.maxVerticalThrust);

        // Update for next iteration
        previousError = error;
        lastTime = currentTime;

        return totalThrust;
    }

    public float GetCurrentError(float currentAltitude)
    {
        if (flightProfile == null) return 0f;
        float effectiveCurrentAltitude = GetEffectiveAltitude(currentAltitude);
        return flightProfile.targetAltitude - effectiveCurrentAltitude;
    }
    
    /// <summary>
    /// Get the effective altitude based on the current mode (absolute or terrain-relative)
    /// </summary>
    private float GetEffectiveAltitude(float currentAltitude)
    {
        if (useTerrainRelativeAltitude && rangeSensor != null && rangeSensor.IsInRange())
        {
            // Use range sensor for terrain-relative altitude
            return rangeSensor.GetDistance();
        }
        else
        {
            // Use absolute altitude (world Y position)
            return currentAltitude;
        }
    }
    
    /// <summary>
    /// Check if currently using terrain-relative altitude control
    /// </summary>
    public bool IsUsingTerrainRelativeMode()
    {
        return useTerrainRelativeAltitude && rangeSensor != null;
    }
    
    /// <summary>
    /// Get the current range sensor being used (if any)
    /// </summary>
    public RangeSensor GetRangeSensor()
    {
        return rangeSensor;
    }
}