using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media;
using CrossHairPlus.Models;
using CrossHairPlus.Renderers;
using CrossHairPlus.Services;

namespace CrossHairPlus;

public partial class OverlayWindow : Window
{
    private readonly SettingsService _settingsService;
    private CrosshairSettings _settings;
    private bool _isClosing;

    public OverlayWindow()
    {
        InitializeComponent();
        _settingsService = new SettingsService();
        _settings = _settingsService.Load();

        Loaded += OnLoaded;
        Closing += OnClosing;
    }

    private void OnLoaded(object sender, RoutedEventArgs e)
    {
        // Enable click-through
        var hwnd = new WindowInteropHelper(this).Handle;
        var source = HwndSource.FromHwnd(hwnd);
        source?.AddHook(WndProc);

        // Set window to be transparent to clicks and hide from Alt+Tab
        var extendedStyle = Win32.GetWindowLong(hwnd, Win32.GWL_EXSTYLE);
        Win32.SetWindowLong(hwnd, Win32.GWL_EXSTYLE, extendedStyle | Win32.WS_EX_TRANSPARENT | Win32.WS_EX_TOOLWINDOW);

        // Position and render
        UpdatePosition();
        RenderCrosshair();

        // Register global hotkeys
        RegisterHotkeys();
    }

    private void RegisterHotkeys()
    {
        var hwnd = new WindowInteropHelper(this).Handle;
        Win32.RegisterHotKey(hwnd, Win32.HOTKEY_TOGGLE, Win32.MOD_CONTROL | Win32.MOD_SHIFT | Win32.MOD_NOREPEAT, Win32.VK_H);
    }

    private IntPtr WndProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
    {
        // Handle hotkeys
        if (msg == Win32.WM_HOTKEY)
        {
            if (wParam.ToInt32() == Win32.HOTKEY_TOGGLE)
            {
                ToggleVisibility();
                handled = true;
            }
        }

        // Make window transparent to mouse clicks
        if (msg == Win32.WM_NCHITTEST)
        {
            handled = true;
            return (IntPtr)Win32.HTTRANSPARENT;
        }

        return IntPtr.Zero;
    }

    public void ToggleVisibility()
    {
        _settings.IsVisible = !_settings.IsVisible;
        Visibility = _settings.IsVisible ? Visibility.Visible : Visibility.Collapsed;
        _settingsService.Save(_settings);
    }

    public void UpdateSettings(CrosshairSettings settings)
    {
        _settings = settings;
        UpdatePosition();
        RenderCrosshair();
        _settingsService.Save(_settings);
    }

    private void UpdatePosition()
    {
        var screens = System.Windows.Forms.Screen.AllScreens;
        if (screens.Length == 0) return;

        var selectedIndex = Math.Min(_settings.SelectedMonitorIndex, screens.Length - 1);
        var screen = screens[selectedIndex];

        // Get DPI scaling factor
        var presentationSource = PresentationSource.FromVisual(this);
        var dpiScaleX = presentationSource?.CompositionTarget?.TransformToDevice.M11 ?? 1.0;
        var dpiScaleY = presentationSource?.CompositionTarget?.TransformToDevice.M22 ?? 1.0;

        // Use a fixed large window size to contain all crosshair sizes
        // Max size is 100, so use 200 to have room
        const double fixedWindowSize = 200;
        var windowSizeDevice = fixedWindowSize * dpiScaleX;

        // Calculate the target center position
        var targetCenterX = screen.Bounds.Left + (screen.Bounds.Width / 2) + (_settings.OffsetX * dpiScaleX);
        var targetCenterY = screen.Bounds.Top + (screen.Bounds.Height / 2) + (_settings.OffsetY * dpiScaleY);

        // Position the fixed-size window centered at target
        var newLeft = targetCenterX - (windowSizeDevice / 2);
        var newTop = targetCenterY - (windowSizeDevice / 2);

        // Convert from device pixels back to WPF logical coordinates
        Left = newLeft / dpiScaleX;
        Top = newTop / dpiScaleY;

        // Set fixed window size
        Width = fixedWindowSize;
        Height = fixedWindowSize;
    }

    private void RenderCrosshair()
    {
        var renderer = new CrosshairRenderer(_settings);
        renderer.Render();

        var visualBrush = new VisualBrush(renderer)
        {
            Stretch = Stretch.None,
            Viewbox = new Rect(0, 0, _settings.Size, _settings.Size),
            ViewboxUnits = BrushMappingMode.Absolute
        };

        // Use fixed size rectangle (same as window size)
        const double fixedSize = 200;

        // Calculate scale factor
        var scale = _settings.Size / fixedSize;

        // Apply scale transform to center the crosshair
        CrosshairRect.Width = fixedSize;
        CrosshairRect.Height = fixedSize;
        CrosshairRect.Fill = visualBrush;
        CrosshairRect.RenderTransform = new System.Windows.Media.ScaleTransform(scale, scale);
        CrosshairRect.RenderTransformOrigin = new System.Windows.Point(0.5, 0.5);
    }

    public CrosshairSettings GetSettings() => _settings;

    public void ResetToCenter()
    {
        _settings.OffsetX = 0;
        _settings.OffsetY = 0;
        UpdatePosition();
        _settingsService.Save(_settings);
    }

    private void OnClosing(object? sender, System.ComponentModel.CancelEventArgs e)
    {
        if (!_isClosing)
        {
            e.Cancel = true;
            Hide();
        }
    }

    public void ForceClose()
    {
        _isClosing = true;
        Close();
    }
}

public static class Win32
{
    public const int WM_NCHITTEST = 0x0084;
    public const int WM_HOTKEY = 0x0312;
    public const int HTTRANSPARENT = -1;
    public const int GWL_EXSTYLE = -20;
    public const int WS_EX_TRANSPARENT = 0x00000020;
    public const int WS_EX_TOOLWINDOW = 0x00000080;

    [DllImport("user32.dll")]
    public static extern int GetWindowLong(IntPtr hwnd, int index);

    [DllImport("user32.dll")]
    public static extern int SetWindowLong(IntPtr hwnd, int index, int newStyle);

    [DllImport("user32.dll")]
    public static extern bool RegisterHotKey(IntPtr hWnd, int id, uint fsModifiers, uint vk);

    [DllImport("user32.dll")]
    public static extern bool UnregisterHotKey(IntPtr hWnd, int id);

    public const uint MOD_CONTROL = 0x0002;
    public const uint MOD_SHIFT = 0x0004;
    public const uint MOD_NOREPEAT = 0x4000;
    public const uint VK_H = 0x48;

    public const int HOTKEY_TOGGLE = 1;
}