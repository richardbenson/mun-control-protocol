using KSPMissionControl.MCP.Krpc;
using KSPMissionControl.Shared.Models;
using ModelContextProtocol.Server;

namespace KSPMissionControl.MCP.Tools;

[McpServerToolType]
internal sealed class CareerTools(IKrpcConnection connection)
{
    /// <summary>Returns current career-mode currencies: funds, science, and reputation.</summary>
    [McpServerTool(Name = "get_career_state")]
    public Task<CareerState> GetCareerStateAsync() =>
        Task.FromResult(new CareerState
        {
            Funds = connection.Funds,
            Science = connection.Science,
            Reputation = connection.Reputation
        });
}
