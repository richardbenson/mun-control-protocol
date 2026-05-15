namespace MunControlProtocol.MCP.Krpc;

internal interface IKrpcConnection : IDisposable
{
    double Funds { get; }
    float Science { get; }
    float Reputation { get; }
    string GetTechTree();
    string GetPartsByCategory(string category);
    string GetPartByName(string name);
    string GetScienceSubjects(string body, string situation);
    string GetSciencePerBodySummary();
    string GetBuildingLevels();
    string GetDifficultySettings();
    string GetVessels(bool includeDebris);
    string GetKerbals();
    string GetCurrentCraft();
    string GetBodyInfo(string? body);
}
