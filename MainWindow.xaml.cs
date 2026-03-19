using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;
using CrossHairPlus.Models;
using CrossHairPlus.Services;

namespace CrossHairPlus;

public partial class MainWindow : Window
{
    private readonly SettingsService _settingsService;
    private CrosshairSettings _settings;
    private OverlayWindow? _overlayWindow;
    private bool _isInitializing = true;

    public MainWindow()
    {
        InitializeComponent();
        _settingsService = new SettingsService();
        _settings = _settingsService.Load();

        Loaded += OnLoaded;
    }

    private void OnLoaded(object sender, RoutedEventArgs e)
    {
        InitializeMonitors();
        LoadSettingsToUI();
        UpdatePreview();
        _isInitializing = false;
    }

    private void InitializeMonitors()
    {
        var screens = System.Windows.Forms.Screen.AllScreens;
        MonitorCombo.Items.Clear();

        for (int i = 0; i < screens.Length; i++)
        {
            var screen = screens[i];
            var displayName = screen.Primary ? $"{screen.DeviceName} (Primary)" : screen.DeviceName;
            MonitorCombo.Items.Add(new ComboBoxItem
            {
                Content = displayName,
                Tag = i
            });
        }

        if (MonitorCombo.Items.Count > 0)
        {
            MonitorCombo.SelectedIndex = Math.Min(_settings.SelectedMonitorIndex, MonitorCombo.Items.Count - 1);
        }
    }

    private void LoadSettingsToUI()
    {
        // Set crosshair type
        CrosshairTypeCombo.SelectedIndex = (int)_settings.Type;

        // Set color selection
        var colorButtons = new[] { BtnRed, BtnGreen, BtnBlue, BtnWhite, BtnBlack, BtnYellow };
        foreach (var btn in colorButtons)
        {
            if (btn.Tag?.ToString()?.Equals(_settings.Color, StringComparison.OrdinalIgnoreCase) == true)
            {
                btn.BorderBrush = new SolidColorBrush(System.Windows.Media.Colors.Yellow);
                btn.BorderThickness = new Thickness(3);
            }
            else
            {
                btn.BorderBrush = new SolidColorBrush(System.Windows.Media.Color.FromRgb(0x66, 0x66, 0x66));
                btn.BorderThickness = new Thickness(2);
            }
        }

        // Set sliders
        SizeSlider.Value = _settings.Size;
        ThicknessSlider.Value = _settings.Thickness;
        OffsetXSlider.Value = _settings.OffsetX;
        OffsetYSlider.Value = _settings.OffsetY;

        // Update labels
        UpdateLabels();
    }

    private void UpdateLabels()
    {
        SizeLabel.Content = $"Size: {SizeSlider.Value:F0}";
        ThicknessLabel.Content = $"Thickness: {ThicknessSlider.Value:F0}";
        OffsetXLabel.Content = $"Offset X: {OffsetXSlider.Value:F0}";
        OffsetYLabel.Content = $"Offset Y: {OffsetYSlider.Value:F0}";
    }

    private void OnSettingChanged(object sender, RoutedEventArgs e)
    {
        if (_isInitializing) return;

        UpdateSettingsFromUI();
        UpdateOverlay();
    }

    private void OnMonitorChanged(object sender, SelectionChangedEventArgs e)
    {
        if (_isInitializing || MonitorCombo.SelectedItem == null) return;

        var selectedItem = (ComboBoxItem)MonitorCombo.SelectedItem;
        _settings.SelectedMonitorIndex = (int)selectedItem.Tag;

        UpdateOverlay();
    }

    private void OnColorClick(object sender, RoutedEventArgs e)
    {
        if (sender is System.Windows.Controls.Button btn && btn.Tag is string color)
        {
            _settings.Color = color;

            // Update button borders
            var colorButtons = new[] { BtnRed, BtnGreen, BtnBlue, BtnWhite, BtnBlack, BtnYellow };
            foreach (var b in colorButtons)
            {
                if (b.Tag?.ToString()?.Equals(color, StringComparison.OrdinalIgnoreCase) == true)
                {
                    b.BorderBrush = new SolidColorBrush(System.Windows.Media.Colors.Yellow);
                    b.BorderThickness = new Thickness(3);
                }
                else
                {
                    b.BorderBrush = new SolidColorBrush(System.Windows.Media.Color.FromRgb(0x66, 0x66, 0x66));
                    b.BorderThickness = new Thickness(2);
                }
            }

            UpdateOverlay();
        }
    }

    private void UpdateSettingsFromUI()
    {
        _settings.Type = (CrosshairType)CrosshairTypeCombo.SelectedIndex;
        _settings.Size = SizeSlider.Value;
        _settings.Thickness = ThicknessSlider.Value;
        _settings.OffsetX = OffsetXSlider.Value;
        _settings.OffsetY = OffsetYSlider.Value;

        UpdateLabels();
    }

