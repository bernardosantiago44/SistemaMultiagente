using UnityEngine;

public class AltitudeHold
{
    private FlightProfile flightProfile;
    private float previousError = 0f;
    private float integralSum = 0f;
    private float lastTime;
    private bool initialized = false;

    public AltitudeHold(FlightProfile profile)
    {
        flightProfile = profile;
        Reset();
    }

    public void Reset()
    {
        previousError = 0f;
        integralSum = 0f;
        initialized = false;
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

        // Calculate error
        float error = flightProfile.targetAltitude - currentAltitude;

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
        return flightProfile.targetAltitude - currentAltitude;
    }
}