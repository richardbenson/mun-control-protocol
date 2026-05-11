using KRPC.Service;
using KRPC.Service.Attributes;
using KSPMissionControl.Career.Internal;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace KSPMissionControl.Career.Services;

/// <summary>Returns all tech tree nodes with their unlock status and contained parts.</summary>
[KRPCService(Name = "KSPMissionControl", GameScene = GameScene.All)]
public static class TechTreeService
{
    internal static readonly StateCache<string> Cache = new();

    /// <summary>Returns all tech tree nodes as a JSON array. Each element has id, title, scienceCost, status, and partNames.</summary>
    [KRPCProcedure]
    public static string GetTechTree() => Cache.Snapshot ?? "[]";

    internal static void RefreshCache()
    {
        var rd = ResearchAndDevelopment.Instance;
        if (rd == null) return;
        try
        {
            Cache.Update(BuildJson(rd));
        }
        catch (Exception ex)
        {
            Debug.LogError("[KSPMissionControl] Failed to refresh tech tree cache: " + ex);
        }
    }

    private static string BuildJson(ResearchAndDevelopment rd)
    {
        // Group loaded parts by the tech node that unlocks them.
        var partsByTech = new Dictionary<string, List<string>>(StringComparer.Ordinal);
        foreach (var part in PartLoader.LoadedPartsList)
        {
            var techId = part.TechRequired;
            if (string.IsNullOrEmpty(techId)) continue;
            if (!partsByTech.TryGetValue(techId, out var list))
                partsByTech[techId] = list = new List<string>();
            list.Add(part.name);
        }

        // Tech node definitions (id, cost) come from the game database config.
        var configs = GameDatabase.Instance.GetConfigNodes("TechTree");
        if (configs == null || configs.Length == 0) return "[]";

        var sb = new StringBuilder("[");
        bool first = true;
        foreach (ConfigNode rdNode in configs[0].GetNodes("RDNode"))
        {
            var techId = rdNode.GetValue("id");
            if (string.IsNullOrEmpty(techId)) continue;

            // Prefer the static KSP helper for title; fall back to config value.
            var title = ResearchAndDevelopment.GetTechnologyTitle(techId);
            if (string.IsNullOrEmpty(title))
                title = rdNode.GetValue("title") ?? techId;

            int.TryParse(rdNode.GetValue("cost"), out int scienceCost);

            // Determine status. GetTechState() returns a non-null ProtoTechNode only when unlocked.
            string status;
            var proto = rd.GetTechState(techId);
            if (proto != null)
            {
                status = "Unlocked";
                scienceCost = proto.scienceCost;
            }
            else
            {
                var state = ResearchAndDevelopment.GetTechnologyState(techId);
                status = state == RDTech.State.Available ? "Available" : "Locked";
            }

            var parts = partsByTech.TryGetValue(techId, out var pList) ? pList : new List<string>();

            if (!first) sb.Append(',');
            first = false;

            sb.Append("{\"id\":").Append(JsonString(techId))
              .Append(",\"title\":").Append(JsonString(title))
              .Append(",\"scienceCost\":").Append(scienceCost)
              .Append(",\"status\":").Append(JsonString(status))
              .Append(",\"partNames\":[");

            for (int i = 0; i < parts.Count; i++)
            {
                if (i > 0) sb.Append(',');
                sb.Append(JsonString(parts[i]));
            }
            sb.Append("]}");
        }
        sb.Append(']');
        return sb.ToString();
    }

    private static string JsonString(string s) =>
        "\"" + s.Replace("\\", "\\\\").Replace("\"", "\\\"")
               .Replace("\n", "\\n").Replace("\r", "\\r").Replace("\t", "\\t") + "\"";
}
