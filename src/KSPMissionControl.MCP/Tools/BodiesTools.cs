using System.Text.Json;
using KSPMissionControl.MCP.Krpc;
using KSPMissionControl.Shared.Models;
using ModelContextProtocol.Server;

namespace KSPMissionControl.MCP.Tools;

[McpServerToolType]
internal sealed class BodiesTools(IKrpcConnection connection)
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
    };

    /// <summary>
    /// Returns celestial body data: orbital, atmospheric, and physical properties.
    /// Omit 'body' for all bodies; provide a name (e.g. 'Mun', 'Duna') for one.
    /// AtmosphereHeight is 0 for vacuum bodies. Parent is null for the Sun.
    /// OrbitalPeriod and SemiMajorAxis are null for the Sun (it has no orbit).
    /// Custom planet packs (OPM, JNSQ) are included automatically.
    /// </summary>
    [McpServerTool(Name = "get_body_info")]
    public Task<IReadOnlyList<BodyInfo>> GetBodyInfoAsync(string? body = null)
    {
        var json = connection.GetBodyInfo(body);
        var bodies = JsonSerializer.Deserialize<List<BodyInfo>>(json, JsonOptions)
            ?? new List<BodyInfo>();
        return Task.FromResult<IReadOnlyList<BodyInfo>>(bodies);
    }
}
