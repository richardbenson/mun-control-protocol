using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;
using MunControlProtocol.Shared.Models.PartModules;
using Xunit;

namespace MunControlProtocol.Shared.Tests;

public sealed class PartModuleTests
{
    private static readonly JsonSerializerOptions Options = new()
    {
        PropertyNameCaseInsensitive = true,
        Converters = { new JsonStringEnumConverter() },
    };

    [Fact]
    public void EngineInfo_RoundTrips()
    {
        var original = new EngineInfo
        {
            ThrustVacuum = 60.0,
            ThrustAsl = 43.5,
            IspVacuum = 345.0,
            IspAsl = 250.0,
            FuelFlowVacuum = 0.01773,
            Propellants = new List<string> { "LiquidFuel", "Oxidizer" },
        };
        var json = JsonSerializer.Serialize(original, Options);
        var result = JsonSerializer.Deserialize<EngineInfo>(json, Options)!;

        Assert.Equal(original.ThrustVacuum, result.ThrustVacuum);
        Assert.Equal(original.ThrustAsl, result.ThrustAsl);
        Assert.Equal(original.IspVacuum, result.IspVacuum);
        Assert.Equal(original.IspAsl, result.IspAsl);
        Assert.Equal(original.FuelFlowVacuum, result.FuelFlowVacuum);
        Assert.Equal(original.Propellants, result.Propellants);
    }

    [Fact]
    public void AntennaInfo_RoundTrips()
    {
        var original = new AntennaInfo
        {
            Range = 500000.0,
            Type = "Direct",
            Combinable = true,
            PacketSize = 2.0,
            PacketInterval = 1.0,
        };
        var json = JsonSerializer.Serialize(original, Options);
        var result = JsonSerializer.Deserialize<AntennaInfo>(json, Options)!;

        Assert.Equal(original.Range, result.Range);
        Assert.Equal(original.Type, result.Type);
        Assert.Equal(original.Combinable, result.Combinable);
        Assert.Equal(original.PacketSize, result.PacketSize);
        Assert.Equal(original.PacketInterval, result.PacketInterval);
    }

    [Fact]
    public void ResourceCapacity_RoundTrips()
    {
        var original = new ResourceCapacity { ResourceName = "ElectricCharge", MaxAmount = 100.0 };
        var json = JsonSerializer.Serialize(original, Options);
        var result = JsonSerializer.Deserialize<ResourceCapacity>(json, Options)!;

        Assert.Equal(original.ResourceName, result.ResourceName);
        Assert.Equal(original.MaxAmount, result.MaxAmount);
    }

    [Fact]
    public void TankInfo_RoundTrips()
    {
        var original = new TankInfo
        {
            Resources = new List<ResourceCapacity>
            {
                new() { ResourceName = "LiquidFuel", MaxAmount = 180.0 },
                new() { ResourceName = "Oxidizer", MaxAmount = 220.0 },
            },
        };
        var json = JsonSerializer.Serialize(original, Options);
        var result = JsonSerializer.Deserialize<TankInfo>(json, Options)!;

        Assert.Equal(2, result.Resources.Count);
        Assert.Equal("LiquidFuel", result.Resources[0].ResourceName);
        Assert.Equal(180.0, result.Resources[0].MaxAmount);
        Assert.Equal("Oxidizer", result.Resources[1].ResourceName);
        Assert.Equal(220.0, result.Resources[1].MaxAmount);
    }

    [Fact]
    public void CommandInfo_RoundTrips()
    {
        var original = new CommandInfo
        {
            CrewCapacity = 3,
            HasSas = true,
            SasLevel = 3,
            HibernationCharge = 0.0,
        };
        var json = JsonSerializer.Serialize(original, Options);
        var result = JsonSerializer.Deserialize<CommandInfo>(json, Options)!;

        Assert.Equal(original.CrewCapacity, result.CrewCapacity);
        Assert.Equal(original.HasSas, result.HasSas);
        Assert.Equal(original.SasLevel, result.SasLevel);
        Assert.Equal(original.HibernationCharge, result.HibernationCharge);
    }

    [Fact]
    public void SolarPanelInfo_RoundTrips()
    {
        var original = new SolarPanelInfo { ChargeRate = 1.64, Retractable = true };
        var json = JsonSerializer.Serialize(original, Options);
        var result = JsonSerializer.Deserialize<SolarPanelInfo>(json, Options)!;

        Assert.Equal(original.ChargeRate, result.ChargeRate);
        Assert.Equal(original.Retractable, result.Retractable);
    }
}
