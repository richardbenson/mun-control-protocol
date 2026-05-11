using System.Collections.Generic;
using System.Threading.Tasks;
using KSPMissionControl.MCP.Krpc;
using KSPMissionControl.MCP.Tools;
using KSPMissionControl.Shared.Models;
using Moq;
using Xunit;

namespace KSPMissionControl.MCP.Tests;

public sealed class TechTreeToolsTests
{
    private const string SampleJson = """
        [
          {"id":"basicRocketry","title":"Basic Rocketry","scienceCost":5,"status":"Unlocked","partNames":["fuelTankSmall"]},
          {"id":"stability","title":"Stability","scienceCost":18,"status":"Available","partNames":["SAS"]},
          {"id":"advConstruction","title":"Advanced Construction","scienceCost":45,"status":"Locked","partNames":[]}
        ]
        """;

    [Fact]
    public async Task GetTechTreeAsync_MapsTechNodesCorrectly()
    {
        var mock = new Mock<IKrpcConnection>();
        mock.Setup(c => c.GetTechTree()).Returns(SampleJson);

        var tool = new CareerTools(mock.Object);
        var result = await tool.GetTechTreeAsync();

        Assert.Equal(3, result.Count);

        Assert.Equal("basicRocketry", result[0].Id);
        Assert.Equal("Basic Rocketry", result[0].Title);
        Assert.Equal(5, result[0].ScienceCost);
        Assert.Equal(TechNodeStatus.Unlocked, result[0].Status);
        Assert.Single(result[0].PartNames);

        Assert.Equal(TechNodeStatus.Available, result[1].Status);
        Assert.Equal(TechNodeStatus.Locked, result[2].Status);
        Assert.Empty(result[2].PartNames);
    }

    [Fact]
    public async Task GetTechTreeAsync_ReturnsEmptyList_WhenJsonIsEmptyArray()
    {
        var mock = new Mock<IKrpcConnection>();
        mock.Setup(c => c.GetTechTree()).Returns("[]");

        var tool = new CareerTools(mock.Object);
        var result = await tool.GetTechTreeAsync();

        Assert.Empty(result);
    }
}
