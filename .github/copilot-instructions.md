# Multi-Agent System - Unity Project

This is a Unity 6000.0.40f1 (Unity 6 LTS) project for developing multi-agent systems with 3D simulation capabilities. The project uses Universal Render Pipeline (URP) and includes assets for agent navigation, perception, scenarios, and sensors.

**Always reference these instructions first and fallback to search or bash commands only when you encounter unexpected information that does not match the info here.**

## Quick Start (First Time Setup)

1. **Install Unity Hub** from https://unity.com/download
2. **Install Unity 6000.0.40f1** through Unity Hub with Linux/Windows build support
3. **Open Project**: In Unity Hub, click "Add" → select `RetoMultiagentes` folder
4. **Wait for Import**: First import takes 15-30 minutes. NEVER CANCEL - be patient
5. **Verify Setup**: Open MainScene.unity and press Play - should load without errors
6. **Ready to Code**: Scripts are in `Assets/Scripts/`, start with modifying `HelloWorld.cs`

## Working Effectively

### Prerequisites and Installation
- **CRITICAL**: Unity 6000.0.40f1 (Unity 6 LTS) is required. Do not attempt to use older versions.
- **Unity Hub Installation**:
  - Go to https://unity.com/download and download Unity Hub for your platform
  - Linux: Download the .deb package or AppImage, install with `sudo dpkg -i UnityHub.deb` or run AppImage directly
  - Windows: Download and run the .exe installer
  - macOS: Download and install the .dmg file
  - **Alternative Linux**: `sudo snap install unity-hub --classic` (if snap is available)
- **Unity Editor Installation** (through Unity Hub):
  - Open Unity Hub and navigate to "Installs" tab
  - Click "Install Editor" → "Install" for version 6000.0.40f1
  - **Required modules** to select during installation:
    - Linux Build Support (IL2CPP) - for Linux builds
    - Windows Build Support (IL2CPP) - for Windows builds (if not on Windows)
    - macOS Build Support (IL2CPP) - for macOS builds (if not on macOS)
    - Documentation (recommended)
  - **TIMING**: Unity Editor download and installation takes 20-45 minutes. NEVER CANCEL.
- **System Requirements**: Ensure you have at least 10GB free space for Unity Editor installation

### Opening the Project
- **NEVER open individual scenes directly** - always open the project through Unity Hub
- In Unity Hub, click "Add" and select the `RetoMultiagentes` folder
- **FIRST-TIME SETUP**: Initial project import takes 15-30 minutes. NEVER CANCEL - set timeout to 45+ minutes
- Unity will compile scripts and import assets during first launch
- Wait for all import processes to complete before making changes

### Build and Test Process
- **Play Mode Testing**:
  - Open `Assets/Scenes/MainScene.unity` or `Assets/Scenes/SampleScene.unity`
  - Click Play button in Unity Editor
  - Verify the scene loads without errors
  - **TIMING**: Scene loading typically takes 10-30 seconds depending on complexity
- **Standalone Build**:
  - File → Build Settings
  - Add scenes to build: drag scenes from Assets/Scenes to "Scenes In Build"
  - Select target platform (PC, Mac & Linux Standalone is default)
  - Click "Build" and select output directory
  - **TIMING**: First build takes 20-45 minutes. NEVER CANCEL. Set timeout to 60+ minutes.
  - Subsequent builds: 5-15 minutes depending on changes

### Development Workflow
- **Script Development**:
  - All C# scripts are in `Assets/Scripts/`
  - Main script: `HelloWorld.cs` (template - replace with actual agent logic)
  - Use MonoBehaviour for Unity components
  - Always test in Play Mode before building
- **Asset Management**:
  - Agent models and prefabs: `Assets/Agents/` (to be created)
  - Environment assets: `Assets/free-low-poly-forest/`, `Assets/andrew-sink-full-color-selfie-3d-scan/`
  - Drone model: `Assets/drone.fbx`
  - Input actions: `Assets/InputSystem_Actions.inputactions`

## Validation and Testing

### Manual Validation Steps
- **Always run through these steps after making changes**:
  1. Open Unity Editor and let it compile all scripts (wait for progress bar in bottom-right to finish)
  2. Check Console window (Window → General → Console) for any errors or warnings
  3. Open MainScene.unity or SampleScene.unity from Assets/Scenes/
  4. Enter Play Mode (press Play button or Ctrl+P) and verify:
     - Scene loads without errors
     - No red error messages in Console
     - Game objects are visible in Scene view
     - If adding agent behaviors, verify basic movement and interaction
  5. Exit Play Mode (press Play button again or Ctrl+P) before making further changes
  
### Build Validation
- **Before committing changes**:
  1. Ensure all scripts compile without errors (no red messages in Console)
  2. Test in Play Mode with your changes
  3. **Development Build Test**: 
     - File → Build Settings → Check "Development Build"
     - Build to a test directory (faster than full build)
     - Run the built executable and verify it launches correctly
  4. **DO NOT** commit if builds fail or produce errors
  5. If using version control, verify all .meta files are included with assets

