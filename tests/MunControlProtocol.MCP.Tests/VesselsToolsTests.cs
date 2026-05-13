using MunControlProtocol.MCP.Krpc;
using MunControlProtocol.MCP.Tools;
using Moq;
using Xunit;

namespace MunControlProtocol.MCP.Tests;

public sealed class VesselsToolsTests
{
    private const string ProbeVessel  = """{"Name":"Sentinel Probe","Type":"Probe","Situation":"Orbiting","Body":"Kerbin","CrewNames":[],"Apoapsis":150000.0,"Periapsis":149000.0,"Inclination":28.5}""";
    private const string ShipVessel   = """{"Name":"Explorer I","Type":"Ship","Situation":"Orbiting","Body":"Mun","CrewNames":["Jebediah Kerman","Valentina Kerman"],"Apoapsis":20000.0,"Periapsis":20000.0,"Inclination":0.0}""";
    private const string DebrisVessel = """{"Name":"Fairing","Type":"Debris","Situation":"Orbiting","Body":"Kerbin","CrewNames":[],"Apoapsis":140000.0,"Periapsis":130000.0,"Inclination":5.0}""";

    [Fact]
    public async Task GetVesselsAsync_ExcludesDebrisByDefault()
    {
        var json = $"[{ProbeVessel},{DebrisVessel}]";
        var mock = new Mock<IKrpcConnection>();
        mock.Setup(c => c.GetVessels(false)).Returns(json);

        var tool = new VesselsTools(mock.Object);
        var result = await tool.GetVesselsAsync(include_debris: false);

        Assert.Equal(2, result.Count);
        Assert.Contains(result, v => v.Name == "Sentinel Probe");
        Assert.Contains(result, v => v.Name == "Fairing");

        mock.Verify(c => c.GetVessels(false), Times.Once);
    }

    [Fact]
    public async Task GetVesselsAsync_IncludesDebrisWhenRequested()
    {
        var json = $"[{ProbeVessel},{ShipVessel},{DebrisVessel}]";
        var mock = new Mock<IKrpcConnection>();
        mock.Setup(c => c.GetVessels(true)).Returns(json);

        var tool = new VesselsTools(mock.Object);
        var result = await tool.GetVesselsAsync(include_debris: true);

        Assert.Equal(3, result.Count);
        mock.Verify(c => c.GetVessels(true), Times.Once);
    }

    [Fact]
    public async Task GetVesselsAsync_MapsFieldsCorrectly()
    {
        var mock = new Mock<IKrpcConnection>();
        mock.Setup(c => c.GetVessels(false)).Returns($"[{ShipVessel}]");

        var tool = new VesselsTools(mock.Object);
        var result = await tool.GetVesselsAsync();

        var v = Assert.Single(result);
        Assert.Equal("Explorer I", v.Name);
        Assert.Equal("Ship", v.Type);
        Assert.Equal("Orbiting", v.Situation);
        Assert.Equal("Mun", v.Body);
        Assert.Equal(2, v.CrewNames.Count);
        Assert.Contains("Jebediah Kerman", v.CrewNames);
        Assert.Equal(20000.0, v.Apoapsis);
        Assert.Equal(0.0, v.Inclination);
    }

    [Fact]
    public async Task GetVesselsAsync_EmptyList_ReturnsEmptyCollection()
    {
        var mock = new Mock<IKrpcConnection>();
        mock.Setup(c => c.GetVessels(false)).Returns("[]");

        var tool = new VesselsTools(mock.Object);
        var result = await tool.GetVesselsAsync();

        Assert.Empty(result);
    }
}
