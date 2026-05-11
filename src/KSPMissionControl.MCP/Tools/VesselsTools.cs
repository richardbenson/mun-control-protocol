using System.Text.Json;
using KSPMissionControl.MCP.Krpc;
using KSPMissionControl.Shared.Models;
using ModelContextProtocol.Server;

namespace KSPMissionControl.MCP.Tools;

[McpServerToolType]
internal sealed class VesselsTools(IKrpcConnection connection)
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
    };

    /// <summary>
    /// Returns all active vessels (debris excluded by default — set include_debris to true to include it).
    /// Each vessel includes name, type, situation, current SOI body, crew names, and basic orbital data
    /// (apoapsis altitude m, periapsis altitude m, inclination degrees).
    /// </summary>
    [McpServerTool(Name = "get_vessels")]
    public Task<IReadOnlyList<Vessel>> GetVesselsAsync(bool include_debris = false)
    {
        var json = connection.GetVessels(include_debris);
        var vessels = JsonSerializer.Deserialize<List<Vessel>>(json, JsonOptions)
            ?? new List<Vessel>();
        return Task.FromResult<IReadOnlyList<Vessel>>(vessels);
    }
}
