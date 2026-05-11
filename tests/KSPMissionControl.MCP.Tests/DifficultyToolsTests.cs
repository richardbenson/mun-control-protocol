using KSPMissionControl.MCP.Krpc;
using KSPMissionControl.MCP.Tools;
using Moq;
using Xunit;

namespace KSPMissionControl.MCP.Tests;

public sealed class DifficultyToolsTests
{
    private const string SampleJson = """{"commNetRangeModifier":1.0,"dsnModifier":1.0,"requireSignalForControl":true,"requireSignalForScience":false,"enableCommNet":true,"occludeBodies":true,"rangeModifier":1.0,"scienceRewardsMultiplier":1.0,"fundsRewardsMultiplier":1.0,"reputationRewardsMultiplier":1.0,"fundsPenaltiesMultiplier":1.0,"reputationPenaltiesMultiplier":1.0,"reentryHeatingMultiplier":1.0,"crashToleranceMultiplier":1.0,"plasmaBlackout":false,"kerbalGToleranceMultiplier":1.0,"missingCrewsRespawn":true}""";

    [Fact]
    public async Task GetDifficultySettingsAsync_ReturnsParsedSettings()
    {
        var mock = new Mock<IKrpcConnection>();
        mock.Setup(c => c.GetDifficultySettings()).Returns(SampleJson);

        var tool = new CareerTools(mock.Object);
        var result = await tool.GetDifficultySettingsAsync();

        Assert.Equal(1.0, result.CommNetRangeModifier);
        Assert.Equal(1.0, result.DsnModifier);
        Assert.True(result.RequireSignalForControl);
        Assert.False(result.RequireSignalForScience);
        Assert.True(result.EnableCommNet);
        Assert.True(result.OccludeBodies);
        Assert.Equal(1.0, result.ScienceRewardsMultiplier);
        Assert.Equal(1.0, result.ReentryHeatingMultiplier);
        Assert.False(result.PlasmaBlackout);
        Assert.True(result.MissingCrewsRespawn);

        mock.Verify(c => c.GetDifficultySettings(), Times.Once);
    }

    [Fact]
    public async Task GetDifficultySettingsAsync_EmptyJson_ReturnsDefaults()
    {
        var mock = new Mock<IKrpcConnection>();
        mock.Setup(c => c.GetDifficultySettings()).Returns("{}");

        var tool = new CareerTools(mock.Object);
        var result = await tool.GetDifficultySettingsAsync();

        Assert.Equal(0.0, result.CommNetRangeModifier);
        Assert.False(result.RequireSignalForControl);
        Assert.False(result.EnableCommNet);
    }
}
