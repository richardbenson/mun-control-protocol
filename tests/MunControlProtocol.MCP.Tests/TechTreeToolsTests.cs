using System.Collections.Generic;
using System.Threading.Tasks;
using MunControlProtocol.MCP.Krpc;
using MunControlProtocol.MCP.Tools;
using MunControlProtocol.Shared.Models;
using Moq;
using Xunit;

namespace MunControlProtocol.MCP.Tests;

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
    public async Task GetTechTreeAsync_DefaultArgs_OmitsPartNames()
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
        Assert.Null(result[0].PartNames);

        Assert.Equal(TechNodeStatus.Available, result[1].Status);
        Assert.Null(result[1].PartNames);

        Assert.Equal(TechNodeStatus.Locked, result[2].Status);
        Assert.Null(result[2].PartNames);
    }

    [Fact]
    public async Task GetTechTreeAsync_IncludeParts_PopulatesPartNames()
    {
        var mock = new Mock<IKrpcConnection>();
        mock.Setup(c => c.GetTechTree()).Returns(SampleJson);

        var tool = new CareerTools(mock.Object);
        var result = await tool.GetTechTreeAsync(include_parts: true);

        Assert.Equal(3, result.Count);
        Assert.Single(result[0].PartNames!);
        Assert.Equal("fuelTankSmall", result[0].PartNames![0]);
        Assert.Single(result[1].PartNames!);
        Assert.Empty(result[2].PartNames!);
    }

    [Fact]
    public async Task GetTechTreeAsync_StatusFilter_ReturnsMatchingNodes()
    {
        var mock = new Mock<IKrpcConnection>();
        mock.Setup(c => c.GetTechTree()).Returns(SampleJson);

        var tool = new CareerTools(mock.Object);
        var result = await tool.GetTechTreeAsync(status: TechNodeStatus.Available);

        Assert.Single(result);
        Assert.Equal("stability", result[0].Id);
        Assert.Equal(TechNodeStatus.Available, result[0].Status);
    }

    [Fact]
    public async Task GetTechTreeAsync_StatusFilterWithIncludeParts_BothApply()
    {
        var mock = new Mock<IKrpcConnection>();
        mock.Setup(c => c.GetTechTree()).Returns(SampleJson);

        var tool = new CareerTools(mock.Object);
        var result = await tool.GetTechTreeAsync(status: TechNodeStatus.Unlocked, include_parts: true);

        Assert.Single(result);
        Assert.Equal("basicRocketry", result[0].Id);
        Assert.Single(result[0].PartNames!);
    }

    [Fact]
    public async Task GetTechTreeAsync_StatusFilterNoMatch_ReturnsEmpty()
    {
        var mock = new Mock<IKrpcConnection>();
        mock.Setup(c => c.GetTechTree()).Returns("[]");

        var tool = new CareerTools(mock.Object);
        var result = await tool.GetTechTreeAsync(status: TechNodeStatus.Available);

        Assert.Empty(result);
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
