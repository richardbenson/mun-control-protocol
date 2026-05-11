using System.Collections.Generic;

namespace KSPMissionControl.Shared.Models.PartModules;

public sealed class TankInfo
{
    public IList<ResourceCapacity> Resources { get; set; } = new List<ResourceCapacity>();
}
