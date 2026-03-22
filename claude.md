# AimSight - Crosshair Overlay Tool

## Project Overview

AimSight is a Windows desktop application that displays a customizable crosshair overlay on top of games. It uses a transparent WPF window positioned above games using Win32 APIs, allowing players to display aim assistance visuals without modifying game files.

> **Note:** Process injection has been temporarily disabled. The overlay displays on the selected monitor and works best with games running in borderless/windowed mode.

## Technology Stack

### Main Application (CrossHairPlus)
- **Framework**: .NET 8.0 Windows (WPF)
- **Languages**: C# 12
- **Dependencies**:
  - Newtonsoft.Json (settings serialization)
  - System.Drawing.Common (screen handling)

### Architecture

The application uses a **transparent WPF overlay** approach:
- A click-through, always-on-top WPF window renders the crosshair
- Win32 APIs (`SetWindowPos`, `SetWindowLong`) handle window positioning and click-through
- Global hotkeys (Ctrl+Shift+H) toggle overlay visibility
- Settings are persisted to `%APPDATA%\AimSight\settings.json`

## Solution Structure

```
CrossHairPlus/
├── CrossHairPlus.sln           # Visual Studio solution file
├── CLAUDE.md                   # This file (Claude Code documentation)
├── README.md                   # Public project documentation
├── LICENSE                     # MIT License
├── .gitignore                  # Git ignore patterns
│
└── CrossHairPlus/              # Main WPF application
    ├── CrossHairPlus.csproj    # .NET 8.0 WPF project
    ├── App.xaml(.cs)           # Application entry point
    ├── MainWindow.xaml(.cs)    # Settings UI window
    ├── OverlayWindow.xaml(.cs) # Transparent crosshair overlay
    ├── TrayIconManager.cs      # System tray icon handling
    │
    ├── Models/                 # Data models
    │   └── CrosshairSettings.cs
    │
    ├── Services/               # Business logic
    │   ├── SettingsService.cs  # Settings persistence
    │   └── InjectionManager.cs # Process detection (injection disabled)
    │
    ├── Renderers/              # Crosshair rendering
    │   └── CrosshairRenderer.cs
    │
    └── Utils/                  # Utility classes
        └── Win32.cs            # Win32 API interop
```

## Key Source Files

| File | Purpose |
|------|---------|
| `OverlayWindow.xaml.cs` | Creates transparent click-through overlay, handles rendering, hotkeys, and window positioning |
| `MainWindow.xaml.cs` | Settings UI for crosshair configuration |
| `TrayIconManager.cs` | System tray icon with context menu |
| `Services/SettingsService.cs` | Persists settings to JSON in AppData |
| `Services/InjectionManager.cs` | Game process detection (injection currently disabled) |
| `Models/CrosshairSettings.cs` | Crosshair configuration model |

## Build Instructions

### Prerequisites

- Visual Studio 2022 or later
- .NET 8.0 SDK

### Building

1. Open `CrossHairPlus.sln` in Visual Studio
2. Set build configuration to Release
3. Build solution (Ctrl+Shift+B)

### Build Output

- **Executable**: `CrossHairPlus/bin/Release/net8.0-windows/AimSight.exe`

## Architecture Notes

### Overlay Rendering

The crosshair is rendered directly in WPF using standard shapes (`Ellipse`, `Line`) inside a transparent overlay window. This approach:

- **Pros**: Simple, reliable, no process injection needed
- **Cons**: Requires games to run in borderless/windowed mode; overlay may not appear on top of fullscreen exclusive games

### Window Configuration

The overlay uses these Win32 extended window styles:
- `WS_EX_TRANSPARENT` - Makes window click-through
- `WS_EX_TOOLWINDOW` - Hides from Alt+Tab
- `WS_EX_TOPMOST` - Keeps window above other windows

### Hotkeys

Default hotkey to toggle overlay visibility: **Ctrl+Shift+H**

### Future Enhancements

Planned improvements for future releases:
- **Process injection**: Re-enable DirectX hook injection for fullscreen game support
- **Game-specific profiles**: Auto-detect running games and apply saved crosshair settings
- **Custom crosshair profiles**: Save and load multiple crosshair configurations
