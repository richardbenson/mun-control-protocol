namespace MunControlProtocol.Shared.Models;

public sealed class ScienceSubject
{
    public string Id { get; set; } = "";
    public string ExperimentId { get; set; } = "";
    public string Body { get; set; } = "";
    public string Situation { get; set; } = "";
    public string Biome { get; set; } = "";
    public string Title { get; set; } = "";
    public double Earned { get; set; }
    public double Cap { get; set; }
    public double Remaining { get; set; }
    public double SubjectValue { get; set; }
    public double ScienceMultiplier { get; set; }
}

public sealed class ScienceBodySummary
{
    public string Body { get; set; } = "";
    public int SubjectsTotal { get; set; }
    public int SubjectsCompleted { get; set; }
    public double ScienceRemaining { get; set; }
}
