namespace MunControlProtocol.Shared.Models;

public sealed class CareerState
{
    /// <summary>Available funds in the career (currency units).</summary>
    public double Funds { get; set; }

    /// <summary>Accumulated science points.</summary>
    public double Science { get; set; }

    /// <summary>Current reputation points.</summary>
    public double Reputation { get; set; }
}
