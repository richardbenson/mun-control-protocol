using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace MunControlProtocol.Shared.Models;

public sealed class TechNode
{
    public string Id { get; set; } = "";
    public string Title { get; set; } = "";
    public int ScienceCost { get; set; }
    public TechNodeStatus Status { get; set; }
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public IReadOnlyList<string>? PartNames { get; set; }
}

public enum TechNodeStatus
{
    Locked = 0,
    Available = 1,
    Unlocked = 2,
}
