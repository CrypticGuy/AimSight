using System.Text.Json.Serialization;

namespace CrossHairPlus.Models;

public enum CrosshairType
{
    Dot,
    Cross,
    Circle,
    CrossCircle
}

public class CrosshairSettings
{
    public CrosshairType Type { get; set; } = CrosshairType.Cross;
    public string Color { get; set; } = "#FF0000";
    public double Size { get; set; } = 40;
    public double Thickness { get; set; } = 2;
    public double OffsetX { get; set; } = 0;
    public double OffsetY { get; set; } = 0;
    public int SelectedMonitorIndex { get; set; } = 0;
    public bool IsVisible { get; set; } = true;
}