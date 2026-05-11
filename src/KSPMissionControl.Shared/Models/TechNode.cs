using System.Collections.Generic;

namespace KSPMissionControl.Shared.Models;

public sealed class TechNode
{
    public string Id { get; set; } = "";
    public string Title { get; set; } = "";
    public int ScienceCost { get; set; }
    public TechNodeStatus Status { get; set; }
    public IReadOnlyList<string> PartNames { get; set; } = new List<string>();
}

public enum TechNodeStatus
{
    Locked = 0,
    Available = 1,
    Unlocked = 2,
}
