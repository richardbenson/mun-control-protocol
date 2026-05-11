using System.Text.Json;
using System.Text.Json.Serialization;
using KSPMissionControl.MCP.Krpc;
using KSPMissionControl.Shared.Models;
using ModelContextProtocol.Server;

namespace KSPMissionControl.MCP.Tools;

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

    /// <summary>Returns all tech tree nodes with their unlock status and the parts contained in each node.</summary>
    [McpServerTool(Name = "get_tech_tree")]
    public Task<IReadOnlyList<TechNode>> GetTechTreeAsync()
    {
        var json = connection.GetTechTree();
        var nodes = JsonSerializer.Deserialize<List<TechNode>>(json, JsonOptions)
            ?? new List<TechNode>();
        return Task.FromResult<IReadOnlyList<TechNode>>(nodes);
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
