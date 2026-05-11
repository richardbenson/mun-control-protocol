using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;
using KSPMissionControl.Shared.Models;
using Xunit;

namespace KSPMissionControl.Shared.Tests;

public sealed class TechNodeTests
{
    private static readonly JsonSerializerOptions Options = new()
    {
        PropertyNameCaseInsensitive = true,
        Converters = { new JsonStringEnumConverter() },
    };

    [Fact]
    public void TechNode_RoundTrips_ThroughSystemTextJson()
    {
        var original = new TechNode
        {
            Id = "basicRocketry",
            Title = "Basic Rocketry",
            ScienceCost = 5,
            Status = TechNodeStatus.Unlocked,
            PartNames = new List<string> { "fuelTankSmall", "liquidEngine" },
        };

        var json = JsonSerializer.Serialize(original, Options);
        var result = JsonSerializer.Deserialize<TechNode>(json, Options)!;

        Assert.Equal(original.Id, result.Id);
        Assert.Equal(original.Title, result.Title);
        Assert.Equal(original.ScienceCost, result.ScienceCost);
        Assert.Equal(original.Status, result.Status);
        Assert.Equal(original.PartNames, result.PartNames);
    }

    [Theory]
    [InlineData("Locked", TechNodeStatus.Locked)]
    [InlineData("Available", TechNodeStatus.Available)]
    [InlineData("Unlocked", TechNodeStatus.Unlocked)]
    public void TechNode_DeserializesStatusFromString(string statusString, TechNodeStatus expected)
    {
        var json = $$"""{"id":"x","title":"X","scienceCost":0,"status":"{{statusString}}","partNames":[]}""";
        var result = JsonSerializer.Deserialize<TechNode>(json, Options)!;
        Assert.Equal(expected, result.Status);
    }
}
