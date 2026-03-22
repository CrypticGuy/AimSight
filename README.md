<h1><img src="docs/tray-icon.png" width="32" height="32" align="center"/> AimSight</h1>

A crosshair overlay tool for Windows games that renders customizable aim assistance visuals on top of any DirectX 11 game.

![CrossHairPlus](https://img.shields.io/badge/.NET-9.0-blue) ![Platform](https://img.shields.io/badge/Platform-Windows-blue)

## Features

- **Game Overlay**: Renders crosshairs directly on top of running games
- **Customizable Crosshairs**: Multiple styles, colors, sizes, and opacity settings
- **Process Injection**: Automatically injects into selected games using EasyHook
- **System Tray**: Runs quietly in background while gaming
- **Hotkey Support**: Toggle overlay visibility with customizable hotkeys
- **IPC Communication**: Reliable communication between main app and game-injected hook

## Technology Stack

| Component | Technology |
|-----------|------------|
| Main App | .NET 9.0 WPF (C#) |
| Hook Library | .NET Framework 4.8 (C#) |
| Graphics | SharpDX DirectX 11 |
| Injection | EasyHook 2.7.2 |
| IPC | Named Pipes |

## Project Structure

```
CrossHairPlus/
├── CrossHairPlus.sln          # Visual Studio solution
├── CrossHairPlus/             # Main WPF application
│   ├── Services/              # Settings and injection management
│   ├── Renderers/             # Crosshair rendering
│   └── Models/                # Data models
└── OverlayHook/               # DirectX 11 hook library
    ├── Dx11Hook.cs            # DX11 Present hook
    ├── IpcServer.cs            # IPC server
    └── OverlayRenderer.cs      # Crosshair renderer
```

## Build Instructions

### Prerequisites

- Visual Studio 2022 or later
- .NET 9.0 SDK
- .NET Framework 4.8 SDK

### Building

1. Clone the repository
2. Open `CrossHairPlus.sln` in Visual Studio
3. Select Release configuration
4. Build solution (Ctrl+Shift+B)

The built executable will be located at:
```
CrossHairPlus/bin/Release/net9.0-windows/AimSight.exe
```

## Usage

1. Launch `AimSight.exe`
2. Select a running DirectX 11 game from the process list
3. Configure your crosshair style, color, and size
4. Click "Inject" to attach the overlay
5. Press Insert (or your configured hotkey) to toggle the overlay

> **Important:** For best results, run your game in **borderless windowed** or **windowed** mode. Running in fullscreen exclusive mode may cause the overlay to not display properly or appear behind the game window.

## Recent Updates

- **Improved rendering and visibility**: Fixed issues where the crosshair would not display correctly or would become hidden behind the game window
- **Size scaling fixes**: Corrected crosshair size scaling across different crosshair types
- **App renaming**: Application rebranded to AimSight with updated icon and branding
- **Initial release**: Complete crosshair overlay application with DirectX 11 injection support

## Future Development

 Planned improvements for upcoming releases:

- **Enhanced injection reliability**: Improvements to process injection stability across a wider range of games
- **Broader game compatibility**: Extended support for additional DirectX 11 titles
- **Custom crosshair profiles**: Save and load multiple crosshair configurations
- **Game-specific settings**: Per-game crosshair preferences that auto-detect running games

## Architecture

## Architecture

The application consists of two components:

1. **Main Application (WPF)**: Provides the UI for configuration and renders the crosshair to a transparent overlay window

2. **Overlay Hook (DLL)**: Injected into the target game process where it hooks DirectX 11's Present function to coordinate frame rendering with the overlay

Communication between the two components uses named pipes (IPC).

## License

MIT License - see [LICENSE](LICENSE) file for details.

## Acknowledgments

- [EasyHook](https://easyhook.github.io/) - Process injection library
- [SharpDX](https://github.com/sharpdx/SharpDX) - DirectX interop for .NET