### Automated Validation Commands
- **Script Compilation Check**:
  ```bash
  # Quick compilation test (when Unity is in PATH)
  Unity -batchmode -quit -projectPath "$(pwd)/RetoMultiagentes" -executeMethod CompilationPipeline.RequestScriptCompilation -logFile compile.log
  ```
- **Asset Import Validation**:
  ```bash
  # Re-import all assets (useful after pulling changes)
  Unity -batchmode -quit -projectPath "$(pwd)/RetoMultiagentes" -executeMethod AssetDatabase.Refresh -logFile import.log
  ```

### Performance and System Requirements
- **Minimum System Requirements**:
  - 8GB RAM (16GB recommended for complex scenes)
  - Graphics card with DirectX 11 or OpenGL 4.5 support
  - 10GB free disk space for Unity Editor + project
- **Development Performance**:
  - Compilation: 30 seconds to 2 minutes for script changes
  - Scene loading: 10-30 seconds
  - **NEVER CANCEL** operations that show progress bars

## Project Structure

### Key Directories
```
RetoMultiagentes/
├── Assets/
│   ├── Agents/          # Agent prefabs and scripts (to be created)
│   ├── Common/          # Shared utilities (to be created)
│   ├── Navigation/      # Navigation and pathfinding (to be created)
│   ├── Perception/      # Sensor and perception systems (to be created)
│   ├── Scenarios/       # Simulation scenarios (to be created)
│   ├── Scenes/          # Unity scenes (MainScene, SampleScene)
│   ├── Scripts/         # C# source code
│   ├── Sensors/         # Sensor implementations (to be created)
│   ├── Settings/        # URP rendering settings
│   └── TutorialInfo/    # Unity template information
├── Packages/            # Unity package dependencies
├── ProjectSettings/     # Unity project configuration
└── UserSettings/        # User-specific settings (gitignored)
```

### Important Files
- `ProjectSettings/ProjectVersion.txt` - Unity version (6000.0.40f1)
- `Packages/manifest.json` - Package dependencies (see Package Dependencies below)
- `Assets/InputSystem_Actions.inputactions` - Input mappings for player controls
- `Assets/Settings/` - URP rendering pipeline configuration

### Package Dependencies
The project uses these Unity packages (from `Packages/manifest.json`):
- `com.unity.ai.navigation` (2.0.6) - AI Navigation and NavMesh
- `com.unity.inputsystem` (1.13.0) - New Unity Input System
- `com.unity.render-pipelines.universal` (17.0.4) - Universal Render Pipeline
- `com.unity.test-framework` (1.4.6) - Unity Test Framework
- `com.unity.timeline` (1.8.7) - Timeline system for sequences
- `com.unity.ugui` (2.0.0) - Unity UI system
- `com.unity.visualscripting` (1.9.5) - Visual scripting system
- **DO NOT** manually edit `packages-lock.json` - Unity manages this automatically

### Asset Information
- **3D Models Available**:
  - `drone.fbx` - Drone agent model
  - `andrew-sink-full-color-selfie-3d-scan/` - Character model
  - `free-low-poly-forest/` - Environment assets
- **Rendering**: Uses Universal Render Pipeline (URP) for optimized performance
- **Input System**: New Unity Input System configured for player controls

## Common Tasks and Commands

### Through Unity Editor (Recommended)
- **Opening Project**: Use Unity Hub → Add Project → Select RetoMultiagentes folder
- **Building**: File → Build Settings → Build (or Build and Run)
- **Testing**: Click Play button in Scene view
- **Package Management**: Window → Package Manager

### Command Line (Advanced)
- **Build from Command Line** (when Unity is in PATH):
  ```bash
  # Linux build
  Unity -batchmode -quit -projectPath "$(pwd)/RetoMultiagentes" -buildTarget Linux64 -buildPath "./Builds/LinuxBuild" -logFile build.log
  
  # Windows build (from Linux/macOS)
  Unity -batchmode -quit -projectPath "$(pwd)/RetoMultiagentes" -buildTarget Win64 -buildPath "./Builds/WindowsBuild" -logFile build.log
  
  # Test compilation only (faster)
  Unity -batchmode -quit -projectPath "$(pwd)/RetoMultiagentes" -executeMethod BuildPipeline.BuildAssetBundles -logFile compile.log
  ```
- **TIMING**: Command line builds take 20-60 minutes. Use timeout of 90+ minutes.
- **Log files**: Always check `build.log` or `compile.log` for errors after command line operations

### Version Control
- **Always ignore**: `Library/`, `Temp/`, `Obj/`, `Build/`, `UserSettings/`
- **Commit**: `.meta` files along with assets (critical for Unity)
- **Never commit**: Generated build files, temporary Unity files

