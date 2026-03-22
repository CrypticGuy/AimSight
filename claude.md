# AimSight - Crosshair Overlay Tool

## Project Overview
AimSight is a Windows desktop application that renders a customizable crosshair overlay on top of games. It uses DirectX 11 hooking to inject an overlay into running games, allowing players to display aim assistance visuals without modifying game files.

## Technology Stack

### Main Application (CrossHairPlus)
- **Framework**: .NET 9.0 Windows (WPF)
- **Languages**: C# 12+
- **Key Libraries**:
  - SharpDX.Direct3D11 (DirectX 11 interop)
  - SharpDX.DXGI (DXGI interfaces)
  - EasyHook 2.7.2 (Process injection)
  - System.Drawing.Common (Graphics utilities)

### Hook Library (OverlayHook)
- **Framework**: .NET Framework 4.8 (Windows Forms)
- **Purpose**: Low-level DirectX 11 hook for game injection
- **Key Libraries**:
  - SharpDX.Direct3D11
  - SharpDX.DXGI
  - EasyHook 2.7.2
  - Newtonsoft.Json (IPC serialization)

## Solution Structure

```
CrossHairPlus/
├── CrossHairPlus.sln           # Visual Studio solution file
├── claude.md                   # This file (Claude Code documentation)
├── README.md                   # Public project documentation
├── LICENSE                     # MIT License
├── .gitignore                  # Git ignore patterns
│
├── CrossHairPlus/              # Main WPF application
│   ├── CrossHairPlus.csproj    # .NET 9.0 WPF project
│   ├── App.xaml(.cs)           # Application entry point
│   ├── MainWindow.xaml(.cs)    # Main UI window
│   ├── OverlayWindow.xaml(.cs) # Transparent overlay window
│   ├── TrayIconManager.cs      # System tray icon handling
│   │
│   ├── Models/                 # Data models
│   ├── Services/               # Business logic services
│   │   ├── SettingsService.cs  # Application settings management
│   │   └── InjectionManager.cs # Process injection via EasyHook
│   ├── Renderers/              # Crosshair rendering components
│   └── Utils/                  # Utility classes
│
└── OverlayHook/                 # DirectX hook library
    ├── OverlayHook.csproj      # .NET Framework 4.8 library
    ├── HookEntry.cs            # EasyHook entry point
    ├── Dx11Hook.cs             # DirectX 11 hook implementation
    ├── IpcServer.cs            # IPC between app and hook
    └── OverlayRenderer.cs      # Crosshair rendering in hook
```

## Key Source Files

### Main Application

| File | Purpose |
|------|---------|
| `Services/InjectionManager.cs` | Manages process injection using EasyHook to load OverlayHook into target games |
| `Services/SettingsService.cs` | Persists and loads user settings (crosshair style, colors, opacity, hotkeys) |
| `OverlayWindow.xaml.cs` | Creates and manages the transparent WPF overlay window |
| `MainWindow.xaml.cs` | Settings UI and application main window |
| `TrayIconManager.cs` | System tray icon with context menu for quick access |

### Hook Library

| File | Purpose |
|------|---------|
| `Dx11Hook.cs` | DirectX 11 Present hook to render crosshair on top of game frames |
| `IpcServer.cs` | Named pipe server for communication between main app and injected hook |
| `OverlayRenderer.cs` | Renders crosshair graphics using SharpDX |
| `HookEntry.cs` | EasyHook driver entry point that initializes the hook |

## Build Instructions

### Prerequisites
- Visual Studio 2022 or later
- .NET 9.0 SDK
- .NET Framework 4.8 SDK (for OverlayHook)

### Build Steps
1. Open `CrossHairPlus.sln` in Visual Studio
2. Set build configuration to Release
3. Build solution (Ctrl+Shift+B)
4. The main executable will be in `CrossHairPlus/bin/Release/net9.0-windows/`

### Build Output
- **Main App**: `CrossHairPlus/bin/Release/net9.0-windows/AimSight.exe`
- **Hook DLL**: `OverlayHook/bin/Release/net48/OverlayHook.dll`
- The hook DLL is copied to the main app output during build

## Architecture Notes

### Overlay Injection Flow
1. User selects a running game process
2. `InjectionManager` injects `OverlayHook.dll` into the target process using EasyHook
3. The hook creates a `Dx11Hook` that intercepts the game's Present function
4. On each frame, the hook sends a message via IPC to the main app
5. The main app renders the crosshair to an invisible overlay window
6. The overlay window is positioned above the game window using Win32 APIs

### IPC Mechanism
- Named pipes server in `OverlayHook/IpcServer.cs`
- Main app connects as client
- Messages include: hook status, frame timing, window handle info

### Key Technical Decisions
- **Separate hook project**: The DirectX hook runs in the game process, requiring .NET Framework 4.8 for broader compatibility
- **Transparent WPF overlay**: The crosshair is rendered in a separate WPF window for easier graphics programming
- **EasyHook for injection**: Battle-tested library for reliable process injection on Windows

## Development Notes

### Known Working Games
The overlay is designed to work with DirectX 11 games. Compatibility varies by game.

### Hotkeys
Default hotkey to toggle overlay visibility: Insert key (configured in settings)

### System Tray
Application minimizes to system tray when closed, allowing it to run in background while gaming.
