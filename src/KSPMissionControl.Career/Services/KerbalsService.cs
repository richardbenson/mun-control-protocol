using KSPMissionControl.Career.Internal;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace KSPMissionControl.Career.Services;

public static class KerbalsService
{
    internal static readonly StateCache<string> Cache = new();

    internal static string GetKerbals() => Cache.Snapshot ?? "[]";

    internal static void RefreshCache()
    {
        if (HighLogic.CurrentGame == null) return;
        try
        {
            Cache.Update(BuildJson());
        }
        catch (Exception ex)
        {
            Debug.LogError("[KSPMissionControl] Failed to refresh kerbals cache: " + ex);
        }
    }

    private static string BuildJson()
    {
        var roster = HighLogic.CurrentGame?.CrewRoster;
        if (roster == null) return "[]";

        // Build a kerbal-name → vessel-name map from protoVessels so we don't have to search per kerbal.
        var kerbalToVessel = new Dictionary<string, string>(StringComparer.Ordinal);
        var protoVessels = HighLogic.CurrentGame?.flightState?.protoVessels;
        if (protoVessels != null)
        {
            foreach (ProtoVessel pv in protoVessels)
            {
                if (pv == null) continue;
                var crew = pv.GetVesselCrew();
                if (crew == null) continue;
                foreach (ProtoCrewMember pcm in crew)
                {
                    if (pcm?.name != null)
                        kerbalToVessel[pcm.name] = pv.vesselName ?? "";
                }
            }
        }

        var sb = new StringBuilder("[");
        bool first = true;

        foreach (ProtoCrewMember pcm in roster.Crew)
        {
            if (pcm == null) continue;
            AppendKerbal(sb, ref first, pcm, kerbalToVessel);
        }

        foreach (ProtoCrewMember pcm in roster.Tourist)
        {
            if (pcm == null) continue;
            AppendKerbal(sb, ref first, pcm, kerbalToVessel);
        }

        sb.Append(']');
        return sb.ToString();
    }

    private static void AppendKerbal(
        StringBuilder sb, ref bool first,
        ProtoCrewMember pcm, Dictionary<string, string> kerbalToVessel)
    {
        string location = pcm.rosterStatus switch
        {
            ProtoCrewMember.RosterStatus.Available => "Available",
            ProtoCrewMember.RosterStatus.Assigned  => "Assigned",
            ProtoCrewMember.RosterStatus.Dead      => "KIA",
            ProtoCrewMember.RosterStatus.Missing   => "Missing",
            _                                      => "Unknown"
        };

        bool hasVessel = kerbalToVessel.TryGetValue(pcm.name, out var vesselName);

        if (!first) sb.Append(',');
        first = false;

        sb.Append("{\"name\":").Append(JsonString(pcm.name ?? ""))
          .Append(",\"experienceLevel\":").Append(pcm.experienceLevel)
          .Append(",\"specialty\":").Append(JsonString(pcm.trait ?? ""))
          .Append(",\"location\":").Append(JsonString(location));

        if (hasVessel && vesselName != null)
            sb.Append(",\"assignedVessel\":").Append(JsonString(vesselName));
        else
            sb.Append(",\"assignedVessel\":null");

        sb.Append('}');
    }

    private static string JsonString(string s) =>
        "\"" + s.Replace("\\", "\\\\").Replace("\"", "\\\"")
               .Replace("\n", "\\n").Replace("\r", "\\r").Replace("\t", "\\t") + "\"";
}