## Troubleshooting

### Common Issues
- **"Assembly could not be loaded"**: Delete `Library/` folder and reopen project (regenerates project cache)
- **Missing scripts in Inspector**: Check all .cs files compile without errors in Console
- **Scene loading slowly**: Normal on first import (reimporting assets), subsequent loads are faster
- **Build errors**: Always check Console window (Window → General → Console) for specific error messages
- **"Package Manager Error"**: Window → Package Manager → Advanced → Reset Packages to defaults
- **Input System conflicts**: If old Input Manager warnings appear, ignore them (project uses new Input System)

### When Things Don't Work
- **First step**: Always check Unity Console (Window → General → Console) for error messages
- **Second step**: Try these in order:
  1. File → Save Project
  2. Assets → Refresh (or Ctrl+R)
  3. If still failing: Close Unity, delete `Library/` and `Temp/` folders, reopen project
- **Third step**: Verify Unity version is exactly 6000.0.40f1 in Help → About Unity
- **Unity Hub issues**: Download Unity Hub directly from unity.com if auto-updates fail
- **Script compilation stuck**: Window → General → Console, click "Clear on Play" and restart Unity

### Platform-Specific Issues
- **Linux**: If Unity crashes on startup, try running with `unity-editor --force-opengl` 
- **Windows**: If builds fail, ensure Windows 10 SDK is installed
- **macOS**: May require Xcode command line tools for builds (`xcode-select --install`)

### Performance Issues
- **Scene view lag**: Switch to 2D view mode, or close other heavy applications
- **Long compile times**: Normal for large projects with many scripts, be patient
- **Memory issues**: Close other applications, restart Unity if memory usage > 8GB
- **Slow asset imports**: Normal behavior, Unity optimizes assets in background

### Emergency Recovery
- **Project won't open**: Delete these folders and retry: `Library/`, `Temp/`, `obj/`, `Logs/`
- **Corrupted scenes**: Check git history, revert to last working commit
- **Package corruption**: Delete `Library/PackageCache/` and `Packages/packages-lock.json`, restart Unity

## Multi-Agent System Development Notes

### Project Purpose
This is a template/framework for developing multi-agent systems in Unity with:
- Agent navigation and pathfinding
- Sensor and perception systems
- Scenario management
- 3D visualization and simulation

### Development Pattern
1. Create agent prefabs in `Assets/Agents/`
2. Implement behavior scripts inheriting from MonoBehaviour
3. Set up scenarios in `Assets/Scenarios/`
4. Configure sensors and perception in respective folders
5. Test in Play Mode before building

### Key Components to Develop
- Agent controllers and AI logic
- Navigation mesh setup for environments
- Sensor implementations (vision, proximity, etc.)
- Communication systems between agents
- Scenario scripting and management

**CRITICAL**: Always test agent behaviors in Play Mode before building. Multi-agent systems can be complex to debug in built applications.

## Common File Listings

### Repository Root Files
```
.
├── .git/                           # Git repository data
├── .gitignore                      # Unity-specific gitignore
├── RetoMultiagentes/               # Main Unity project folder
└── Evidencia 2 Sistemas multiagentes.pdf  # Project documentation
```

### Unity Project Structure (RetoMultiagentes/)
```
RetoMultiagentes/
├── Assets/
│   ├── Scenes/
│   │   ├── MainScene.unity         # Main simulation scene
│   │   └── SampleScene.unity       # Template scene
│   ├── Scripts/
│   │   └── HelloWorld.cs           # Template script (replace with agent logic)
│   ├── Settings/                   # URP render pipeline settings
│   ├── TutorialInfo/               # Unity template info (can be deleted)
│   ├── InputSystem_Actions.inputactions # Input system configuration
│   ├── drone.fbx                   # Drone 3D model
│   ├── andrew-sink-full-color-selfie-3d-scan/ # Character model
│   └── free-low-poly-forest/       # Environment assets
├── Packages/
│   ├── manifest.json               # Package dependencies
│   └── packages-lock.json          # Auto-generated package lock file
├── ProjectSettings/                # Unity project configuration
├── UserSettings/                   # User-specific settings (gitignored)
├── Library/                        # Unity cache (gitignored)
└── Temp/                          # Temporary files (gitignored)
```

### Key Configuration Files Content

#### ProjectSettings/ProjectVersion.txt
```
m_EditorVersion: 6000.0.40f1
m_EditorVersionWithRevision: 6000.0.40f1 (157d81624ddf)
```

#### Packages/manifest.json (Key Dependencies)
```json
{
  "dependencies": {
    "com.unity.ai.navigation": "2.0.6",
    "com.unity.inputsystem": "1.13.0", 
    "com.unity.render-pipelines.universal": "17.0.4",
    "com.unity.test-framework": "1.4.6"
  }
}
```