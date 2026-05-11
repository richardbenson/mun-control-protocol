using KSPMissionControl.MCP.Krpc;
using KSPMissionControl.MCP.Tools;
using Moq;
using Xunit;

namespace KSPMissionControl.MCP.Tests;

public sealed class BodiesToolsTests
{
    private const string KerbinJson = """{"Name":"Kerbin","Mass":5.2915158E+22,"Radius":600000.0,"AtmosphereHeight":70000.0,"SoiRadius":84159286.0,"Parent":"Sun","OrbitalPeriod":9203544.6,"SemiMajorAxis":13599840256.0}""";
    private const string MunJson    = """{"Name":"Mun","Mass":9.7599066E+20,"Radius":200000.0,"AtmosphereHeight":0.0,"SoiRadius":2429559.0,"Parent":"Kerbin","OrbitalPeriod":138984.38,"SemiMajorAxis":12000000.0}""";
    private const string SunJson    = """{"Name":"Sun","Mass":1.7565670E+28,"Radius":261600000.0,"AtmosphereHeight":600000.0,"SoiRadius":6E+13,"Parent":null,"OrbitalPeriod":null,"SemiMajorAxis":null}""";

    [Fact]
    public async Task GetBodyInfoAsync_NoFilter_ReturnsAllBodies()
    {
        var json = $"[{KerbinJson},{MunJson},{SunJson}]";
        var mock = new Mock<IKrpcConnection>();
        mock.Setup(c => c.GetBodyInfo(null)).Returns(json);

        var tool = new BodiesTools(mock.Object);
        var result = await tool.GetBodyInfoAsync();

        Assert.Equal(3, result.Count);
        mock.Verify(c => c.GetBodyInfo(null), Times.Once);
    }

    [Fact]
    public async Task GetBodyInfoAsync_WithBodyFilter_PassesFilterToConnection()
    {
        var mock = new Mock<IKrpcConnection>();
        mock.Setup(c => c.GetBodyInfo("Mun")).Returns($"[{MunJson}]");

        var tool = new BodiesTools(mock.Object);
        var result = await tool.GetBodyInfoAsync(body: "Mun");

        Assert.Single(result);
        mock.Verify(c => c.GetBodyInfo("Mun"), Times.Once);
    }

    [Fact]
    public async Task GetBodyInfoAsync_MapsKerbinCorrectly()
    {
        var mock = new Mock<IKrpcConnection>();
        mock.Setup(c => c.GetBodyInfo(null)).Returns($"[{KerbinJson}]");

        var tool = new BodiesTools(mock.Object);
        var result = await tool.GetBodyInfoAsync();

        var b = Assert.Single(result);
        Assert.Equal("Kerbin", b.Name);
        Assert.Equal(600000.0, b.Radius);
        Assert.Equal(70000.0, b.AtmosphereHeight);
        Assert.Equal(84159286.0, b.SoiRadius);
        Assert.Equal("Sun", b.Parent);
        Assert.NotNull(b.OrbitalPeriod);
        Assert.NotNull(b.SemiMajorAxis);
    }

    [Fact]
    public async Task GetBodyInfoAsync_SunHasNullParentAndOrbit()
    {
        var mock = new Mock<IKrpcConnection>();
        mock.Setup(c => c.GetBodyInfo(null)).Returns($"[{SunJson}]");

        var tool = new BodiesTools(mock.Object);
        var result = await tool.GetBodyInfoAsync();

        var b = Assert.Single(result);
        Assert.Equal("Sun", b.Name);
        Assert.Null(b.Parent);
        Assert.Null(b.OrbitalPeriod);
        Assert.Null(b.SemiMajorAxis);
    }

    [Fact]
    public async Task GetBodyInfoAsync_MunHasNoAtmosphere()
    {
        var mock = new Mock<IKrpcConnection>();
        mock.Setup(c => c.GetBodyInfo(null)).Returns($"[{MunJson}]");

        var tool = new BodiesTools(mock.Object);
        var result = await tool.GetBodyInfoAsync();

        var b = Assert.Single(result);
        Assert.Equal(0.0, b.AtmosphereHeight);
        Assert.Equal("Kerbin", b.Parent);
    }
}
