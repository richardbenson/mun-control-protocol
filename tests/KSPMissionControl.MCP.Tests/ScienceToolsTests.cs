using KSPMissionControl.MCP.Krpc;
using KSPMissionControl.MCP.Tools;
using Moq;
using Xunit;

namespace KSPMissionControl.MCP.Tests;

public sealed class ScienceToolsTests
{
    // Two Kerbin FlyingHigh subjects and one Mun InSpaceLow subject
    private const string KerbinFlyingHighSubject1 = """{"id":"crewReport@KerbinFlyingHighlands","experimentId":"crewReport","body":"Kerbin","situation":"FlyingHigh","biome":"lands","title":"Crew Report whilst flying high over Kerbin's Highlands","earned":5.0,"cap":5.0,"remaining":0.0,"subjectValue":1.0,"scienceMultiplier":0.7}""";
    private const string KerbinFlyingHighSubject2 = """{"id":"temperatureScan@KerbinFlyingHighHighPlains","experimentId":"temperatureScan","body":"Kerbin","situation":"FlyingHigh","biome":"HighPlains","title":"Temperature Scan whilst flying high over Kerbin's High Plains","earned":2.5,"cap":5.0,"remaining":2.5,"subjectValue":0.5,"scienceMultiplier":0.7}""";
    private const string MunSubject = """{"id":"crewReport@MunInSpaceLow","experimentId":"crewReport","body":"Mun","situation":"InSpaceLow","biome":"","title":"Crew Report from low orbit of the Mun","earned":0.0,"cap":4.0,"remaining":4.0,"subjectValue":1.0,"scienceMultiplier":3.0}""";

    private const string AllSubjectsJson = $"[{KerbinFlyingHighSubject1},{KerbinFlyingHighSubject2},{MunSubject}]";

    private const string SummaryJson = """[{"body":"Kerbin","subjectsTotal":10,"subjectsCompleted":7,"scienceRemaining":12.5},{"body":"Mun","subjectsTotal":8,"subjectsCompleted":2,"scienceRemaining":48.0}]""";

    [Fact]
    public async Task GetScienceStatusAsync_BodyAndSituation_ReturnsFilteredSubjects()
    {
        var mock = new Mock<IKrpcConnection>();
        mock.Setup(c => c.GetScienceSubjects("Kerbin", "FlyingHigh")).Returns($"[{KerbinFlyingHighSubject1},{KerbinFlyingHighSubject2}]");

        var tool = new ScienceTools(mock.Object);
        var result = await tool.GetScienceStatusAsync(body: "Kerbin", situation: "FlyingHigh");

        Assert.Null(result.Warning);
        Assert.Null(result.Summary);
        Assert.NotNull(result.Subjects);
        Assert.Equal(2, result.Subjects!.Count);
        Assert.Equal("crewReport@KerbinFlyingHighlands", result.Subjects[0].Id);
        Assert.Equal("Kerbin", result.Subjects[0].Body);
        Assert.Equal("FlyingHigh", result.Subjects[0].Situation);
        Assert.Equal(5.0, result.Subjects[0].Cap);
        Assert.Equal(0.0, result.Subjects[0].Remaining);

        mock.Verify(c => c.GetScienceSubjects("Kerbin", "FlyingHigh"), Times.Once);
        mock.Verify(c => c.GetSciencePerBodySummary(), Times.Never);
    }

    [Fact]
    public async Task GetScienceStatusAsync_BodyOnly_ReturnsAllBodySubjects()
    {
        var mock = new Mock<IKrpcConnection>();
        mock.Setup(c => c.GetScienceSubjects("Kerbin", "")).Returns($"[{KerbinFlyingHighSubject1},{KerbinFlyingHighSubject2}]");

        var tool = new ScienceTools(mock.Object);
        var result = await tool.GetScienceStatusAsync(body: "Kerbin");

        Assert.Null(result.Warning);
        Assert.Null(result.Summary);
        Assert.NotNull(result.Subjects);
        Assert.Equal(2, result.Subjects!.Count);

        mock.Verify(c => c.GetScienceSubjects("Kerbin", ""), Times.Once);
        mock.Verify(c => c.GetSciencePerBodySummary(), Times.Never);
    }

    [Fact]
    public async Task GetScienceStatusAsync_NoBody_ReturnsSummaryWithWarning()
    {
        var mock = new Mock<IKrpcConnection>();
        mock.Setup(c => c.GetSciencePerBodySummary()).Returns(SummaryJson);

        var tool = new ScienceTools(mock.Object);
        var result = await tool.GetScienceStatusAsync();

        Assert.NotNull(result.Warning);
        Assert.Contains("No body filter", result.Warning);
        Assert.NotNull(result.Summary);
        Assert.Equal(2, result.Summary!.Count);
        Assert.Equal("Kerbin", result.Summary[0].Body);
        Assert.Equal(10, result.Summary[0].SubjectsTotal);
        Assert.Equal(7, result.Summary[0].SubjectsCompleted);
        Assert.Equal(12.5, result.Summary[0].ScienceRemaining);
        Assert.Null(result.Subjects);

        mock.Verify(c => c.GetSciencePerBodySummary(), Times.Once);
        mock.Verify(c => c.GetScienceSubjects(It.IsAny<string>(), It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public async Task GetScienceStatusAsync_EmptyBodyString_ReturnsSummaryWithWarning()
    {
        var mock = new Mock<IKrpcConnection>();
        mock.Setup(c => c.GetSciencePerBodySummary()).Returns(SummaryJson);

        var tool = new ScienceTools(mock.Object);
        var result = await tool.GetScienceStatusAsync(body: "");

        Assert.NotNull(result.Warning);
        Assert.Null(result.Subjects);
        Assert.NotNull(result.Summary);

        mock.Verify(c => c.GetSciencePerBodySummary(), Times.Once);
        mock.Verify(c => c.GetScienceSubjects(It.IsAny<string>(), It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public async Task GetScienceStatusAsync_SubjectsHaveCorrectRemainingCalculation()
    {
        var mock = new Mock<IKrpcConnection>();
        mock.Setup(c => c.GetScienceSubjects("Kerbin", "")).Returns($"[{KerbinFlyingHighSubject2}]");

        var tool = new ScienceTools(mock.Object);
        var result = await tool.GetScienceStatusAsync(body: "Kerbin");

        Assert.NotNull(result.Subjects);
        var subj = result.Subjects![0];
        Assert.Equal(2.5, subj.Earned);
        Assert.Equal(5.0, subj.Cap);
        Assert.Equal(2.5, subj.Remaining);
        Assert.Equal(0.5, subj.SubjectValue);
    }
}
