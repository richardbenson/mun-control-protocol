using MunControlProtocol.MCP.Krpc;
using MunControlProtocol.Shared.Models;
using MunControlProtocol.Shared.Models.PartModules;
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
    public Task<IReadOnlyList<PartInfoMcp>> GetPartsByCategoryAsync(string category)
    {
        if (string.IsNullOrWhiteSpace(category))
            throw new ArgumentException("category is required.");
        var json = connection.GetPartsByCategory(category);
        var parts = JsonSerializer.Deserialize<List<PartInfo>>(json, PartsOptions)
            ?? new List<PartInfo>();
        return Task.FromResult<IReadOnlyList<PartInfoMcp>>(
            parts.Select(p => PartInfoMcp.From(p, includeModules: false)).ToList());
    }

    /// <summary>Returns full part stats including module details (engine thrust/Isp, antenna range, tank resources, etc.). Provide part_name for a single part or category for all parts in that category. At least one is required. isPurchased=false means the tech node is unlocked but a funds purchase is still needed.</summary>
    [McpServerTool(Name = "get_part_stats")]
    public Task<IReadOnlyList<PartInfoMcp>> GetPartStatsAsync(string? part_name = null, string? category = null)
    {
        if (string.IsNullOrWhiteSpace(part_name) && string.IsNullOrWhiteSpace(category))
            throw new ArgumentException("Provide either part_name or category.");

        if (!string.IsNullOrWhiteSpace(part_name))
        {
            var json = connection.GetPartByName(part_name!);
            if (json == "null" || string.IsNullOrEmpty(json))
                return Task.FromResult<IReadOnlyList<PartInfoMcp>>(new List<PartInfoMcp>());
            var part = JsonSerializer.Deserialize<PartInfo>(json, PartsOptions);
            var result = part is null
                ? new List<PartInfoMcp>()
                : new List<PartInfoMcp> { PartInfoMcp.From(part) };
            return Task.FromResult<IReadOnlyList<PartInfoMcp>>(result);
        }

        var catJson = connection.GetPartsByCategory(category!);
        var catParts = JsonSerializer.Deserialize<List<PartInfo>>(catJson, PartsOptions)
            ?? new List<PartInfo>();
        return Task.FromResult<IReadOnlyList<PartInfoMcp>>(
            catParts.Select(p => PartInfoMcp.From(p)).ToList());
    }
}

internal sealed class PartInfoMcp
{
    public string Name { get; init; } = "";
    public string Title { get; init; } = "";
    public string Category { get; init; } = "";
    public double MassDry { get; init; }
    public double MassWet { get; init; }
    public double Cost { get; init; }
    public string TechRequired { get; init; } = "";
    public bool IsPurchased { get; init; }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public EngineInfo? Engine { get; init; }
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public AntennaInfo? Antenna { get; init; }
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public TankInfo? Tank { get; init; }
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public CommandInfo? Command { get; init; }
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public SolarPanelInfo? SolarPanel { get; init; }

    public static PartInfoMcp From(PartInfo p, bool includeModules = true) => new()
    {
        Name = p.Name,
        Title = p.Title,
        Category = p.Category,
        MassDry = p.MassDry,
        MassWet = p.MassWet,
        Cost = p.Cost,
        TechRequired = p.TechRequired,
        IsPurchased = p.IsPurchased,
        Engine = includeModules ? p.Engine : null,
        Antenna = includeModules ? p.Antenna : null,
        Tank = includeModules ? p.Tank : null,
        Command = includeModules ? p.Command : null,
        SolarPanel = includeModules ? p.SolarPanel : null,
    };
}
