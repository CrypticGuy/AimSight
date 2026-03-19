using System.Windows;
using System.Windows.Media;
using CrossHairPlus.Models;

namespace CrossHairPlus.Renderers;

public class CrosshairRenderer : DrawingVisual
{
    private readonly CrosshairSettings _settings;
    private readonly System.Windows.Media.Brush _brush;
    private readonly System.Windows.Media.Pen _pen;

    public CrosshairRenderer(CrosshairSettings settings)
    {
        _settings = settings;

        var color = (System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString(settings.Color);
        _brush = new SolidColorBrush(color);
        _brush.Freeze();

        _pen = new System.Windows.Media.Pen(_brush, settings.Thickness);
        _pen.Freeze();
    }

    public void Render()
    {
        using var dc = RenderOpen();

        var centerX = _settings.Size / 2;
        var centerY = _settings.Size / 2;
        var halfSize = _settings.Size / 2;

        switch (_settings.Type)
        {
            case CrosshairType.Dot:
                RenderDot(dc, centerX, centerY);
                break;
            case CrosshairType.Cross:
                RenderCross(dc, centerX, centerY, halfSize);
                break;
            case CrosshairType.Circle:
                RenderCircle(dc, centerX, centerY, halfSize);
                break;
            case CrosshairType.CrossCircle:
                RenderCrossCircle(dc, centerX, centerY, halfSize);
                break;
        }
    }

    private void RenderDot(DrawingContext dc, double centerX, double centerY)
    {
        var radius = _settings.Thickness;
        dc.DrawEllipse(_brush, null, new System.Windows.Point(centerX, centerY), radius, radius);
    }

    private void RenderCross(DrawingContext dc, double centerX, double centerY, double halfSize)
    {
        var gap = _settings.Thickness;

        // Top line
        dc.DrawLine(_pen, new System.Windows.Point(centerX, 0), new System.Windows.Point(centerX, centerY - gap));
        // Bottom line
        dc.DrawLine(_pen, new System.Windows.Point(centerX, centerY + gap), new System.Windows.Point(centerX, _settings.Size));
        // Left line
        dc.DrawLine(_pen, new System.Windows.Point(0, centerY), new System.Windows.Point(centerX - gap, centerY));
        // Right line
        dc.DrawLine(_pen, new System.Windows.Point(centerX + gap, centerY), new System.Windows.Point(_settings.Size, centerY));
    }

    private void RenderCircle(DrawingContext dc, double centerX, double centerY, double halfSize)
    {
        dc.DrawEllipse(null, _pen, new System.Windows.Point(centerX, centerY), halfSize - _settings.Thickness / 2, halfSize - _settings.Thickness / 2);
    }

    private void RenderCrossCircle(DrawingContext dc, double centerX, double centerY, double halfSize)
    {
        // Draw circle
        dc.DrawEllipse(null, _pen, new System.Windows.Point(centerX, centerY), halfSize - _settings.Thickness / 2, halfSize - _settings.Thickness / 2);

        // Draw cross lines through center
        var gap = _settings.Thickness;
        dc.DrawLine(_pen, new System.Windows.Point(0, centerY), new System.Windows.Point(centerX - gap, centerY));
        dc.DrawLine(_pen, new System.Windows.Point(centerX + gap, centerY), new System.Windows.Point(_settings.Size, centerY));
        dc.DrawLine(_pen, new System.Windows.Point(centerX, 0), new System.Windows.Point(centerX, centerY - gap));
        dc.DrawLine(_pen, new System.Windows.Point(centerX, centerY + gap), new System.Windows.Point(centerX, _settings.Size));
    }
}