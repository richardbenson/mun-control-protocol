using KSPMissionControl.MCP.Tools;
using Xunit;

namespace KSPMissionControl.MCP.Tests;

public sealed class FormulasToolsTests
{
    // KSP body constants (from game data / KSP wiki)
    private const double KerbinMass   = 5.2915158e22; // kg
    private const double KerbinRadius = 600_000;       // m
    private const double MunMass      = 9.7599066e20;  // kg
    private const double MunRadius    = 200_000;        // m

    private readonly FormulasTools _tools = new();

    // ── calculate_delta_v ────────────────────────────────────────────────────

    [Fact]
    public async Task DeltaV_StandardBurn_MatchesTsiolkovsky()
    {
        // Isp=320s, 10t→5t → ΔV = 320 × 9.80665 × ln(2) ≈ 2173 m/s
        var result = await _tools.CalculateDeltaVAsync(320, 10, 5);
        Assert.Equal(320 * 9.80665 * Math.Log(2), result.DeltaVMs, 3);
    }

    [Fact]
    public async Task DeltaV_NoFuelBurned_ReturnsZero()
    {
        var result = await _tools.CalculateDeltaVAsync(320, 5, 5);
        Assert.Equal(0.0, result.DeltaVMs, 6);
    }

    [Fact]
    public async Task DeltaV_NegativeIsp_Throws()
    {
        await Assert.ThrowsAsync<ArgumentException>(() =>
            _tools.CalculateDeltaVAsync(-1, 10, 5));
    }

    [Fact]
    public async Task DeltaV_DryExceedsWet_Throws()
    {
        await Assert.ThrowsAsync<ArgumentException>(() =>
            _tools.CalculateDeltaVAsync(300, 5, 10));
    }

    // ── calculate_orbital_velocity ───────────────────────────────────────────

    [Fact]
    public async Task OrbitalVelocity_KerbinLKO_ReasonableValue()
    {
        // LKO 80 km — expect ~2240–2300 m/s
        var result = await _tools.CalculateOrbitalVelocityAsync(KerbinMass, KerbinRadius, 80_000);
        Assert.InRange(result.VelocityMs, 2240, 2310);
    }

    [Fact]
    public async Task OrbitalVelocity_ZeroAltitude_EqualsFormula()
    {
        const double G  = 6.674e-11;
        var expected = Math.Sqrt(G * MunMass / MunRadius);
        var result   = await _tools.CalculateOrbitalVelocityAsync(MunMass, MunRadius, 0);
        Assert.Equal(expected, result.VelocityMs, 3);
    }

    [Fact]
    public async Task OrbitalVelocity_NegativeAltitude_Throws()
    {
        await Assert.ThrowsAsync<ArgumentException>(() =>
            _tools.CalculateOrbitalVelocityAsync(KerbinMass, KerbinRadius, -1));
    }

    // ── calculate_orbital_period ─────────────────────────────────────────────

    [Fact]
    public async Task OrbitalPeriod_KerbinLKO_ReasonableValue()
    {
        // LKO ~80 km — actual ≈ 1875 s
        var result = await _tools.CalculateOrbitalPeriodAsync(KerbinMass, KerbinRadius, 80_000);
        Assert.InRange(result.PeriodS, 1800, 2000);
    }

    [Fact]
    public async Task OrbitalPeriod_MatchesFormula()
    {
        const double G = 6.674e-11;
        var r        = MunRadius + 10_000.0;
        var expected = 2 * Math.PI * Math.Sqrt(r * r * r / (G * MunMass));
        var result   = await _tools.CalculateOrbitalPeriodAsync(MunMass, MunRadius, 10_000);
        Assert.Equal(expected, result.PeriodS, 3);
    }

    // ── calculate_hohmann_transfer ───────────────────────────────────────────

    [Fact]
    public async Task Hohmann_SameOrbit_ZeroDeltaV()
    {
        var result = await _tools.CalculateHohmannTransferAsync(KerbinMass, KerbinRadius, 80_000, 80_000);
        Assert.Equal(0.0, result.Dv1Ms, 6);
        Assert.Equal(0.0, result.Dv2Ms, 6);
        Assert.Equal(0.0, result.TotalDvMs, 6);
    }

    [Fact]
    public async Task Hohmann_RaisingOrbit_BothBurnsPositive()
    {
        var result = await _tools.CalculateHohmannTransferAsync(KerbinMass, KerbinRadius, 80_000, 200_000);
        Assert.True(result.Dv1Ms > 0, "first burn should be prograde");
        Assert.True(result.Dv2Ms > 0, "second burn should be prograde");
        Assert.Equal(result.Dv1Ms + result.Dv2Ms, result.TotalDvMs, 6);
    }

