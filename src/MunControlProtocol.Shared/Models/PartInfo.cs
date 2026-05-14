using MunControlProtocol.Shared.Models.PartModules;

namespace MunControlProtocol.Shared.Models;

public sealed class PartInfo
{
    public string Name { get; set; } = "";
    public string Title { get; set; } = "";
    public string Category { get; set; } = "";
    public double MassDry { get; set; }
    public double MassWet { get; set; }
    public double Cost { get; set; }
    public string TechRequired { get; set; } = "";
    // True if the part is ready to use in builds. False means the tech node is unlocked but
    // the part still requires a funds purchase (only relevant when part purchasing is enabled
    // in difficulty settings — PartModelPurchased returns true when purchasing is disabled).
    public bool IsPurchased { get; set; }

    public EngineInfo? Engine { get; set; }
    public AntennaInfo? Antenna { get; set; }
    public TankInfo? Tank { get; set; }
    public CommandInfo? Command { get; set; }
    public SolarPanelInfo? SolarPanel { get; set; }
}
