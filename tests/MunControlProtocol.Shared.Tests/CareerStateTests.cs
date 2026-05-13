using System.Text.Json;
using MunControlProtocol.Shared.Models;
using Xunit;

namespace MunControlProtocol.Shared.Tests;

public sealed class CareerStateTests
{
    [Fact]
    public void CareerState_RoundTrips_ThroughSystemTextJson()
    {
        var original = new CareerState { Funds = 1_234_567.89, Science = 42.5, Reputation = 100.0 };

        var json = JsonSerializer.Serialize(original);
        var deserialized = JsonSerializer.Deserialize<CareerState>(json)!;

        Assert.Equal(original.Funds, deserialized.Funds);
        Assert.Equal(original.Science, deserialized.Science);
        Assert.Equal(original.Reputation, deserialized.Reputation);
    }
}
