# Flight Control System - Issue #8

This document describes the PID-based flight control system implementation for altitude and velocity control.

## Overview

The flight control system implements PID controllers for:
- **Altitude Hold**: Maintains target altitude with configurable PID gains
- **Velocity Control**: Controls horizontal velocity for stable navigation

## Quick Setup & Testing

### 1. Unity Scene Setup
1. Open `MainScene.unity` or `SampleScene.unity`
2. Find the Drone GameObject (or create one with DroneController component)
3. Assign the `FlightProfile.asset` to the DroneController's `flightProfile` field
4. Add `FlightControlTest.cs` to any GameObject for automated validation
5. Add `FlightControlDemo.cs` for interactive waypoint demonstration

### 2. Testing the System
**Automated Test:**
- Press Play in Unity
- FlightControlTest will automatically validate RMS error < 0.5m requirement
- Check Console for test results

**Interactive Demo:**
- Attach FlightControlDemo to a GameObject
- Set waypoints in the inspector
- Enable `autoDemo` for automatic waypoint navigation
- Use context menu for manual waypoint testing

### 3. PID Tuning
- Select the FlightProfile.asset in Project window
- Adjust PID values in real-time while playing:
  - Start with default values (Kp=1.0, Ki=0.1, Kd=0.5)
  - Increase Kp for faster response
  - Increase Ki to eliminate steady-state error
  - Increase Kd to reduce overshoot

## Components

### FlightProfile.cs (ScriptableObject)
Configurable asset containing all flight parameters including PID gains.

**Key Parameters:**
- `targetAltitude`: Desired altitude in meters (default: 20m)
- `altitudeKp/Ki/Kd`: PID gains for altitude control
- `targetSpeed`: Desired horizontal speed in m/s (default: 10 m/s)
- `velocityKp/Ki/Kd`: PID gains for velocity control
- `maxVerticalThrust`/`maxForwardThrust`: Safety limits

### AltitudeHold.cs
PID controller for vertical position control with:
- Integral windup prevention
- Proper initialization handling
- Error tracking for telemetry

### VelocityController.cs
PID controller for horizontal velocity control with:
- Speed-based control (not position)
- Thrust limiting for safety
- Reset functionality for clean state

### FlightControlTest.cs
Automated testing component that validates:
- RMS altitude error < 0.5m (Issue #8 requirement)
- Velocity stability metrics
- Performance logging and reporting

### FlightControlDemo.cs
Interactive demonstration component featuring:
- Waypoint navigation system
- Real-time PID tuning presets
- Flight metrics logging
- Manual control methods

## Usage

### API Methods
- `GetAltitudeError()`: Returns current altitude error for debugging
- `GetVelocityError()`: Returns current velocity error
- `ResetPIDControllers()`: Resets controller state (useful for takeoff)

### Tuning Guide
Start with default values and adjust:
1. **Kp (Proportional)**: Increase for faster response, decrease if oscillating
2. **Ki (Integral)**: Increase to eliminate steady-state error, decrease if unstable
3. **Kd (Derivative)**: Increase to reduce overshoot, decrease if noisy

### Testing Commands
Use these context menu commands in FlightControlDemo:
- "Go to Waypoint X": Navigate to specific waypoint
- "Reset PID Controllers": Clean controller state
- "Tune PID - Conservative": Stable but slower response
- "Tune PID - Aggressive": Faster but potentially less stable

## Acceptance Criteria Compliance

✅ **RMS altitude error < 0.5m**: Validated by FlightControlTest component
✅ **Stable velocity**: PID controller prevents oscillations
✅ **Configurable parameters**: All tuning through FlightProfile ScriptableObject
✅ **PID with limits**: Both controllers implement proper limiting
✅ **Integration with DroneController**: Seamless integration with fallback support

## Architecture Notes

- Controllers are stateful classes (not MonoBehaviours) for performance
- Integral windup prevention prevents controller saturation
- Fallback to original control if FlightProfile is missing
- Clean separation of concerns: profile, controllers, and integration
- Thread-safe design for future multi-threading support