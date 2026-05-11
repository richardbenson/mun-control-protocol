using KSPMissionControl.MCP.Krpc;
using KSPMissionControl.MCP.Tools;
using Moq;
using Xunit;

namespace KSPMissionControl.MCP.Tests;

public sealed class BuildingsToolsTests
{
    private const string SampleJson = """{"vab":2,"sph":1,"launchpad":2,"runway":1,"trackingStation":2,"researchAndDevelopment":2,"astronautComplex":1,"missionControl":1,"administration":0}""";

    [Fact]
    public async Task GetBuildingLevelsAsync_ReturnsParsedLevels()
    {
        var mock = new Mock<IKrpcConnection>();
        mock.Setup(c => c.GetBuildingLevels()).Returns(SampleJson);

        var tool = new CareerTools(mock.Object);
        var result = await tool.GetBuildingLevelsAsync();

        Assert.Equal(2, result.Vab);
        Assert.Equal(1, result.Sph);
        Assert.Equal(2, result.Launchpad);
        Assert.Equal(1, result.Runway);
        Assert.Equal(2, result.TrackingStation);
        Assert.Equal(2, result.ResearchAndDevelopment);
        Assert.Equal(1, result.AstronautComplex);
        Assert.Equal(1, result.MissionControl);
        Assert.Equal(0, result.Administration);

        mock.Verify(c => c.GetBuildingLevels(), Times.Once);
    }

    [Fact]
    public async Task GetBuildingLevelsAsync_EmptyJson_ReturnsDefaults()
    {
        var mock = new Mock<IKrpcConnection>();
        mock.Setup(c => c.GetBuildingLevels()).Returns("{}");

        var tool = new CareerTools(mock.Object);
        var result = await tool.GetBuildingLevelsAsync();

        Assert.Equal(0, result.Vab);
        Assert.Equal(0, result.Administration);
    }
}
