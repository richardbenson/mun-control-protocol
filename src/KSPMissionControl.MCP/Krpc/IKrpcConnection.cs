namespace KSPMissionControl.MCP.Krpc;

internal interface IKrpcConnection : IDisposable
{
    double Funds { get; }
    float Science { get; }
    float Reputation { get; }
    string GetTechTree();
    string GetPartsByCategory(string category);
    string GetPartByName(string name);
}
