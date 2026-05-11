using System.Text.Json;
using KSPMissionControl.MCP.Krpc;
using KSPMissionControl.Shared.Models;
using ModelContextProtocol.Server;

namespace KSPMissionControl.MCP.Tools;

[McpServerToolType]
internal sealed class KerbalsTools(IKrpcConnection connection)
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
    };

    /// <summary>
    /// Returns the full Kerbal roster with experience levels and current assignment.
    /// Location values: Available, Assigned, KIA, Missing.
    /// AssignedVessel is the vessel name when Location is Assigned, otherwise null.
    /// </summary>
    [McpServerTool(Name = "get_kerbals")]
    public Task<IReadOnlyList<Kerbal>> GetKerbalsAsync()
    {
        var json = connection.GetKerbals();
        var kerbals = JsonSerializer.Deserialize<List<Kerbal>>(json, JsonOptions)
            ?? new List<Kerbal>();
        return Task.FromResult<IReadOnlyList<Kerbal>>(kerbals);
    }
}
