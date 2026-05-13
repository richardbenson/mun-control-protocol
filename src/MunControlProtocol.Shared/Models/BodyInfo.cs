namespace MunControlProtocol.Shared.Models;

public sealed class BodyInfo
{
    public string Name { get; set; } = "";
    public double Mass { get; set; }
    public double Radius { get; set; }
    public double AtmosphereHeight { get; set; }
    public double SoiRadius { get; set; }
    public string? Parent { get; set; }
    public double? OrbitalPeriod { get; set; }
    public double? SemiMajorAxis { get; set; }
    public double RotationPeriodS { get; set; }
}
