using ModelContextProtocol.Server;

namespace KSPMissionControl.MCP.Tools;

[McpServerToolType]
internal sealed class FormulasTools
{
    private const double G  = 6.674e-11; // N·m²/kg²
    private const double G0 = 9.80665;   // standard gravity, m/s²

    /// <summary>
    /// Tsiolkovsky rocket equation: ΔV = Isp × 9.80665 × ln(m_wet / m_dry).
    /// Use vacuum ISP for vacuum burns, ASL ISP for atmospheric launches.
    /// Get engine ISP from get_part_stats; mass values from part wet/dry mass totals.
    /// </summary>
    [McpServerTool(Name = "calculate_delta_v")]
    public Task<DeltaVResult> CalculateDeltaVAsync(
        double isp_seconds,
        double mass_wet_tonnes,
        double mass_dry_tonnes)
    {
        if (isp_seconds <= 0)
            throw new ArgumentException("isp_seconds must be positive.");
        if (mass_wet_tonnes <= 0 || mass_dry_tonnes <= 0)
            throw new ArgumentException("Mass values must be positive.");
        if (mass_dry_tonnes > mass_wet_tonnes)
            throw new ArgumentException("mass_dry_tonnes cannot exceed mass_wet_tonnes.");

        var dv = isp_seconds * G0 * Math.Log(mass_wet_tonnes / mass_dry_tonnes);
        return Task.FromResult(new DeltaVResult(dv));
    }

    /// <summary>
    /// Circular orbital velocity at a given altitude: v = √(μ / r), where μ = G×M and r = body_radius_m + altitude_m.
    /// Get body_mass_kg and body_radius_m from get_body_info.
    /// </summary>
    [McpServerTool(Name = "calculate_orbital_velocity")]
    public Task<OrbitalVelocityResult> CalculateOrbitalVelocityAsync(
        double body_mass_kg,
        double body_radius_m,
        double altitude_m)
    {
        if (body_mass_kg <= 0) throw new ArgumentException("body_mass_kg must be positive.");
        if (body_radius_m <= 0) throw new ArgumentException("body_radius_m must be positive.");
        if (altitude_m < 0) throw new ArgumentException("altitude_m cannot be negative.");

        var mu = G * body_mass_kg;
        var r  = body_radius_m + altitude_m;
        var v  = Math.Sqrt(mu / r);
        return Task.FromResult(new OrbitalVelocityResult(v));
    }

    /// <summary>
    /// Orbital period of a circular orbit at a given altitude: T = 2π × √(r³ / μ).
    /// Get body_mass_kg and body_radius_m from get_body_info.
    /// </summary>
    [McpServerTool(Name = "calculate_orbital_period")]
    public Task<OrbitalPeriodResult> CalculateOrbitalPeriodAsync(
        double body_mass_kg,
        double body_radius_m,
        double altitude_m)
    {
        if (body_mass_kg <= 0) throw new ArgumentException("body_mass_kg must be positive.");
        if (body_radius_m <= 0) throw new ArgumentException("body_radius_m must be positive.");
        if (altitude_m < 0) throw new ArgumentException("altitude_m cannot be negative.");

        var mu = G * body_mass_kg;
        var r  = body_radius_m + altitude_m;
        var t  = 2.0 * Math.PI * Math.Sqrt(r * r * r / mu);
        return Task.FromResult(new OrbitalPeriodResult(t));
    }

    /// <summary>
    /// Hohmann transfer between two circular orbits around the same body (vis-viva equation).
    /// Returns the two burn delta-vs, their sum, and the coast time.
    /// A positive dv value means a prograde burn. Get body values from get_body_info.
    /// </summary>
    [McpServerTool(Name = "calculate_hohmann_transfer")]
    public Task<HohmannResult> CalculateHohmannTransferAsync(
        double body_mass_kg,
        double body_radius_m,
        double current_altitude_m,
        double target_altitude_m)
    {
        if (body_mass_kg <= 0) throw new ArgumentException("body_mass_kg must be positive.");
        if (body_radius_m <= 0) throw new ArgumentException("body_radius_m must be positive.");
        if (current_altitude_m < 0) throw new ArgumentException("current_altitude_m cannot be negative.");
        if (target_altitude_m < 0) throw new ArgumentException("target_altitude_m cannot be negative.");

        var mu = G * body_mass_kg;
        var r1 = body_radius_m + current_altitude_m;
        var r2 = body_radius_m + target_altitude_m;
        var a  = (r1 + r2) / 2.0; // semi-major axis of transfer ellipse

        var v1Circ     = Math.Sqrt(mu / r1);
        var v2Circ     = Math.Sqrt(mu / r2);
        var v1Transfer = Math.Sqrt(mu * (2.0 / r1 - 1.0 / a));
        var v2Transfer = Math.Sqrt(mu * (2.0 / r2 - 1.0 / a));

        var dv1          = v1Transfer - v1Circ;
        var dv2          = v2Circ - v2Transfer;
        var totalDv      = Math.Abs(dv1) + Math.Abs(dv2);
        var transferTime = Math.PI * Math.Sqrt(a * a * a / mu);

        return Task.FromResult(new HohmannResult(dv1, dv2, totalDv, transferTime));
    }

