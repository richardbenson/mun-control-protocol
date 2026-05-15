using MunControlProtocol.MCP.Krpc;
using MunControlProtocol.MCP.Tools;
using Moq;
using Xunit;

namespace MunControlProtocol.MCP.Tests;

public sealed class EditorToolsTests
{
    private const string EnginePartJson = """
        {
          "name": "liquidEngine",
          "title": "LV-T45 Swivel",
          "massT": 1.5,
          "resourceMassT": 0.0,
          "cost": 1200.0,
          "stageIndex": 1,
          "resources": [],
          "engine": {
            "thrustVacuum": 215.0,
            "thrustAsl": 168.0,
            "ispVacuum": 320.0,
            "ispAsl": 270.0,
            "fuelFlowVacuum": 0.068,
            "propellants": ["LiquidFuel", "Oxidizer"]
          },
          "antenna": null,
          "tank": null,
          "command": null,
          "solarPanel": null
        }
        """;

    private const string PodPartJson = """
        {
          "name": "mk1pod",
          "title": "Mk1 Command Pod",
          "massT": 0.84,
          "resourceMassT": 0.056,
          "cost": 600.0,
          "stageIndex": 0,
          "resources": [
            {"name": "ElectricCharge", "amount": 50.0, "maxAmount": 50.0},
            {"name": "Monopropellant", "amount": 10.0, "maxAmount": 10.0}
          ],
          "engine": null,
          "antenna": null,
          "tank": null,
          "command": {"crewCapacity": 1, "hasSas": true, "sasLevel": 3, "hibernationCharge": 0},
          "solarPanel": null
        }
        """;

    private const string CraftJson = $$"""
        {
          "name": "Mun Mission I",
          "editorType": "VAB",
          "partCount": 2,
          "totalMassT": 2.396,
          "totalCost": 1800.0,
          "crewCapacity": 1,
          "parts": [{{EnginePartJson}}, {{PodPartJson}}]
        }
        """;

    [Fact]
    public async Task GetCurrentCraftAsync_ReturnsNull_WhenJsonIsNull()
    {
        var mock = new Mock<IKrpcConnection>();
        mock.Setup(c => c.GetCurrentCraft()).Returns("null");

        var tool = new EditorTools(mock.Object);
        var result = await tool.GetCurrentCraftAsync();

        Assert.Null(result);
    }

    [Fact]
    public async Task GetCurrentCraftAsync_ReturnsNull_WhenJsonIsEmpty()
    {
        var mock = new Mock<IKrpcConnection>();
        mock.Setup(c => c.GetCurrentCraft()).Returns("");

        var tool = new EditorTools(mock.Object);
        var result = await tool.GetCurrentCraftAsync();

        Assert.Null(result);
    }

    [Fact]
    public async Task GetCurrentCraftAsync_MapsTopLevelFields()
    {
        var mock = new Mock<IKrpcConnection>();
        mock.Setup(c => c.GetCurrentCraft()).Returns(CraftJson);

        var tool = new EditorTools(mock.Object);
        var result = await tool.GetCurrentCraftAsync();

        Assert.NotNull(result);
        Assert.Equal("Mun Mission I", result!.Name);
        Assert.Equal("VAB", result.EditorType);
        Assert.Equal(2, result.PartCount);
        Assert.Equal(2.396, result.TotalMassT);
        Assert.Equal(1800.0, result.TotalCost);
        Assert.Equal(1, result.CrewCapacity);
    }

    [Fact]
    public async Task GetCurrentCraftAsync_MapsParts()
    {
        var mock = new Mock<IKrpcConnection>();
        mock.Setup(c => c.GetCurrentCraft()).Returns(CraftJson);

        var tool = new EditorTools(mock.Object);
        var result = await tool.GetCurrentCraftAsync();

        Assert.NotNull(result);
        Assert.Equal(2, result!.Parts.Count);
        Assert.Equal("liquidEngine", result.Parts[0].Name);
        Assert.Equal("mk1pod", result.Parts[1].Name);
    }

    [Fact]
    public async Task GetCurrentCraftAsync_MapsEngineModule()
    {
        var singlePartCraft = $$"""
            {
              "name": "Test Rocket",
              "editorType": "VAB",
              "partCount": 1,
              "totalMassT": 1.5,
              "totalCost": 1200.0,
              "crewCapacity": 0,
              "parts": [{{EnginePartJson}}]
            }
            """;

        var mock = new Mock<IKrpcConnection>();
        mock.Setup(c => c.GetCurrentCraft()).Returns(singlePartCraft);

        var tool = new EditorTools(mock.Object);
        var result = await tool.GetCurrentCraftAsync();

        Assert.NotNull(result);
        var engine = result!.Parts[0].Engine;
        Assert.NotNull(engine);
        Assert.Equal(215.0, engine!.ThrustVacuum);
        Assert.Equal(320.0, engine.IspVacuum);
    }

    [Fact]
    public async Task GetCurrentCraftAsync_MapsResources()
    {
        var singlePartCraft = $$"""
            {
              "name": "Test Pod",
              "editorType": "VAB",
              "partCount": 1,
              "totalMassT": 0.896,
              "totalCost": 600.0,
              "crewCapacity": 1,
              "parts": [{{PodPartJson}}]
            }
            """;

        var mock = new Mock<IKrpcConnection>();
        mock.Setup(c => c.GetCurrentCraft()).Returns(singlePartCraft);

        var tool = new EditorTools(mock.Object);
        var result = await tool.GetCurrentCraftAsync();

        Assert.NotNull(result);
        Assert.Equal(2, result!.Parts[0].Resources.Count);
        Assert.Equal("ElectricCharge", result.Parts[0].Resources[0].Name);
        Assert.Equal(50.0, result.Parts[0].Resources[0].Amount);
        Assert.Equal(50.0, result.Parts[0].Resources[0].MaxAmount);
    }

    [Fact]
    public async Task GetCurrentCraftAsync_NullModulesWhenAbsent()
    {
        var singlePartCraft = $$"""
            {
              "name": "Test Pod",
              "editorType": "VAB",
              "partCount": 1,
              "totalMassT": 0.896,
              "totalCost": 600.0,
              "crewCapacity": 1,
              "parts": [{{PodPartJson}}]
            }
            """;

        var mock = new Mock<IKrpcConnection>();
        mock.Setup(c => c.GetCurrentCraft()).Returns(singlePartCraft);

        var tool = new EditorTools(mock.Object);
        var result = await tool.GetCurrentCraftAsync();

        Assert.NotNull(result);
        Assert.Null(result!.Parts[0].Engine);
        Assert.Null(result.Parts[0].Tank);
    }
}
