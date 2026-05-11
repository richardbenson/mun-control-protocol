using KSPMissionControl.MCP.Krpc;
using KSPMissionControl.MCP.Tools;
using Moq;
using Xunit;

namespace KSPMissionControl.MCP.Tests;

public sealed class CareerToolsTests
{
    [Fact]
    public async Task GetCareerStateAsync_MapsConnectionValuesToDto()
    {
        var mock = new Mock<IKrpcConnection>();
        mock.SetupGet(c => c.Funds).Returns(12_345.67);
        mock.SetupGet(c => c.Science).Returns(50.0f);
        mock.SetupGet(c => c.Reputation).Returns(99.5f);

        var tool = new CareerTools(mock.Object);
        var result = await tool.GetCareerStateAsync();

        Assert.Equal(12_345.67, result.Funds);
        Assert.Equal(50.0, result.Science);
        Assert.Equal(99.5, result.Reputation);
    }

    [Fact]
    public async Task GetCareerStateAsync_PropagatesKrpcConnectionException()
    {
        var mock = new Mock<IKrpcConnection>();
        mock.SetupGet(c => c.Funds)
            .Throws(new KrpcConnectionException("KSP not running", new Exception()));

        var tool = new CareerTools(mock.Object);
        await Assert.ThrowsAsync<KrpcConnectionException>(() => tool.GetCareerStateAsync());
    }
}
