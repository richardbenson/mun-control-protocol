using System.Text.Json;
using System.Text.Json.Serialization;
using KSPMissionControl.MCP.Krpc;
using KSPMissionControl.Shared.Models;
using ModelContextProtocol.Server;

namespace KSPMissionControl.MCP.Tools;

[McpServerToolType]
internal sealed class CareerTools(IKrpcConnection connection)
{
    private static readonly JsonSerializerOptions TechTreeOptions = new()
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
        var nodes = JsonSerializer.Deserialize<List<TechNode>>(json, TechTreeOptions)
            ?? new List<TechNode>();
        return Task.FromResult<IReadOnlyList<TechNode>>(nodes);
    }
}
