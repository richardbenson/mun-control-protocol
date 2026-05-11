using System.Collections.Generic;

namespace KSPMissionControl.Shared.Models.PartModules;

public sealed class EngineInfo
{
    public double ThrustVacuum { get; set; }
    public double ThrustAsl { get; set; }
    public double IspVacuum { get; set; }
    public double IspAsl { get; set; }
    public double FuelFlowVacuum { get; set; }
    public IList<string> Propellants { get; set; } = new List<string>();
}
