using KSPMissionControl.Career.Internal;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using UnityEngine;
using KspScienceSubject = global::ScienceSubject;

namespace KSPMissionControl.Career.Services;

public static class ScienceService
{
    // Situations as they appear in subject IDs; ordered longest-first to avoid prefix ambiguity.
    private static readonly string[] Situations = new[]
    {
        "InSpaceHigh", "InSpaceLow", "FlyingHigh", "FlyingLow", "Splashed", "Landed"
    };

    internal static readonly StateCache<string> SubjectsCache = new();
    internal static readonly StateCache<string> SummaryCache = new();

    internal static string GetScienceSubjects(string body, string situation)
    {
        var json = SubjectsCache.Snapshot ?? "[]";
        if (string.IsNullOrEmpty(body) && string.IsNullOrEmpty(situation))
            return json;
        return FilterJson(json, body, situation);
    }

    internal static string GetSciencePerBodySummary() => SummaryCache.Snapshot ?? "[]";

    internal static void RefreshCache()
    {
        if (ResearchAndDevelopment.Instance == null) return;
        try
        {
            BuildCaches();
        }
        catch (Exception ex)
        {
            Debug.LogError("[KSPMissionControl] Failed to refresh science cache: " + ex);
        }
    }

    private static void BuildCaches()
    {
        var subjects = GetAllSubjects();
        if (subjects == null) return;

        var bodyNames = GetBodyNames();

        // Per-body summary accumulators
        var summaryMap = new Dictionary<string, (int total, int completed, double remaining)>(StringComparer.Ordinal);

        var sb = new StringBuilder("[");
        bool first = true;

        foreach (KspScienceSubject? subj in subjects)
        {
            if (subj == null) continue;
            double cap = subj.scienceCap;
            if (cap <= 0) continue; // no valid science available for this subject

            double earned = subj.science;
            double remaining = cap - earned;
            double subjectValue = subj.subjectValue;
            double sciMult = subj.scientificValue;
            string title = subj.title ?? "";
            string id = subj.id ?? "";

            ParseId(id, bodyNames, out string expId, out string bodyName, out string situation, out string biome);

            // Accumulate summary (per body, subjects with cap > 0 only)
            bool completed = remaining <= 0.001;
            if (!summaryMap.TryGetValue(bodyName, out var entry))
                entry = (0, 0, 0.0);
            summaryMap[bodyName] = (
                entry.total + 1,
                entry.completed + (completed ? 1 : 0),
                entry.remaining + (remaining > 0 ? remaining : 0)
            );

            if (!first) sb.Append(',');
            first = false;

            sb.Append("{\"id\":").Append(JsonString(id))
              .Append(",\"experimentId\":").Append(JsonString(expId))
              .Append(",\"body\":").Append(JsonString(bodyName))
              .Append(",\"situation\":").Append(JsonString(situation))
              .Append(",\"biome\":").Append(JsonString(biome))
              .Append(",\"title\":").Append(JsonString(title))
              .Append(",\"earned\":").Append(earned.ToString("R"))
              .Append(",\"cap\":").Append(cap.ToString("R"))
              .Append(",\"remaining\":").Append(remaining.ToString("R"))
              .Append(",\"subjectValue\":").Append(subjectValue.ToString("R"))
              .Append(",\"scienceMultiplier\":").Append(sciMult.ToString("R"))
              .Append('}');
        }
        sb.Append(']');
        SubjectsCache.Update(sb.ToString());

        var sumSb = new StringBuilder("[");
        bool sumFirst = true;
        foreach (var kvp in summaryMap)
        {
            if (!sumFirst) sumSb.Append(',');
            sumFirst = false;
            var (total, completed, remaining) = kvp.Value;
            sumSb.Append("{\"body\":").Append(JsonString(kvp.Key))
                 .Append(",\"subjectsTotal\":").Append(total)
                 .Append(",\"subjectsCompleted\":").Append(completed)
                 .Append(",\"scienceRemaining\":").Append(remaining.ToString("R"))
                 .Append('}');
        }
        sumSb.Append(']');
        SummaryCache.Update(sumSb.ToString());
    }

    // KSP does not expose a public static GetExperimentSubjects() method.
    // Scan all fields on the ResearchAndDevelopment instance for the subjects dictionary —
    // its field name varies across minor KSP patches, so we match by type instead of name.
    private static IEnumerable<KspScienceSubject>? GetAllSubjects()
    {
        var rd = ResearchAndDevelopment.Instance;
        if (rd == null) return null;

        foreach (FieldInfo field in typeof(ResearchAndDevelopment).GetFields(
            BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Public))
        {
            if (field.GetValue(rd) is Dictionary<string, KspScienceSubject> dict)
                return dict.Values;
        }
        return null;
    }

    private static List<string> GetBodyNames()
    {
        var names = new List<string>();
        if (FlightGlobals.Bodies != null)
        {
            foreach (var body in FlightGlobals.Bodies)
                if (body?.name != null)
                    names.Add(body.name);
        }
        // Sort longest-first so prefix matches don't shadow longer names (e.g. "Eve" vs "Eeloo").
        names.Sort((a, b) => b.Length.CompareTo(a.Length));
        return names;
    }

    private static void ParseId(string id, List<string> bodyNames, out string expId, out string body, out string situation, out string biome)
    {
        expId = "";
        body = "";
        situation = "";
        biome = "";

        if (string.IsNullOrEmpty(id)) return;

        int atIdx = id.IndexOf('@');
        if (atIdx < 0)
        {
            expId = id;
            return;
        }

        expId = id.Substring(0, atIdx);
        string rest = id.Substring(atIdx + 1);

        // Match body (longest names tried first)
        foreach (var bodyName in bodyNames)
        {
            if (rest.StartsWith(bodyName, StringComparison.Ordinal))
            {
                body = bodyName;
                rest = rest.Substring(bodyName.Length);
                break;
            }
        }

        // Match situation (longest names first to avoid "Landed" shadowing a hypothetical longer name)
        foreach (var sit in Situations)
        {
            if (rest.StartsWith(sit, StringComparison.Ordinal))
            {
                situation = sit;
                rest = rest.Substring(sit.Length);
                break;
            }
        }

        biome = rest;
    }

    private static string FilterJson(string json, string bodyFilter, string situationFilter)
    {
        var sb = new StringBuilder("[");
        bool first = true;
        int i = 0;
        while ((i = json.IndexOf("{\"id\":", i, StringComparison.Ordinal)) >= 0)
        {
            int end = FindObjectEnd(json, i);
            if (end < 0) break;
            var raw = json.Substring(i, end - i + 1);

            bool match = true;
            if (!string.IsNullOrEmpty(bodyFilter))
            {
                var bodyVal = ExtractString(raw, "\"body\":");
                if (!string.Equals(bodyVal, bodyFilter, StringComparison.OrdinalIgnoreCase))
                    match = false;
            }
            if (match && !string.IsNullOrEmpty(situationFilter))
            {
                var sitVal = ExtractString(raw, "\"situation\":");
                if (!string.Equals(sitVal, situationFilter, StringComparison.OrdinalIgnoreCase))
                    match = false;
            }

            if (match)
            {
                if (!first) sb.Append(',');
                first = false;
                sb.Append(raw);
            }
            i = end + 1;
        }
        sb.Append(']');
        return sb.ToString();
    }

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

    private static string JsonString(string s) =>
        "\"" + s.Replace("\\", "\\\\").Replace("\"", "\\\"")
               .Replace("\n", "\\n").Replace("\r", "\\r").Replace("\t", "\\t") + "\"";
}
