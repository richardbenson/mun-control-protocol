using MunControlProtocol.MCP.Krpc;
using MunControlProtocol.Shared.Models;
using ModelContextProtocol.Server;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace MunControlProtocol.MCP.Tools;

[McpServerToolType]
internal sealed class PartsTools(IKrpcConnection connection)
{
    private static readonly JsonSerializerOptions PartsOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        Converters = { new JsonStringEnumConverter() },
    };

    /// <summary>Lists unlocked parts in the given category (engine, fueltank, command, science, communication, structural, ...) with basic stats: name, title, mass, cost, techRequired, isPurchased. Module details (thrust, Isp, etc.) are excluded — use get_part_stats for those. isPurchased=false means the tech node is unlocked but a funds purchase is still needed.</summary>
    [McpServerTool(Name = "get_parts_by_category")]
    public Task<IReadOnlyList<PartInfo>> GetPartsByCategoryAsync(string category)
    {
        if (string.IsNullOrWhiteSpace(category))
            throw new ArgumentException("category is required.");
        var json = connection.GetPartsByCategory(category);
        var parts = JsonSerializer.Deserialize<List<PartInfo>>(json, PartsOptions)
            ?? new List<PartInfo>();
        foreach (var p in parts)
        {
            p.Engine = null;
            p.Antenna = null;
            p.Tank = null;
            p.Command = null;
            p.SolarPanel = null;
        }
        return Task.FromResult<IReadOnlyList<PartInfo>>(parts);
    }

    /// <summary>Returns full part stats including module details (engine thrust/Isp, antenna range, tank resources, etc.). Provide part_name for a single part or category for all parts in that category. At least one is required. isPurchased=false means the tech node is unlocked but a funds purchase is still needed.</summary>
    [McpServerTool(Name = "get_part_stats")]
    public Task<IReadOnlyList<PartInfo>> GetPartStatsAsync(string? part_name = null, string? category = null)
    {
        if (string.IsNullOrWhiteSpace(part_name) && string.IsNullOrWhiteSpace(category))
            throw new ArgumentException("Provide either part_name or category.");

        if (!string.IsNullOrWhiteSpace(part_name))
        {
            var json = connection.GetPartByName(part_name!);
            if (json == "null" || string.IsNullOrEmpty(json))
                return Task.FromResult<IReadOnlyList<PartInfo>>(new List<PartInfo>());
            var part = JsonSerializer.Deserialize<PartInfo>(json, PartsOptions);
            var result = part is null ? new List<PartInfo>() : new List<PartInfo> { part };
            return Task.FromResult<IReadOnlyList<PartInfo>>(result);
        }

        // category only
        var catJson = connection.GetPartsByCategory(category!);
        var catParts = JsonSerializer.Deserialize<List<PartInfo>>(catJson, PartsOptions)
            ?? new List<PartInfo>();
        return Task.FromResult<IReadOnlyList<PartInfo>>(catParts);
    }
}
