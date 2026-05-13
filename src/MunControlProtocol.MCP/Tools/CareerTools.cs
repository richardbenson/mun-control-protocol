using System.Text.Json;
using System.Text.Json.Serialization;
using MunControlProtocol.MCP.Krpc;
using MunControlProtocol.Shared.Models;
using ModelContextProtocol.Server;

namespace MunControlProtocol.MCP.Tools;

[McpServerToolType]
internal sealed class CareerTools(IKrpcConnection connection)
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        Converters = { new JsonStringEnumConverter() },
    };

    /// <summary>Returns current career-mode currencies: funds, science, and reputation.</summary>
    [McpServerTool(Name = "get_career_state")]
    public Task<CareerState> GetCareerStateAsync() =>
        Task.FromResult(new CareerState
        {
            Funds = connection.Funds,
            Science = connection.Science,
            Reputation = connection.Reputation
        });

    /// <summary>
    /// Returns tech tree nodes. By default returns all nodes without their part lists (Id, Title, ScienceCost, Status only).
    /// Use status to filter: Locked, Available, or Unlocked.
    /// Use include_parts=true to include PartNames on each node.
    /// </summary>
    [McpServerTool(Name = "get_tech_tree")]
    public Task<IReadOnlyList<TechNodeMcp>> GetTechTreeAsync(TechNodeStatus? status = null, bool include_parts = false)
    {
        var json = connection.GetTechTree();
        var nodes = JsonSerializer.Deserialize<List<TechNode>>(json, JsonOptions)
            ?? new List<TechNode>();

        IEnumerable<TechNode> filtered = status.HasValue
            ? nodes.Where(n => n.Status == status.Value)
            : nodes;

        var result = filtered.Select(n => new TechNodeMcp
        {
            Id = n.Id,
            Title = n.Title,
            ScienceCost = n.ScienceCost,
            Status = n.Status,
            PartNames = include_parts ? n.PartNames : null,
        }).ToList();

        return Task.FromResult<IReadOnlyList<TechNodeMcp>>(result);
    }

    /// <summary>Returns the upgrade level of each KSC facility (0-indexed; 0 = unupgraded, max varies per building).</summary>
    [McpServerTool(Name = "get_building_levels")]
    public Task<BuildingLevels> GetBuildingLevelsAsync()
    {
        var json = connection.GetBuildingLevels();
        var levels = JsonSerializer.Deserialize<BuildingLevels>(json, JsonOptions)
            ?? new BuildingLevels();
        return Task.FromResult(levels);
    }

    /// <summary>Returns all relevant career difficulty modifiers including CommNet, science/funds rewards, reentry heating, crash tolerance.</summary>
    [McpServerTool(Name = "get_difficulty_settings")]
    public Task<DifficultySettings> GetDifficultySettingsAsync()
    {
        var json = connection.GetDifficultySettings();
        var settings = JsonSerializer.Deserialize<DifficultySettings>(json, JsonOptions)
            ?? new DifficultySettings();
        return Task.FromResult(settings);
    }
}

internal sealed class TechNodeMcp
{
    public string Id { get; init; } = "";
    public string Title { get; init; } = "";
    public int ScienceCost { get; init; }
    public TechNodeStatus Status { get; init; }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public IReadOnlyList<string>? PartNames { get; init; }
}