    private void UpdateOverlay()
    {
        if (_overlayWindow != null)
        {
            UpdateSettingsFromUI();
            _overlayWindow.UpdateSettings(_settings);
        }
        UpdatePreview();
    }

    private void UpdatePreview()
    {
        PreviewCanvas.Children.Clear();

        var size = 80.0;
        var centerX = size / 2;
        var centerY = size / 2;

        try
        {
            var color = (System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString(_settings.Color);
            var brush = new SolidColorBrush(color);
            var pen = new System.Windows.Media.Pen(brush, _settings.Thickness);

            var scale = size / _settings.Size;
            var visualSize = _settings.Size * scale;
            var visualThickness = _settings.Thickness * scale;
            var visualCenter = visualSize / 2;
            var gap = visualThickness;

            switch (_settings.Type)
            {
                case CrosshairType.Dot:
                    var dotRadius = Math.Max(visualThickness, 2);
                    var dot = new Ellipse
                    {
                        Width = dotRadius * 2,
                        Height = dotRadius * 2,
                        Fill = brush
                    };
                    Canvas.SetLeft(dot, centerX - dotRadius);
                    Canvas.SetTop(dot, centerY - dotRadius);
                    PreviewCanvas.Children.Add(dot);
                    break;

                case CrosshairType.Cross:
                    var crossColor = new SolidColorBrush(color);
                    var crossPen = new System.Windows.Media.Pen(crossColor, visualThickness);

                    PreviewCanvas.Children.Add(CreateLine(centerX, 0, centerX, centerY - gap, crossPen));
                    PreviewCanvas.Children.Add(CreateLine(centerX, centerY + gap, centerX, size, crossPen));
                    PreviewCanvas.Children.Add(CreateLine(0, centerY, centerX - gap, centerY, crossPen));
                    PreviewCanvas.Children.Add(CreateLine(centerX + gap, centerY, size, centerY, crossPen));
                    break;

                case CrosshairType.Circle:
                    var circle = new Ellipse
                    {
                        Width = (visualSize - visualThickness),
                        Height = (visualSize - visualThickness),
                        Stroke = brush,
                        StrokeThickness = visualThickness
                    };
                    Canvas.SetLeft(circle, visualThickness / 2);
                    Canvas.SetTop(circle, visualThickness / 2);
                    PreviewCanvas.Children.Add(circle);
                    break;

                case CrosshairType.CrossCircle:
                    var crossCircle = new Ellipse
                    {
                        Width = (visualSize - visualThickness),
                        Height = (visualSize - visualThickness),
                        Stroke = brush,
                        StrokeThickness = visualThickness
                    };
                    Canvas.SetLeft(crossCircle, visualThickness / 2);
                    Canvas.SetTop(crossCircle, visualThickness / 2);
                    PreviewCanvas.Children.Add(crossCircle);

                    var ccPen = new System.Windows.Media.Pen(brush, visualThickness);
                    PreviewCanvas.Children.Add(CreateLine(0, centerY, centerX - gap, centerY, ccPen));
                    PreviewCanvas.Children.Add(CreateLine(centerX + gap, centerY, size, centerY, ccPen));
                    PreviewCanvas.Children.Add(CreateLine(centerX, 0, centerX, centerY - gap, ccPen));
                    PreviewCanvas.Children.Add(CreateLine(centerX, centerY + gap, centerX, size, ccPen));
                    break;
            }
        }
        catch
        {
            // Ignore color parsing errors in preview
        }
    }

    private Line CreateLine(double x1, double y1, double x2, double y2, System.Windows.Media.Pen pen)
    {
        return new Line
        {
            X1 = x1,
            Y1 = y1,
            X2 = x2,
            Y2 = y2,
            Stroke = pen.Brush,
            StrokeThickness = pen.Thickness
        };
    }

    private void OnToggleOverlay(object sender, RoutedEventArgs e)
    {
        if (_overlayWindow != null && _overlayWindow.IsLoaded)
        {
            _overlayWindow.ToggleVisibility();
        }
    }

    private void OnApply(object sender, RoutedEventArgs e)
    {
        // Explicitly apply settings to overlay
        UpdateSettingsFromUI();
        UpdateOverlay();
    }

    private void OnResetOffset(object sender, RoutedEventArgs e)
    {
        _settings.OffsetX = 0;
        _settings.OffsetY = 0;
        OffsetXSlider.Value = 0;
        OffsetYSlider.Value = 0;
        UpdateOverlay();
    }

    private void OnClose(object sender, RoutedEventArgs e)
    {
        _settingsService.Save(_settings);
        if (_overlayWindow != null)
        {
            _overlayWindow.ForceClose();
        }
        System.Windows.Application.Current.Shutdown();
    }

    public void SetOverlayWindow(OverlayWindow overlayWindow)
    {
        _overlayWindow = overlayWindow;
    }
}