namespace MunControlProtocol.Shared.Models;

public sealed class BodyInfo
{
    private const double G = 6.674e-11;

    public string Name { get; set; } = "";
    public double Mass { get; set; }
    public double Radius { get; set; }
    public double AtmosphereHeight { get; set; }
    public double SoiRadius { get; set; }
    public string? Parent { get; set; }
    public double? OrbitalPeriod { get; set; }
    public double? SemiMajorAxis { get; set; }
    public double RotationPeriodS { get; set; }

    /// <summary>Standard gravitational parameter μ = G×M in m³/s². Use this directly in orbital formula tools.</summary>
    public double GravitationalParameterM3S2 => G * Mass;
}