    [Fact]
    public async Task Hohmann_LoweringOrbit_BothBurnsNegative()
    {
        var result = await _tools.CalculateHohmannTransferAsync(KerbinMass, KerbinRadius, 200_000, 80_000);
        Assert.True(result.Dv1Ms < 0, "first burn should be retrograde");
        Assert.True(result.Dv2Ms < 0, "second burn should be retrograde");
        Assert.Equal(Math.Abs(result.Dv1Ms) + Math.Abs(result.Dv2Ms), result.TotalDvMs, 6);
    }

    [Fact]
    public async Task Hohmann_TransferTime_PositiveAndReasonable()
    {
        var result = await _tools.CalculateHohmannTransferAsync(KerbinMass, KerbinRadius, 80_000, 200_000);
        Assert.True(result.TransferTimeS > 0);
        // Half-orbit at roughly ~740 km SMA — expect hundreds of seconds, not hours
        Assert.InRange(result.TransferTimeS, 100, 5000);
    }

    // ── calculate_escape_velocity ────────────────────────────────────────────

    [Fact]
    public async Task EscapeVelocity_MunSurface_AboutEightHundred()
    {
        // KSP wiki: Mun escape velocity ≈ 806 m/s
        var result = await _tools.CalculateEscapeVelocityAsync(MunMass, MunRadius, 0);
        Assert.InRange(result.EscapeVelocityMs, 800, 815);
    }

    [Fact]
    public async Task EscapeVelocity_DefaultsToSurface()
    {
        var fromSurface = await _tools.CalculateEscapeVelocityAsync(MunMass, MunRadius);
        var withZero    = await _tools.CalculateEscapeVelocityAsync(MunMass, MunRadius, 0);
        Assert.Equal(fromSurface.EscapeVelocityMs, withZero.EscapeVelocityMs);
    }

    [Fact]
    public async Task EscapeVelocity_HigherAltitude_IsLower()
    {
        var surface = await _tools.CalculateEscapeVelocityAsync(KerbinMass, KerbinRadius, 0);
        var high    = await _tools.CalculateEscapeVelocityAsync(KerbinMass, KerbinRadius, 100_000);
        Assert.True(high.EscapeVelocityMs < surface.EscapeVelocityMs);
    }

    // ── calculate_synchronous_orbit ──────────────────────────────────────────

    [Fact]
    public async Task SynchronousOrbit_Kerbin_AltitudeAboutTwoThousandKm()
    {
        // Kerbin sidereal day ≈ 21 549.4 s (KSP wiki); synchronous orbit ≈ 2 863 km altitude
        var result = await _tools.CalculateSynchronousOrbitAsync(KerbinMass, KerbinRadius, 21_549.4);
        Assert.NotNull(result.AltitudeM);
        Assert.InRange(result.AltitudeM!.Value, 2_800_000, 2_930_000);
    }

    [Fact]
    public async Task SynchronousOrbit_VelocityIsPositive()
    {
        var result = await _tools.CalculateSynchronousOrbitAsync(KerbinMass, KerbinRadius, 21_549.4);
        Assert.True(result.OrbitalVelocityMs > 0);
    }

    [Fact]
    public async Task SynchronousOrbit_BelowSurface_ReturnsNullAltitude()
    {
        // Extremely short rotation period → synchronous radius < body radius
        var result = await _tools.CalculateSynchronousOrbitAsync(MunMass, MunRadius, 1.0);
        Assert.Null(result.AltitudeM);
    }

    // ── calculate_commnet_range ──────────────────────────────────────────────

    [Fact]
    public async Task CommNetRange_SymmetricAntennas_EqualsInputPower()
    {
        // √(P × P) = P
        var result = await _tools.CalculateCommNetRangeAsync(1e9, 1e9);
        Assert.Equal(1e9, result.MaxRangeM, 1);
    }

    [Fact]
    public async Task CommNetRange_AsymmetricAntennas_GeometricMean()
    {
        var result = await _tools.CalculateCommNetRangeAsync(4e9, 1e9);
        Assert.Equal(Math.Sqrt(4e9 * 1e9), result.MaxRangeM, 1);
    }

    [Fact]
    public async Task CommNetRange_ZeroPower_Throws()
    {
        await Assert.ThrowsAsync<ArgumentException>(() =>
            _tools.CalculateCommNetRangeAsync(0, 1e9));
    }
}
