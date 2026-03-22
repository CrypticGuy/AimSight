<h1><img src="docs/tray-icon.png" width="32" height="32" align="center"/> AimSight</h1>

A crosshair overlay tool for Windows games that renders customizable aim assistance visuals on top of any game running in windowed or borderless mode.

![CrossHairPlus](https://img.shields.io/badge/.NET-8.0-blue) ![Platform](https://img.shields.io/badge/Platform-Windows-blue)

## Features

- **Transparent Overlay**: Click-through crosshair overlay using WPF and Win32 APIs
- **Customizable Crosshairs**: Multiple styles (Dot, Cross, Circle, CrossCircle), colors, and sizes
- **Global Hotkey**: Toggle overlay visibility with Ctrl+Shift+H
- **System Tray**: Runs quietly in background while gaming
- **Multi-Monitor Support**: Select which monitor to display the overlay on
- **Settings Persistence**: Your preferences are saved automatically

## Technology Stack

| Component | Technology |
|-----------|------------|
| Framework | .NET 8.0 WPF (C#) |
| Settings | Newtonsoft.Json |
| Window Interop | Win32 API (user32.dll) |

## Project Structure

```
CrossHairPlus/
├── CrossHairPlus.sln          # Visual Studio solution
├── CrossHairPlus/             # Main WPF application
│   ├── Services/              # Settings and process detection
│   ├── Renderers/             # Crosshair rendering
│   ├── Models/                # Data models
│   └── Utils/                 # Win32 interop
└── README.md
```

## Build Instructions

### Prerequisites

- Visual Studio 2022 or later
- .NET 8.0 SDK

### Building

1. Clone the repository
2. Open `CrossHairPlus.sln` in Visual Studio
3. Select Release configuration
4. Build solution (Ctrl+Shift+B)

The built executable will be located at:
```
CrossHairPlus/bin/Release/net8.0-windows/AimSight.exe
```

## Usage

1. Launch `AimSight.exe`
2. Configure your crosshair style, color, and size
3. Select which monitor to display the overlay on
4. Press **Ctrl+Shift+H** to toggle the overlay visibility

> **Important:** For best results, run your game in **borderless windowed** or **windowed** mode. Running in fullscreen exclusive mode may cause the overlay to not display properly or appear behind the game window.

## Architecture

The application uses a transparent WPF overlay window that is positioned above games using Win32 APIs:

1. **Overlay Window**: A click-through, always-on-top WPF window
2. **Win32 Integration**: Uses `SetWindowPos` and extended window styles to maintain topmost position
3. **Hotkey Handling**: Global hotkey registration for Ctrl+Shift+H to toggle visibility
4. **Settings Persistence**: JSON-based settings stored in `%APPDATA%\AimSight\settings.json`

### Crosshair Types

- **Dot**: Simple centered dot
- **Cross**: Four lines extending from center with a gap
- **Circle**: Hollow circle outline
- **CrossCircle**: Circle with cross lines through center

## Recent Updates

- **WPF Overlay Architecture**: Replaced DirectX hook injection with simpler transparent WPF overlay
- **Improved rendering and visibility**: Fixed issues where the crosshair would not display correctly
- **Size scaling fixes**: Corrected crosshair size scaling across different crosshair types
- **Multi-monitor support**: Added ability to select which monitor displays the overlay
- **Hotkey improvements**: Changed to Ctrl+Shift+H for toggle

## Future Development

Planned improvements for upcoming releases:

- **Process injection**: Re-enable DirectX hook injection for fullscreen exclusive game support
- **Broader game compatibility**: Extended support through game-specific detection
- **Custom crosshair profiles**: Save and load multiple crosshair configurations
- **Game-specific settings**: Per-game crosshair preferences that auto-detect running games

## License

MIT License - see [LICENSE](LICENSE) file for details.
