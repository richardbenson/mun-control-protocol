using KRPC.Client;
using KRPC.Client.Services.KSPMissionControl;
using KRPC.Client.Services.SpaceCenter;
using System.Text.Json;
using SpaceCenterService = KRPC.Client.Services.SpaceCenter.Service;
using KspMcService = KRPC.Client.Services.KSPMissionControl.Service;
using SharedVessel = KSPMissionControl.Shared.Models.Vessel;
using SharedBody = KSPMissionControl.Shared.Models.BodyInfo;

namespace KSPMissionControl.MCP.Krpc;

internal sealed class KrpcConnection : IKrpcConnection
{
    private static readonly JsonSerializerOptions _serializeOptions = new();

    private Connection? _connection;
    private SpaceCenterService? _spaceCenter;
    private KspMcService? _kspMissionControl;
    private readonly object _lock = new();

    private Connection EnsureConnected()
    {
        if (_connection is not null) return _connection;
        lock (_lock)
        {
            if (_connection is not null) return _connection;
            try
            {
                _connection = new Connection("KSP Mission Control");
                return _connection;
            }
            catch (Exception ex)
            {
                throw new KrpcConnectionException(
                    "Failed to connect to kRPC server. " +
                    "Launch KSP, load a career save, and confirm the kRPC mod is running " +
                    "on the default port (50000) via the kRPC status window.", ex);
            }
        }
    }

    private SpaceCenterService SpaceCenter => _spaceCenter ??= EnsureConnected().SpaceCenter();

    private KspMcService KspMissionControl =>
        _kspMissionControl ??= EnsureConnected().KSPMissionControl();

    double IKrpcConnection.Funds      => SpaceCenter.Funds;
    float  IKrpcConnection.Science    => SpaceCenter.Science;
    float  IKrpcConnection.Reputation => SpaceCenter.Reputation;

    string IKrpcConnection.GetTechTree()                             => KspMissionControl.GetTechTree();
    string IKrpcConnection.GetPartsByCategory(string category)       => KspMissionControl.GetPartsByCategory(category);
    string IKrpcConnection.GetPartByName(string name)                => KspMissionControl.GetPartByName(name);
    string IKrpcConnection.GetScienceSubjects(string body, string s) => KspMissionControl.GetScienceSubjects(body, s);
    string IKrpcConnection.GetSciencePerBodySummary()                => KspMissionControl.GetSciencePerBodySummary();
    string IKrpcConnection.GetBuildingLevels()                       => KspMissionControl.GetBuildingLevels();
    string IKrpcConnection.GetDifficultySettings()                   => KspMissionControl.GetDifficultySettings();
    string IKrpcConnection.GetKerbals()                              => KspMissionControl.GetKerbals();

    string IKrpcConnection.GetVessels(bool includeDebris)
    {
        var vessels = SpaceCenter.Vessels;
        var dtos = new List<SharedVessel>(vessels.Count);
        foreach (var v in vessels)
        {
            if (!includeDebris && v.Type.ToString() == "Debris") continue;

            double apoapsis = 0, periapsis = 0, inclination = 0;
            string bodyName = "";
            try
            {
                var orbit = v.Orbit;
                apoapsis    = orbit.ApoapsisAltitude;
                periapsis   = orbit.PeriapsisAltitude;
                inclination = orbit.Inclination * (180.0 / Math.PI);
                bodyName    = orbit.Body.Name;
            }
            catch { /* vessel may have no valid orbit */ }

            var crewNames = new List<string>();
            try
            {
                foreach (var cm in v.Crew)
                    crewNames.Add(cm.Name);
            }
            catch { }

            dtos.Add(new SharedVessel
            {
                Name        = v.Name,
                Type        = v.Type.ToString(),
                Situation   = v.Situation.ToString(),
                Body        = bodyName,
                CrewNames   = crewNames,
                Apoapsis    = apoapsis,
                Periapsis   = periapsis,
                Inclination = inclination,
            });
        }
        return JsonSerializer.Serialize(dtos, _serializeOptions);
    }

    string IKrpcConnection.GetBodyInfo(string? body)
    {
        var bodies = SpaceCenter.Bodies;
        var dtos = new List<SharedBody>(bodies.Count);

        foreach (var kvp in bodies)
        {
            if (body != null && !string.Equals(kvp.Key, body, StringComparison.OrdinalIgnoreCase))
                continue;

            var cb = kvp.Value;
            string? parentName = null;
            double? orbPeriod  = null;
            double? sma        = null;

            try
            {
                var orbit  = cb.Orbit;
                parentName = orbit.Body.Name;
                orbPeriod  = orbit.Period;
                sma        = orbit.SemiMajorAxis;
            }
            catch { /* Sun / root body has no orbit */ }

            dtos.Add(new SharedBody
            {
                Name             = kvp.Key,
                Mass             = cb.Mass,
                Radius           = cb.EquatorialRadius,
                AtmosphereHeight = cb.AtmosphereDepth,
                SoiRadius        = cb.SphereOfInfluence,
                Parent           = parentName,
                OrbitalPeriod    = orbPeriod,
                SemiMajorAxis    = sma,
                RotationPeriodS  = cb.RotationalPeriod,
            });
        }
        return JsonSerializer.Serialize(dtos, _serializeOptions);
    }

    public void Dispose()
    {
        _connection?.Dispose();
    }
}
