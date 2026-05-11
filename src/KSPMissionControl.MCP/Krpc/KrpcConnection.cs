using KRPC.Client;
using KRPC.Client.Services.KSPMissionControl;
using KRPC.Client.Services.SpaceCenter;
using SpaceCenterService = KRPC.Client.Services.SpaceCenter.Service;
using KspMcService = KRPC.Client.Services.KSPMissionControl.Service;

namespace KSPMissionControl.MCP.Krpc;

internal sealed class KrpcConnection : IKrpcConnection
{
    private Connection? _connection;
    private SpaceCenterService? _spaceCenter;
    private KspMcService? _kspMissionControl;
    private readonly object _lock = new();

    private Connection EnsureConnected()
    {
        if (_connection is not null) return _connection;
        lock (_lock)
        {
            if (_connection is not null) return _connection;
            try
            {
                _connection = new Connection("KSP Mission Control");
                return _connection;
            }
            catch (Exception ex)
            {
                throw new KrpcConnectionException(
                    "Failed to connect to kRPC server. " +
                    "Launch KSP, load a career save, and confirm the kRPC mod is running " +
                    "on the default port (50000) via the kRPC status window.", ex);
            }
        }
    }

    private SpaceCenterService SpaceCenter => _spaceCenter ??= EnsureConnected().SpaceCenter();

    private KspMcService KspMissionControl =>
        _kspMissionControl ??= EnsureConnected().KSPMissionControl();

    double IKrpcConnection.Funds => SpaceCenter.Funds;
    float IKrpcConnection.Science => SpaceCenter.Science;
    float IKrpcConnection.Reputation => SpaceCenter.Reputation;
    string IKrpcConnection.GetTechTree() => KspMissionControl.GetTechTree();
    string IKrpcConnection.GetPartsByCategory(string category) => KspMissionControl.GetPartsByCategory(category);
    string IKrpcConnection.GetPartByName(string name) => KspMissionControl.GetPartByName(name);
    string IKrpcConnection.GetScienceSubjects(string body, string situation) => KspMissionControl.GetScienceSubjects(body, situation);
    string IKrpcConnection.GetSciencePerBodySummary() => KspMissionControl.GetSciencePerBodySummary();

    public void Dispose()
    {
        _connection?.Dispose();
    }
}
