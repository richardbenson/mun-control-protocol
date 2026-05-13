using System.Text.Json;
using System.Text.Json.Serialization;
using MunControlProtocol.MCP.Krpc;
using MunControlProtocol.Shared.Models;
using ModelContextProtocol.Server;

namespace MunControlProtocol.MCP.Tools;

[McpServerToolType]
internal sealed class ScienceTools(IKrpcConnection connection)
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
    };

    /// <summary>Returns the player's science subject status. Subject id format: &lt;experimentId&gt;@&lt;body&gt;&lt;situation&gt;&lt;biome&gt;. Provide `body` to scope the response (strongly recommended). Optionally provide `situation` (Landed, Splashed, FlyingLow, FlyingHigh, InSpaceLow, InSpaceHigh) to narrow further.</summary>
    [McpServerTool(Name = "get_science_status")]
    public Task<ScienceStatusResult> GetScienceStatusAsync(string? body = null, string? situation = null)
    {
        if (string.IsNullOrWhiteSpace(body))
        {
            var summaryJson = connection.GetSciencePerBodySummary();
            var summary = JsonSerializer.Deserialize<List<ScienceBodySummary>>(summaryJson, JsonOptions)
                ?? new List<ScienceBodySummary>();
            return Task.FromResult(new ScienceStatusResult
            {
                Warning = "No body filter; returning per-body summary only",
                Summary = summary,
            });
        }

        var json = connection.GetScienceSubjects(body, situation ?? "");
        var subjects = JsonSerializer.Deserialize<List<ScienceSubject>>(json, JsonOptions)
            ?? new List<ScienceSubject>();
        return Task.FromResult(new ScienceStatusResult
        {
            Subjects = subjects,
        });
    }
}

internal sealed class ScienceStatusResult
{
    [JsonPropertyName("warning")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? Warning { get; init; }

    [JsonPropertyName("summary")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public IReadOnlyList<ScienceBodySummary>? Summary { get; init; }

    [JsonPropertyName("subjects")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public IReadOnlyList<ScienceSubject>? Subjects { get; init; }
}
