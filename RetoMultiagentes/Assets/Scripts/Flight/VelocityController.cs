using UnityEngine;

public class VelocityController
{
    private FlightProfile flightProfile;
    private float previousError = 0f;
    private float integralSum = 0f;
    private float lastTime;
    private bool initialized = false;

    public VelocityController(FlightProfile profile)
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

    public float GetForwardThrust(float currentSpeed)
    {
        if (flightProfile == null)
        {
            Debug.LogError("FlightProfile is null in VelocityController");
            return 0f;
        }

        float currentTime = Time.time;
        
        if (!initialized)
        {
            lastTime = currentTime;
            initialized = true;
            return 0f; // No thrust on first call
        }

        float deltaTime = currentTime - lastTime;
        if (deltaTime <= 0f) return 0f;

        // Calculate error
        float error = flightProfile.targetSpeed - currentSpeed;

        // Proportional term
        float proportional = flightProfile.velocityKp * error;

        // Integral term (with windup prevention)
        integralSum += error * deltaTime;
        // Clamp integral to prevent windup
        float maxIntegral = flightProfile.maxForwardThrust / (flightProfile.velocityKi + 0.001f);
        integralSum = Mathf.Clamp(integralSum, -maxIntegral, maxIntegral);
        float integral = flightProfile.velocityKi * integralSum;

        // Derivative term
        float derivative = flightProfile.velocityKd * (error - previousError) / deltaTime;

        // Calculate total thrust
        float totalThrust = proportional + integral + derivative;

        // Apply thrust limits
        totalThrust = Mathf.Clamp(totalThrust, -flightProfile.maxForwardThrust, flightProfile.maxForwardThrust);

        // Update for next iteration
        previousError = error;
        lastTime = currentTime;

        return totalThrust;
    }

    public float GetCurrentError(float currentSpeed)
    {
        if (flightProfile == null) return 0f;
        return flightProfile.targetSpeed - currentSpeed;
    }
}