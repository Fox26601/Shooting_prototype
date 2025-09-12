# Static Camera Shooting System

A complete static camera shooting system with object pooling for Unity 6, built using SOLID OOP principles.

## Features

- **Static Camera Controller**: Mouse look and shooting with Unity's new Input System
- **Object Pooling**: Efficient bullet and target pooling system
- **Target System**: Targets spawn in a line, fall when hit, and return to pool after 5 seconds
- **Shooting Mechanics**: Physics-based bullet system with collision detection
- **Game Management**: Centralized game state management with automatic start
- **Target Limit**: Maximum 10 targets on screen at once
- **UI System**: Game time, target count, and crosshair display

## Quick Setup

### Method 1: Tools Menu Setup (Recommended)
1. Go to **Tools > Shooting System > Setup Scene**
2. Configure settings in the window
3. Click "🚀 Setup Scene" button
4. Press Play - everything is ready and game starts automatically!

### Method 2: Automatic Setup
1. Create an empty GameObject in your scene
2. Add the `SceneSetup` script to it
3. Press Play - everything is ready and game starts automatically!

### Method 3: Manual Setup
1. **Create Player**:
   - Create empty GameObject named "Static Camera"
   - Add `Camera` component
   - Add `StaticCameraController` script

2. **Create Bullet System**:
   - Create empty GameObject "Bullet Pool"
   - Add `BulletPool` script
   - Create bullet prefab (sphere with Rigidbody, Collider, and Bullet script)

3. **Create Target System**:
   - Create empty GameObject "Target Pool"
   - Add `TargetPool` script
   - Create target prefab (cube with Rigidbody, Collider, and Target script)

4. **Create Spawner**:
   - Create empty GameObject "Target Spawner"
   - Add `TargetSpawner` script
   - Position it facing the player

5. **Create Game Manager**:
   - Create empty GameObject "Game Manager"
   - Add `GameManager` script

6. **Create UI**:
   - Create Canvas with `GameUI` script
   - Add crosshair with `Crosshair` script

## Controls

- **Mouse**: Look around
- **Left Click**: Shoot

## Script Architecture

### Core Systems
- `StaticCameraController`: Professional FPS camera with smooth mouse look, FOV transitions, and shooting
- `StaticCameraControllerLegacy`: Alternative controller using legacy Input Manager
- `Bullet`: Individual bullet behavior with physics
- `BulletPool`: Manages bullet object pooling
- `Target`: Target behavior with health and falling mechanics
- `TargetPool`: Manages target object pooling
- `TargetSpawner`: Spawns targets in a straight line formation directly in front of the player

### Management
- `GameManager`: Centralized game state management with target limits
- `GameUI`: UI display and interaction with time, targets count, and restart button
- `Crosshair`: Dynamic crosshair with target detection

### Utilities
- `ITarget`: Interface for target objects
- `SceneSetup`: Complete automatic scene configuration helper with auto-start
- `SceneSetupTool`: Editor tool for scene setup via Tools menu

## Performance Features

- **Object Pooling**: Eliminates garbage collection spikes
- **Event-Driven Architecture**: Minimal Update() usage
- **Efficient Collision Detection**: Optimized physics interactions
- **Modular Design**: Easy to extend and modify

## Customization

### Bullet Settings
- Speed, lifetime, and damage in `Bullet` script
- Pool size in `BulletPool` script

### Target Settings
- Health, fall force, and return delay in `Target` script
- Pool size in `TargetPool` script

### Spawner Settings
- Spawn distance, interval, and target count in `TargetSpawner` script

### Camera Settings
- Mouse sensitivity, smoothing, and FOV settings in `StaticCameraController` script

### Prefab System
- **Bullet Prefab**: Located in `Assets/Prefabs/Bullet Prefab.prefab`
- **Target Prefab**: Located in `Assets/Prefabs/Target Prefab.prefab`
- System automatically loads prefab references from the Prefabs folder
- Links prefab references to pools without instantiating in scene
- No fallback creation - uses your custom prefabs only

### Game Settings
- Maximum targets limit in `GameManager` script (default: 10)
- Auto-start game in `SceneSetup` script

## Auto-Setup Features

The enhanced `SceneSetup` script and `SceneSetupTool` provide complete automation:

### What Gets Created Automatically:
- **Environment**: Ground, walls, and lighting
- **Camera System**: Static camera with shooting controls
- **Object Pools**: Bullet and target pooling systems
- **Spawner**: Target spawner positioned correctly
- **UI System**: Complete UI with time, targets count, status, and restart button
- **Crosshair**: Dynamic crosshair with target detection
- **Game Manager**: Centralized game state management
- **Prefabs**: Bullet and target prefabs with proper components

### Automatic Linking:
- All component references are automatically connected
- Prefabs are assigned to pools
- UI elements are linked to GameUI script
- GameManager references all systems
- Scene validation ensures everything works

### Smart Features:
- **Prefab Reference System**: Links Bullet and Target prefab references from Assets/Prefabs/ folder
- **Smart Object Detection**: Checks for existing objects to avoid duplicates
- **Component Validation**: Adds missing components to existing objects
- **Detailed Logging**: Reports what was created vs what already existed
- **Automatic Game Start**: Game starts immediately after setup
- **Target Limit Enforcement**: Maximum 10 targets on screen
- **Line Spawn System**: Targets spawn in a straight line directly in front of the player
- **Professional FPS Camera**: Smooth mouse look with configurable sensitivity and smoothing
- **FOV System**: Dynamic field of view transitions for aiming
- **Optimized Spawner Settings**: Immediate gameplay ready with perfect positioning
- **Tools Menu Integration**: Easy access via Unity's Tools menu
- **Visual Configuration**: GUI window for all settings
- **Quick Setup**: One-click setup with default settings
- **Scene Clearing**: Remove all objects with one click
- **UI System**: Complete UI with time, targets count, status, and restart button

## Troubleshooting

1. **Input System errors**: The system automatically falls back to legacy input if Input System package is not available
2. **Targets not spawning**: Check TargetSpawner position and settings
3. **Prefab loading issues**: Ensure prefabs exist in `Assets/Prefabs/` folder. System only links references, no fallback creation
4. **Camera jitter**: Check mouse sensitivity and smoothing settings in `StaticCameraController`
5. **Bullets not firing**: Verify BulletPool is properly initialized
6. **Performance issues**: Adjust pool sizes and spawn intervals

## Extensions

The system is designed to be easily extensible:
- Add different weapon types by extending the bullet system
- Implement scoring system by modifying GameManager
- Add sound effects through event system
- Create different target types implementing ITarget interface
