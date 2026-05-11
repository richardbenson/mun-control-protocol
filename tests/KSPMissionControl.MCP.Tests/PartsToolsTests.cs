using KSPMissionControl.MCP.Krpc;
using KSPMissionControl.MCP.Tools;
using Moq;
using Xunit;

namespace KSPMissionControl.MCP.Tests;


public sealed class PartsToolsTests
{
    private const string EnginePart = """{"name":"liquidEngine","title":"LV-T30 Liquid Fuel Engine","category":"Engine","massDry":1.25,"massWet":1.25,"cost":850.0,"techRequired":"basicRocketry","isPurchased":true}""";
    private const string CommandPod = """{"name":"mk1Pod","title":"Mk1 Command Pod","category":"Pods","massDry":0.84,"massWet":0.94,"cost":600.0,"techRequired":"start","isPurchased":false}""";
    private const string EngineJson = $"[{EnginePart}]";
    private const string PodsJson = $"[{CommandPod}]";
    private const string EmptyJson = "[]";

    [Fact]
    public async Task GetPartsByCategoryAsync_ReturnsMappedParts()
    {
        var mock = new Mock<IKrpcConnection>();
        mock.Setup(c => c.GetPartsByCategory("Engine")).Returns(EngineJson);

        var tool = new PartsTools(mock.Object);
        var result = await tool.GetPartsByCategoryAsync("Engine");

        Assert.Single(result);
        Assert.Equal("liquidEngine", result[0].Name);
        Assert.Equal("LV-T30 Liquid Fuel Engine", result[0].Title);
        Assert.Equal("Engine", result[0].Category);
        Assert.Equal(1.25, result[0].MassDry);
        Assert.Equal(1.25, result[0].MassWet);
        Assert.Equal(850.0, result[0].Cost);
        Assert.Equal("basicRocketry", result[0].TechRequired);
    }

