using System.Collections.Generic;
using MunControlProtocol.Shared.Models.PartModules;

namespace MunControlProtocol.Shared.Models;

public sealed class CraftDesign
{
    public string Name { get; set; } = "";
    public string EditorType { get; set; } = "";
    public int PartCount { get; set; }
    public double TotalMassT { get; set; }
    public double TotalCost { get; set; }
    public int CrewCapacity { get; set; }
    public IList<CraftPart> Parts { get; set; } = new List<CraftPart>();
}

public sealed class CraftPart
{
    public string Name { get; set; } = "";
    public string Title { get; set; } = "";
    public double MassT { get; set; }
    public double ResourceMassT { get; set; }
    public double Cost { get; set; }
    public int StageIndex { get; set; }
    public IList<CraftResource> Resources { get; set; } = new List<CraftResource>();

    public EngineInfo? Engine { get; set; }
    public AntennaInfo? Antenna { get; set; }
    public TankInfo? Tank { get; set; }
    public CommandInfo? Command { get; set; }
    public SolarPanelInfo? SolarPanel { get; set; }
}

public sealed class CraftResource
{
    public string Name { get; set; } = "";
    public double Amount { get; set; }
    public double MaxAmount { get; set; }
}
