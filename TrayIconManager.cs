using System.Drawing;
using System.Windows;
using System.Windows.Forms;
using CrossHairPlus.Models;
using CrossHairPlus.Services;

namespace CrossHairPlus;

public class TrayIconManager : IDisposable
{
    private NotifyIcon? _notifyIcon;
    private readonly SettingsService _settingsService;
    private readonly OverlayWindow _overlayWindow;
    private MainWindow? _settingsWindow;

    public TrayIconManager(OverlayWindow overlayWindow)
    {
        _overlayWindow = overlayWindow;
        _settingsService = new SettingsService();

        CreateTrayIcon();
    }

    private void CreateTrayIcon()
    {
        _notifyIcon = new NotifyIcon
        {
            Text = "CrossHair Plus",
            Visible = true
        };

        // Create a simple crosshair icon programmatically
        using var bitmap = new Bitmap(16, 16);
        using var graphics = Graphics.FromImage(bitmap);
        graphics.Clear(Color.Transparent);
        graphics.DrawLine(Pens.Red, 8, 0, 8, 15);
        graphics.DrawLine(Pens.Red, 0, 8, 15, 8);
        graphics.DrawEllipse(Pens.Red, 2, 2, 11, 11);

        _notifyIcon.Icon = Icon.FromHandle(bitmap.GetHicon());

        // Create context menu
        var contextMenu = new ContextMenuStrip();

        var showHideItem = new ToolStripMenuItem("Show/Hide Crosshair");
        showHideItem.Click += (s, e) => _overlayWindow.ToggleVisibility();
        contextMenu.Items.Add(showHideItem);

        var settingsItem = new ToolStripMenuItem("Settings");
        settingsItem.Click += (s, e) => ShowSettings();
        contextMenu.Items.Add(settingsItem);

        contextMenu.Items.Add(new ToolStripSeparator());

        var resetItem = new ToolStripMenuItem("Reset to Center");
        resetItem.Click += (s, e) => _overlayWindow.ResetToCenter();
        contextMenu.Items.Add(resetItem);

        contextMenu.Items.Add(new ToolStripSeparator());

        var exitItem = new ToolStripMenuItem("Exit");
        exitItem.Click += (s, e) => ExitApplication();
        contextMenu.Items.Add(exitItem);

        _notifyIcon.ContextMenuStrip = contextMenu;

        // Double-click to show settings
        _notifyIcon.DoubleClick += (s, e) => ShowSettings();
    }

    public void ShowSettings()
    {
        if (_settingsWindow == null || !_settingsWindow.IsLoaded)
        {
            _settingsWindow = new MainWindow();
            _settingsWindow.SetOverlayWindow(_overlayWindow);
            _settingsWindow.Closed += (s, e) => _settingsWindow = null;
        }

        _settingsWindow.Show();
        _settingsWindow.Activate();
        _settingsWindow.WindowState = WindowState.Normal;
    }

    public void UpdateIconVisibility(bool isVisible)
    {
        if (_notifyIcon != null)
        {
            _notifyIcon.Visible = true;
        }
    }

    private void ExitApplication()
    {
        _overlayWindow.ForceClose();
        if (_settingsWindow != null)
        {
            _settingsWindow.Close();
        }
        System.Windows.Application.Current.Shutdown();
    }

    public void Dispose()
    {
        if (_notifyIcon != null)
        {
            _notifyIcon.Visible = false;
            _notifyIcon.Dispose();
            _notifyIcon = null;
        }
    }
}