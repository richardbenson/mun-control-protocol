using System.Text.Json;
using MunControlProtocol.MCP.Krpc;
using MunControlProtocol.Shared.Models;
using ModelContextProtocol.Server;

namespace MunControlProtocol.MCP.Tools;

[McpServerToolType]
internal sealed class EditorTools(IKrpcConnection connection)
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
    };

    /// <summary>
    /// Returns the craft currently open in the Vehicle Assembly Building (VAB) or Space Plane Hangar (SPH),
    /// or null if the editor is not open or no craft is loaded.
    /// Each part includes: name, title, dry mass (massT), resource mass (resourceMassT), cost,
    /// stageIndex (0 = last stage to fire), resources with current fill levels, and any installed
    /// modules (engine, tank, command pod, antenna, solar panel).
    /// Use stageIndex to group parts by stage, then apply the Tsiolkovsky formula (formula_delta_v)
    /// with stage wet/dry mass and mass-weighted ISP to estimate delta-v per stage.
    /// </summary>
    [McpServerTool(Name = "get_current_craft")]
    public Task<CraftDesign?> GetCurrentCraftAsync()
    {
        var json = connection.GetCurrentCraft();
        if (json == "null" || string.IsNullOrEmpty(json))
            return Task.FromResult<CraftDesign?>(null);

        var craft = JsonSerializer.Deserialize<CraftDesign>(json, JsonOptions);
        return Task.FromResult(craft);
    }
}
