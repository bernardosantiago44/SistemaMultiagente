using UnityEngine;

public class AltitudeHold
{
    private FlightProfile flightProfile;
    private float previousError = 0f;
    private float integralSum = 0f;
    private float lastTime;
    // private bool initialized = false;
    
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
    // initialized = false;
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

        // Empuje base (hover) = peso del dron
        float hoverThrust = flightProfile.massKg * Physics.gravity.magnitude;

        // Error de altitud
        float error = flightProfile.targetAltitude - currentAltitude;

        // Ajuste proporcional sencillo
        float adjustment = flightProfile.altitudeKp * error;

        // Total
        float thrust = hoverThrust + adjustment;

        // Clamp para que no se pase
        return Mathf.Clamp(thrust, 0f, flightProfile.maxVerticalThrust);
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