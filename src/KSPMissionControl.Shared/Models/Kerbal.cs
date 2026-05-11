namespace KSPMissionControl.Shared.Models;

public sealed class Kerbal
{
    public string Name { get; set; } = "";
    public int ExperienceLevel { get; set; }
    public string Specialty { get; set; } = "";
    public string? AssignedVessel { get; set; }
    public string Location { get; set; } = "";
}
