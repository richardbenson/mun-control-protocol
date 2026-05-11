namespace KSPMissionControl.Shared.Models.PartModules;

public sealed class AntennaInfo
{
    public double Range { get; set; }
    public string Type { get; set; } = "";
    public bool Combinable { get; set; }
    public double PacketSize { get; set; }
    public double PacketInterval { get; set; }
}
