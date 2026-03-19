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

        // Force show and reset position on startup
        _settings.IsVisible = true;
        _settings.OffsetX = 0;
        _settings.OffsetY = 0;
        _settingsService.Save(_settings);

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

        // Calculate center of the screen in device pixels
        var centerX = screen.Bounds.Left + (screen.Bounds.Width / 2);
        var centerY = screen.Bounds.Top + (screen.Bounds.Height / 2);

        // Apply user offset
        centerX += (int)_settings.OffsetX;
        centerY += (int)_settings.OffsetY;

        // Window size
        const double fixedWindowSize = 200;

        // Position window centered at target
        // Use device pixels directly for positioning
        var newLeft = centerX - (fixedWindowSize * dpiScaleX / 2);
        var newTop = centerY - (fixedWindowSize * dpiScaleY / 2);

        // Convert back to WPF coordinates
        Left = newLeft / dpiScaleX;
        Top = newTop / dpiScaleY;

        Width = fixedWindowSize;
        Height = fixedWindowSize;
    }

    private void RenderCrosshair()
    {
        // Clear existing children
        CrosshairGrid.Children.Clear();

        var color = (System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString(_settings.Color);
        var brush = new SolidColorBrush(color);

        // User's size is from 10-100, scale to 200 window
        var size = _settings.Size * 2;  // 40 -> 80 in 200 window
        var halfSize = size / 2;
        var halfThickness = _settings.Thickness;
        var gap = halfThickness;

        switch (_settings.Type)
        {
            case CrosshairType.Dot:
                // Dot size should scale with user setting (thickness * scale factor)
                var dotSize = Math.Max(halfThickness * 2, _settings.Size / 5);
                var dot = new System.Windows.Shapes.Ellipse
                {
                    Width = dotSize,
                    Height = dotSize,
                    Fill = brush,
                    HorizontalAlignment = System.Windows.HorizontalAlignment.Center,
                    VerticalAlignment = VerticalAlignment.Center
                };
                CrosshairGrid.Children.Add(dot);
                break;

            case CrosshairType.Cross:
                // Lines from edges to center gap, scaled by user size
                // Center is at 100, size determines how far from center lines go
                var crossExtent = halfSize;  // How far from center to edge
                AddLine(CrosshairGrid, 100, 100 - crossExtent, 100, 100 - gap, brush, halfThickness);
                AddLine(CrosshairGrid, 100, 100 + gap, 100, 100 + crossExtent, brush, halfThickness);
                AddLine(CrosshairGrid, 100 - crossExtent, 100, 100 - gap, 100, brush, halfThickness);
                AddLine(CrosshairGrid, 100 + gap, 100, 100 + crossExtent, 100, brush, halfThickness);
                break;

            case CrosshairType.Circle:
                var circle = new System.Windows.Shapes.Ellipse
                {
                    Width = size,
                    Height = size,
                    Stroke = brush,
                    StrokeThickness = halfThickness,
                    HorizontalAlignment = System.Windows.HorizontalAlignment.Center,
                    VerticalAlignment = VerticalAlignment.Center
                };
                CrosshairGrid.Children.Add(circle);
                break;

            case CrosshairType.CrossCircle:
                var crossCircle = new System.Windows.Shapes.Ellipse
                {
                    Width = size,
                    Height = size,
                    Stroke = brush,
                    StrokeThickness = halfThickness,
                    HorizontalAlignment = System.Windows.HorizontalAlignment.Center,
                    VerticalAlignment = VerticalAlignment.Center
                };
                CrosshairGrid.Children.Add(crossCircle);
                // Cross lines - extend to circle edge (radius minus thickness)
                crossExtent = halfSize - halfThickness;
                AddLine(CrosshairGrid, 100, 100 - crossExtent, 100, 100 - gap, brush, halfThickness);
                AddLine(CrosshairGrid, 100, 100 + gap, 100, 100 + crossExtent, brush, halfThickness);
                AddLine(CrosshairGrid, 100 - crossExtent, 100, 100 - gap, 100, brush, halfThickness);
                AddLine(CrosshairGrid, 100 + gap, 100, 100 + crossExtent, 100, brush, halfThickness);
                break;
        }
    }

    private void AddLine(System.Windows.Controls.Grid parent, double x1, double y1, double x2, double y2, System.Windows.Media.Brush brush, double thickness)
    {
        var line = new System.Windows.Shapes.Line
        {
            X1 = x1,
            Y1 = y1,
            X2 = x2,
            Y2 = y2,
            Stroke = brush,
            StrokeThickness = thickness,
            SnapsToDevicePixels = true
        };
        parent.Children.Add(line);
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