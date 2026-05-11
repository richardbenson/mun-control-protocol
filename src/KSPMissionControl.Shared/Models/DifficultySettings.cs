namespace KSPMissionControl.Shared.Models;

public sealed class DifficultySettings
{
    public double CommNetRangeModifier { get; set; }
    public double DsnModifier { get; set; }
    public bool RequireSignalForControl { get; set; }
    public bool RequireSignalForScience { get; set; }
    public bool EnableCommNet { get; set; }
    public bool OccludeBodies { get; set; }
    public double RangeModifier { get; set; }
    public double ScienceRewardsMultiplier { get; set; }
    public double FundsRewardsMultiplier { get; set; }
    public double ReputationRewardsMultiplier { get; set; }
    public double FundsPenaltiesMultiplier { get; set; }
    public double ReputationPenaltiesMultiplier { get; set; }
    public double ReentryHeatingMultiplier { get; set; }
    public double CrashToleranceMultiplier { get; set; }
    public bool PlasmaBlackout { get; set; }
    public double KerbalGToleranceMultiplier { get; set; }
    public bool MissingCrewsRespawn { get; set; }
}
