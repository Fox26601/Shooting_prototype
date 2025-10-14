# Pistol System Integration Guide

## Overview
The pistol system integrates the pistol prefab with the existing shooting mechanics, providing a unified camera and shooting experience.

## Components

### 1. PistolController.cs
- **Purpose**: Main controller for pistol prefab with integrated camera
- **Features**: 
  - Camera movement (mouse look)
  - Shooting mechanics
  - Reload system
  - FOV management
  - Ammo UI integration

### 2. PistolSetup.cs
- **Purpose**: Handles loading and configuration of pistol prefab
- **Features**:
  - Loads pistol prefab from Resources/Prefabs/
  - Configures camera integration
  - Manages AudioListener setup
  - Replaces old camera system

### 3. GameManager Integration
- **Purpose**: Integrates pistol system with game initialization
- **Features**:
  - Auto-creates PistolSetup if missing
  - Validates pistol system components
  - Manages pistol system lifecycle

## Setup Instructions

### Automatic Setup
1. The system will automatically create necessary components when the game starts
2. Pistol prefab will be loaded from `Resources/Prefabs/Pistol`
3. Camera will be configured automatically

### Manual Setup
1. Go to `Tools > Shooting System > Setup Pistol System`
2. Click "Create PistolSetup GameObject"
3. Click "Setup Pistol System"
4. Click "Validate Pistol Integration" to verify

## Configuration

### PistolController Settings
- `mouseSensitivityX`: Horizontal mouse sensitivity (default: 0.6f)
- `mouseSensitivityY`: Vertical mouse sensitivity (default: 0.4f)
- `maxLookAngle`: Maximum vertical look angle (default: 80f)
- `smoothing`: Camera movement smoothing (default: 8f)
- `pistolOffset`: Position offset of pistol relative to camera

### PistolSetup Settings
- `spawnPosition`: Initial spawn position of pistol
- `spawnRotation`: Initial spawn rotation of pistol
- `replaceMainCamera`: Whether to replace existing main camera
- `destroyOldCamera`: Whether to destroy old camera after replacement

## Integration with Existing Systems

### Shooting System
- Uses existing `BulletPool` for bullet management
- Integrates with `AudioManager` for sound effects
- Compatible with existing `AmmoUI` system

### Camera System
- Replaces `StaticCameraController` functionality
- Maintains same input handling
- Preserves FOV and aiming mechanics

### Game Management
- Integrates with `GameManager` initialization
- Maintains compatibility with existing game flow
- Preserves restart functionality

## Troubleshooting

### Common Issues
1. **Pistol prefab not found**: Ensure `Pistol.fbx` is in `Resources/Prefabs/`
2. **Camera not working**: Check if `PistolController` has `Camera` component
3. **Audio not playing**: Verify `AudioListener` is attached to pistol camera
4. **Shooting not working**: Ensure `BulletPool` is properly initialized

### Validation
Use the editor tool to validate integration:
- `Tools > Shooting System > Setup Pistol System`
- Click "Validate Pistol Integration"

## Migration from StaticCameraController

The pistol system is designed to replace `StaticCameraController` while maintaining compatibility:

1. **Automatic Migration**: GameManager will handle the transition
2. **Preserved Functionality**: All camera and shooting features are maintained
3. **Enhanced Features**: Additional pistol-specific functionality

## Performance Considerations

- Pistol prefab is loaded once at startup
- Camera rendering is optimized for FPS gameplay
- Audio system integration is efficient
- UI updates are batched for performance

## Future Enhancements

- Pistol-specific animations
- Weapon switching system
- Advanced aiming mechanics
- Customizable pistol models

