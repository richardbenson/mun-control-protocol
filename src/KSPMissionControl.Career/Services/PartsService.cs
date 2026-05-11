using KSPMissionControl.Career.Internal;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace KSPMissionControl.Career.Services;

// Caching helper for unlocked parts. kRPC procedures that expose this data live in
// TechTreeService (the single [KRPCService(Name="KSPMissionControl")] class).
public static class PartsService
{
    internal static readonly StateCache<string> Cache = new();

    internal static string GetPartsByCategory(string category)
    {
        var json = Cache.Snapshot ?? "[]";
        if (string.IsNullOrEmpty(category)) return "[]";
        return FilterJson(json, p => string.Equals(p.Category, category, StringComparison.OrdinalIgnoreCase));
    }

    internal static string GetPartByName(string name)
    {
        var json = Cache.Snapshot ?? "[]";
        if (string.IsNullOrEmpty(name)) return "null";
        return FindJson(json, p => string.Equals(p.Name, name, StringComparison.OrdinalIgnoreCase));
    }

    internal static void RefreshCache()
    {
        if (ResearchAndDevelopment.Instance == null) return;
        try
        {
            Cache.Update(BuildJson());
        }
        catch (Exception ex)
        {
            Debug.LogError("[KSPMissionControl] Failed to refresh parts cache: " + ex);
        }
    }

    // Parsed representation used only for cache-side filtering in GetPartsByCategory / GetPartByName.
    private sealed class PartEntry
    {
        public string Name = "";
        public string Category = "";
        public string Raw = "";
    }

    // Minimal JSON parse: extract name and category from each object so we can filter without
    // a full JSON library. The JSON is produced by BuildJson below so the format is known exactly.
    private static List<PartEntry> ParseEntries(string json)
    {
        var entries = new List<PartEntry>();
        // Each object starts with {"name":
        int i = 0;
        while ((i = json.IndexOf("{\"name\":", i, StringComparison.Ordinal)) >= 0)
        {
            int end = FindObjectEnd(json, i);
            if (end < 0) break;
            var raw = json.Substring(i, end - i + 1);
            entries.Add(new PartEntry
            {
                Name = ExtractString(raw, "\"name\":"),
                Category = ExtractString(raw, "\"category\":"),
                Raw = raw
            });
            i = end + 1;
        }
        return entries;
    }

    private static string FilterJson(string json, Func<PartEntry, bool> predicate)
    {
        var entries = ParseEntries(json);
        var sb = new StringBuilder("[");
        bool first = true;
        foreach (var e in entries)
        {
            if (!predicate(e)) continue;
            if (!first) sb.Append(',');
            first = false;
            sb.Append(e.Raw);
        }
        sb.Append(']');
        return sb.ToString();
    }

    private static string FindJson(string json, Func<PartEntry, bool> predicate)
    {
        foreach (var e in ParseEntries(json))
            if (predicate(e)) return e.Raw;
        return "null";
    }

    // Find the closing '}' of the JSON object starting at 'start', accounting for nesting.
    private static int FindObjectEnd(string s, int start)
    {
        int depth = 0;
        bool inStr = false;
        for (int i = start; i < s.Length; i++)
        {
            char c = s[i];
            if (inStr)
            {
                if (c == '\\') { i++; continue; }
                if (c == '"') inStr = false;
                continue;
            }
            if (c == '"') { inStr = true; continue; }
            if (c == '{') depth++;
            else if (c == '}') { if (--depth == 0) return i; }
        }
        return -1;
    }

    // Extract the string value of a known key from a flat JSON object produced by BuildJson.
    private static string ExtractString(string obj, string keyToken)
    {
        int ki = obj.IndexOf(keyToken, StringComparison.Ordinal);
        if (ki < 0) return "";
        int start = obj.IndexOf('"', ki + keyToken.Length);
        if (start < 0) return "";
        start++;
        var sb = new StringBuilder();
        for (int i = start; i < obj.Length; i++)
        {
            char c = obj[i];
            if (c == '\\' && i + 1 < obj.Length)
            {
                char next = obj[i + 1];
                if (next == '"') { sb.Append('"'); i++; }
                else if (next == '\\') { sb.Append('\\'); i++; }
                else if (next == 'n') { sb.Append('\n'); i++; }
                else if (next == 'r') { sb.Append('\r'); i++; }
                else if (next == 't') { sb.Append('\t'); i++; }
                else sb.Append(c);
            }
            else if (c == '"') break;
            else sb.Append(c);
        }
        return sb.ToString();
    }

    private static string BuildJson()
    {
        var sb = new StringBuilder("[");
        bool first = true;
        foreach (var part in PartLoader.LoadedPartsList)
        {
            // Parts with null/empty TechRequired are not in any tech node; treat as locked.
            if (string.IsNullOrEmpty(part.TechRequired)) continue;

            // "start" is always available; otherwise check unlock state.
            if (!string.Equals(part.TechRequired, "start", StringComparison.OrdinalIgnoreCase) &&
                ResearchAndDevelopment.GetTechnologyState(part.TechRequired) != RDTech.State.Available)
                continue;

            double massDry = part.partPrefab != null ? part.partPrefab.mass : 0.0;
            double massWet = massDry;
            if (part.partPrefab != null)
            {
                foreach (PartResource res in part.partPrefab.Resources)
                    massWet += res.amount * res.info.density;
            }

            if (!first) sb.Append(',');
            first = false;

            bool isPurchased = ResearchAndDevelopment.PartModelPurchased(part);

            sb.Append("{\"name\":").Append(JsonString(part.name))
              .Append(",\"title\":").Append(JsonString(part.title))
              .Append(",\"category\":").Append(JsonString(part.category.ToString()))
              .Append(",\"massDry\":").Append(massDry.ToString("R"))
              .Append(",\"massWet\":").Append(massWet.ToString("R"))
              .Append(",\"cost\":").Append(part.cost.ToString("R"))
              .Append(",\"techRequired\":").Append(JsonString(part.TechRequired))
              .Append(",\"isPurchased\":").Append(isPurchased ? "true" : "false")
              .Append('}');
        }
        sb.Append(']');
        return sb.ToString();
    }

    private static string JsonString(string s) =>
        "\"" + s.Replace("\\", "\\\\").Replace("\"", "\\\"")
               .Replace("\n", "\\n").Replace("\r", "\\r").Replace("\t", "\\t") + "\"";
}
