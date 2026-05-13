namespace MunControlProtocol.Shared.Models.PartModules;

public sealed class CommandInfo
{
    public int CrewCapacity { get; set; }
    public bool HasSas { get; set; }
    public int SasLevel { get; set; }
    public double HibernationCharge { get; set; }
}
