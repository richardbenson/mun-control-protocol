using KRPC.Client;
using KRPC.Client.Services.SpaceCenter;

namespace KSPMissionControl.MCP.Krpc;

internal sealed class KrpcConnection : IKrpcConnection
{
    private Connection? _connection;
    private Service? _spaceCenter;
    private readonly object _lock = new();

    // All kRPC SpaceCenter access in the solution goes through this property.
    internal Service SpaceCenter
    {
        get
        {
            if (_spaceCenter is not null) return _spaceCenter;
            lock (_lock)
            {
                if (_spaceCenter is not null) return _spaceCenter;
                try
                {
                    _connection = new Connection("KSP Mission Control");
                    _spaceCenter = _connection.SpaceCenter();
                    return _spaceCenter;
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
    }

    double IKrpcConnection.Funds => SpaceCenter.Funds;
    float IKrpcConnection.Science => SpaceCenter.Science;
    float IKrpcConnection.Reputation => SpaceCenter.Reputation;

    public void Dispose()
    {
        _connection?.Dispose();
    }
}
