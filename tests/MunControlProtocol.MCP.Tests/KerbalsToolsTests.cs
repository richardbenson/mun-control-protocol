using MunControlProtocol.MCP.Krpc;
using MunControlProtocol.MCP.Tools;
using Moq;
using Xunit;

namespace MunControlProtocol.MCP.Tests;

public sealed class KerbalsToolsTests
{
    private const string Jeb     = """{"Name":"Jebediah Kerman","ExperienceLevel":5,"Specialty":"Pilot","AssignedVessel":"Explorer I","Location":"Assigned"}""";
    private const string Bill    = """{"Name":"Bill Kerman","ExperienceLevel":3,"Specialty":"Engineer","AssignedVessel":null,"Location":"Available"}""";
    private const string Valentina = """{"Name":"Valentina Kerman","ExperienceLevel":4,"Specialty":"Pilot","AssignedVessel":null,"Location":"KIA"}""";

    [Fact]
    public async Task GetKerbalsAsync_ReturnsParsedRoster()
    {
        var json = $"[{Jeb},{Bill},{Valentina}]";
        var mock = new Mock<IKrpcConnection>();
        mock.Setup(c => c.GetKerbals()).Returns(json);

        var tool = new KerbalsTools(mock.Object);
        var result = await tool.GetKerbalsAsync();

        Assert.Equal(3, result.Count);
        mock.Verify(c => c.GetKerbals(), Times.Once);
    }

    [Fact]
    public async Task GetKerbalsAsync_MapsAssignedKerbalCorrectly()
    {
        var mock = new Mock<IKrpcConnection>();
        mock.Setup(c => c.GetKerbals()).Returns($"[{Jeb}]");

        var tool = new KerbalsTools(mock.Object);
        var result = await tool.GetKerbalsAsync();

        var k = Assert.Single(result);
        Assert.Equal("Jebediah Kerman", k.Name);
        Assert.Equal(5, k.ExperienceLevel);
        Assert.Equal("Pilot", k.Specialty);
        Assert.Equal("Explorer I", k.AssignedVessel);
        Assert.Equal("Assigned", k.Location);
    }

    [Fact]
    public async Task GetKerbalsAsync_MapsAvailableKerbalCorrectly()
    {
        var mock = new Mock<IKrpcConnection>();
        mock.Setup(c => c.GetKerbals()).Returns($"[{Bill}]");

        var tool = new KerbalsTools(mock.Object);
        var result = await tool.GetKerbalsAsync();

        var k = Assert.Single(result);
        Assert.Equal("Bill Kerman", k.Name);
        Assert.Equal(3, k.ExperienceLevel);
        Assert.Equal("Engineer", k.Specialty);
        Assert.Null(k.AssignedVessel);
        Assert.Equal("Available", k.Location);
    }

    [Fact]
    public async Task GetKerbalsAsync_EmptyRoster_ReturnsEmptyCollection()
    {
        var mock = new Mock<IKrpcConnection>();
        mock.Setup(c => c.GetKerbals()).Returns("[]");

        var tool = new KerbalsTools(mock.Object);
        var result = await tool.GetKerbalsAsync();

        Assert.Empty(result);
    }
}