    /// <summary>
    /// Escape velocity at a given altitude: v_esc = √(2μ / r).
    /// altitude_m defaults to 0 (surface). Get body values from get_body_info.
    /// </summary>
    [McpServerTool(Name = "calculate_escape_velocity")]
    public Task<EscapeVelocityResult> CalculateEscapeVelocityAsync(
        double body_mass_kg,
        double body_radius_m,
        double altitude_m = 0)
    {
        if (body_mass_kg <= 0) throw new ArgumentException("body_mass_kg must be positive.");
        if (body_radius_m <= 0) throw new ArgumentException("body_radius_m must be positive.");
        if (altitude_m < 0) throw new ArgumentException("altitude_m cannot be negative.");

        var mu   = G * body_mass_kg;
        var r    = body_radius_m + altitude_m;
        var vEsc = Math.Sqrt(2.0 * mu / r);
        return Task.FromResult(new EscapeVelocityResult(vEsc));
    }

    /// <summary>
    /// Altitude and velocity of a synchronous (stationary) orbit: a = (μ × T² / 4π²)^(1/3).
    /// rotation_period_s is the body's sidereal day — get it from BodyInfo.RotationPeriodS via get_body_info.
    /// Returns null AltitudeM if the synchronous orbit radius is below the body's surface.
    /// </summary>
    [McpServerTool(Name = "calculate_synchronous_orbit")]
    public Task<SynchronousOrbitResult> CalculateSynchronousOrbitAsync(
        double body_mass_kg,
        double body_radius_m,
        double rotation_period_s)
    {
        if (body_mass_kg <= 0) throw new ArgumentException("body_mass_kg must be positive.");
        if (body_radius_m <= 0) throw new ArgumentException("body_radius_m must be positive.");
        if (rotation_period_s <= 0) throw new ArgumentException("rotation_period_s must be positive.");

        var mu          = G * body_mass_kg;
        var a           = Math.Pow(mu * rotation_period_s * rotation_period_s / (4.0 * Math.PI * Math.PI), 1.0 / 3.0);
        var altitudeM   = a - body_radius_m;
        var velocityMs  = Math.Sqrt(mu / a);

        // A negative altitude means the synchronous orbit is below the surface (e.g. fast-rotating body).
        double? reportedAlt = altitudeM >= 0 ? altitudeM : null;
        return Task.FromResult(new SynchronousOrbitResult(reportedAlt, velocityMs));
    }

    /// <summary>
    /// Maximum CommNet link range between two antennas: range = √(power_1 × power_2).
    /// This is KSP's geometric-mean formula; antenna power values come from get_part_stats (AntennaInfo.Range).
    /// </summary>
    [McpServerTool(Name = "calculate_commnet_range")]
    public Task<CommNetRangeResult> CalculateCommNetRangeAsync(
        double antenna_power_1,
        double antenna_power_2)
    {
        if (antenna_power_1 <= 0) throw new ArgumentException("antenna_power_1 must be positive.");
        if (antenna_power_2 <= 0) throw new ArgumentException("antenna_power_2 must be positive.");

        var range = Math.Sqrt(antenna_power_1 * antenna_power_2);
        return Task.FromResult(new CommNetRangeResult(range));
    }
}

public sealed record DeltaVResult(double DeltaVMs);
public sealed record OrbitalVelocityResult(double VelocityMs);
public sealed record OrbitalPeriodResult(double PeriodS);
public sealed record HohmannResult(double Dv1Ms, double Dv2Ms, double TotalDvMs, double TransferTimeS);
public sealed record EscapeVelocityResult(double EscapeVelocityMs);
public sealed record SynchronousOrbitResult(double? AltitudeM, double OrbitalVelocityMs);
public sealed record CommNetRangeResult(double MaxRangeM);
