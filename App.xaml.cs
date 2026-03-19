using System.Windows;

namespace CrossHairPlus;

public partial class App : System.Windows.Application
{
    private OverlayWindow? _overlayWindow;
    private TrayIconManager? _trayIconManager;

    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        // Create overlay window (hidden from taskbar)
        _overlayWindow = new OverlayWindow();
        _overlayWindow.Show();

        // Create tray icon manager
        _trayIconManager = new TrayIconManager(_overlayWindow);

        // Create and show MainWindow manually so we can pass the overlay reference
        var mainWindow = new MainWindow();
        mainWindow.SetOverlayWindow(_overlayWindow);
        mainWindow.Show();
    }

    protected override void OnExit(ExitEventArgs e)
    {
        _trayIconManager?.Dispose();
        base.OnExit(e);
    }
}