    [Fact]
    public async Task GetPartsByCategoryAsync_EmptyCategory_ThrowsArgumentException()
    {
        var mock = new Mock<IKrpcConnection>();
        var tool = new PartsTools(mock.Object);

        await Assert.ThrowsAsync<ArgumentException>(() => tool.GetPartsByCategoryAsync(""));
        mock.Verify(c => c.GetPartsByCategory(It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public async Task GetPartsByCategoryAsync_ReturnsEmptyList_WhenNoneInCategory()
    {
        var mock = new Mock<IKrpcConnection>();
        mock.Setup(c => c.GetPartsByCategory("Cargo")).Returns(EmptyJson);

        var tool = new PartsTools(mock.Object);
        var result = await tool.GetPartsByCategoryAsync("Cargo");

        Assert.Empty(result);
    }

    [Fact]
    public async Task GetPartStatsAsync_ByName_ReturnsSinglePart()
    {
        var mock = new Mock<IKrpcConnection>();
        mock.Setup(c => c.GetPartByName("mk1Pod")).Returns(CommandPod);

        var tool = new PartsTools(mock.Object);
        var result = await tool.GetPartStatsAsync(part_name: "mk1Pod");

        Assert.Single(result);
        Assert.Equal("mk1Pod", result[0].Name);
        Assert.Equal("Pods", result[0].Category);
        Assert.False(result[0].IsPurchased);
    }

    [Fact]
    public async Task GetPartStatsAsync_ByName_NotFound_ReturnsEmpty()
    {
        var mock = new Mock<IKrpcConnection>();
        mock.Setup(c => c.GetPartByName("unknownPart")).Returns("null");

        var tool = new PartsTools(mock.Object);
        var result = await tool.GetPartStatsAsync(part_name: "unknownPart");

        Assert.Empty(result);
    }

    [Fact]
    public async Task GetPartStatsAsync_ByCategory_ReturnsList()
    {
        var mock = new Mock<IKrpcConnection>();
        mock.Setup(c => c.GetPartsByCategory("Pods")).Returns(PodsJson);

        var tool = new PartsTools(mock.Object);
        var result = await tool.GetPartStatsAsync(category: "Pods");

        Assert.Single(result);
        Assert.Equal("mk1Pod", result[0].Name);
    }

    [Fact]
    public async Task GetPartStatsAsync_NameTakesPrecedenceOverCategory()
    {
        var mock = new Mock<IKrpcConnection>();
        mock.Setup(c => c.GetPartByName("mk1Pod")).Returns(CommandPod);

        var tool = new PartsTools(mock.Object);
        // Both provided — name wins
        var result = await tool.GetPartStatsAsync(part_name: "mk1Pod", category: "Pods");

        Assert.Single(result);
        mock.Verify(c => c.GetPartByName("mk1Pod"), Times.Once);
        mock.Verify(c => c.GetPartsByCategory(It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public async Task GetPartStatsAsync_NeitherProvided_ThrowsArgumentException()
    {
        var mock = new Mock<IKrpcConnection>();
        var tool = new PartsTools(mock.Object);

        await Assert.ThrowsAsync<ArgumentException>(() => tool.GetPartStatsAsync());
        mock.Verify(c => c.GetPartByName(It.IsAny<string>()), Times.Never);
        mock.Verify(c => c.GetPartsByCategory(It.IsAny<string>()), Times.Never);
    }

    // A command pod with integrated antenna, battery, and SAS — multiple sub-DTOs populated.
    private const string MultiModulePod = """
        {"name":"mk1-3pod","title":"Mk1-3 Command Pod","category":"Pods",
         "massDry":2.72,"massWet":2.94,"cost":3800.0,"techRequired":"advancedFlightControl","isPurchased":true,
         "antenna":{"range":5000.0,"type":"Internal","combinable":false,"packetSize":2.0,"packetInterval":1.0},
         "tank":{"resources":[{"resourceName":"ElectricCharge","maxAmount":150.0},{"resourceName":"MonoPropellant","maxAmount":30.0}]},
         "command":{"crewCapacity":3,"hasSas":true,"sasLevel":3,"hibernationCharge":0.0}}
        """;

    private const string StructuralPart = """
        {"name":"strutConnector","title":"EAS-4 Strut Connector","category":"Structural",
         "massDry":0.05,"massWet":0.05,"cost":15.0,"techRequired":"start","isPurchased":true}
        """;

    [Fact]
    public async Task GetPartStatsAsync_MultiModulePart_PopulatesAllSubDtos()
    {
        var mock = new Mock<IKrpcConnection>();
        mock.Setup(c => c.GetPartByName("mk1-3pod")).Returns(MultiModulePod);

        var tool = new PartsTools(mock.Object);
        var result = await tool.GetPartStatsAsync(part_name: "mk1-3pod");

        Assert.Single(result);
        var part = result[0];
        Assert.Equal("mk1-3pod", part.Name);

        Assert.NotNull(part.Antenna);
        Assert.Equal(5000.0, part.Antenna!.Range);
        Assert.Equal("Internal", part.Antenna.Type);

        Assert.NotNull(part.Tank);
        Assert.Equal(2, part.Tank!.Resources.Count);
        Assert.Equal("ElectricCharge", part.Tank.Resources[0].ResourceName);
        Assert.Equal(150.0, part.Tank.Resources[0].MaxAmount);

        Assert.NotNull(part.Command);
        Assert.Equal(3, part.Command!.CrewCapacity);
        Assert.True(part.Command.HasSas);
        Assert.Equal(3, part.Command.SasLevel);

        Assert.Null(part.Engine);
        Assert.Null(part.SolarPanel);
    }

    [Fact]
    public async Task GetPartStatsAsync_StructuralPart_AllSubDtosNull()
    {
        var mock = new Mock<IKrpcConnection>();
        mock.Setup(c => c.GetPartByName("strutConnector")).Returns(StructuralPart);

        var tool = new PartsTools(mock.Object);
        var result = await tool.GetPartStatsAsync(part_name: "strutConnector");

        Assert.Single(result);
        var part = result[0];
        Assert.Equal("strutConnector", part.Name);
        Assert.Null(part.Engine);
        Assert.Null(part.Antenna);
        Assert.Null(part.Tank);
        Assert.Null(part.Command);
        Assert.Null(part.SolarPanel);
    }
}
