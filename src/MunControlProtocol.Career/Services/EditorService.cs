using MunControlProtocol.Career.Internal;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace MunControlProtocol.Career.Services;

public static class EditorService
{
    internal static readonly StateCache<string> Cache = new();

    internal static string GetCurrentCraft() => Cache.Snapshot ?? "null";

    internal static void RefreshCache()
    {
        if (EditorLogic.fetch == null || EditorLogic.fetch.ship == null)
        {
            Cache.Update("null");
            return;
        }
        try
        {
            Cache.Update(BuildJson());
        }
        catch (Exception ex)
        {
            Debug.LogError("[MunControlProtocol] Failed to refresh editor cache: " + ex);
        }
    }

    private static string BuildJson()
    {
        var ship = EditorLogic.fetch.ship;
        string editorType = ship.shipFacility == EditorFacility.VAB ? "VAB" : "SPH";

        double totalMassT = 0.0;
        double totalCost = 0.0;
        int crewCapacity = 0;

        foreach (Part part in ship.parts)
        {
            double resourceMassT = 0.0;
            foreach (PartResource res in part.Resources)
                resourceMassT += res.amount * res.info.density;
            totalMassT += part.mass + resourceMassT;
            totalCost += part.partInfo.cost;
            crewCapacity += part.CrewCapacity;
        }

        var sb = new StringBuilder();
        sb.Append("{\"name\":").Append(JsonString(ship.shipName));
        sb.Append(",\"editorType\":").Append(JsonString(editorType));
        sb.Append(",\"partCount\":").Append(ship.parts.Count);
        sb.Append(",\"totalMassT\":").Append(totalMassT.ToString("R"));
        sb.Append(",\"totalCost\":").Append(totalCost.ToString("R"));
        sb.Append(",\"crewCapacity\":").Append(crewCapacity);
        sb.Append(",\"parts\":[");

        bool firstPart = true;
        foreach (Part part in ship.parts)
        {
            double resourceMassT = 0.0;
            foreach (PartResource res in part.Resources)
                resourceMassT += res.amount * res.info.density;

            if (!firstPart) sb.Append(',');
            firstPart = false;

            sb.Append("{\"name\":").Append(JsonString(part.partInfo.name));
            sb.Append(",\"title\":").Append(JsonString(part.partInfo.title));
            sb.Append(",\"massT\":").Append(((double)part.mass).ToString("R"));
            sb.Append(",\"resourceMassT\":").Append(resourceMassT.ToString("R"));
            sb.Append(",\"cost\":").Append(((double)part.partInfo.cost).ToString("R"));
            sb.Append(",\"stageIndex\":").Append(part.inverseStage);

            sb.Append(",\"resources\":[");
            bool firstRes = true;
            foreach (PartResource res in part.Resources)
            {
                if (!firstRes) sb.Append(',');
                firstRes = false;
                sb.Append("{\"name\":").Append(JsonString(res.resourceName));
                sb.Append(",\"amount\":").Append(res.amount.ToString("R"));
                sb.Append(",\"maxAmount\":").Append(res.maxAmount.ToString("R"));
                sb.Append('}');
            }
            sb.Append(']');

            try { AppendModuleInfo(sb, part); }
            catch (Exception ex) { Debug.LogWarning("[MunControlProtocol] Module info failed for " + part.partInfo.name + ": " + ex.Message); }

            sb.Append('}');
        }

        sb.Append("]}");
        return sb.ToString();
    }

    private static void AppendModuleInfo(StringBuilder sb, Part part)
    {
        ModuleEngines? engineMod = null;
        ModuleDataTransmitter? antennaMod = null;
        bool hasCommand = false;
        ModuleSAS? sasMod = null;
        ModuleDeployableSolarPanel? solarMod = null;

        foreach (PartModule mod in part.Modules)
        {
            if (engineMod == null && mod is ModuleEngines e)
                engineMod = e;
            else if (antennaMod == null && mod is ModuleDataTransmitter a)
                antennaMod = a;
            else if (!hasCommand && mod is ModuleCommand)
                hasCommand = true;
            else if (sasMod == null && mod is ModuleSAS s)
                sasMod = s;
            else if (solarMod == null && mod is ModuleDeployableSolarPanel sp)
                solarMod = sp;
        }

        if (engineMod != null)
        {
            float ispVac = engineMod.atmosphereCurve.Evaluate(0f);
            float ispAsl = engineMod.atmosphereCurve.Evaluate(1f);
            double thrustVac = engineMod.maxThrust;
            double thrustAsl = ispVac > 0f ? thrustVac * ispAsl / ispVac : 0.0;

            sb.Append(",\"engine\":{");
            sb.Append("\"thrustVacuum\":").Append(thrustVac.ToString("R"));
            sb.Append(",\"thrustAsl\":").Append(thrustAsl.ToString("R"));
            sb.Append(",\"ispVacuum\":").Append(((double)ispVac).ToString("R"));
            sb.Append(",\"ispAsl\":").Append(((double)ispAsl).ToString("R"));
            sb.Append(",\"fuelFlowVacuum\":").Append(((double)engineMod.maxFuelFlow).ToString("R"));
            sb.Append(",\"propellants\":[");
            bool firstProp = true;
            foreach (Propellant prop in engineMod.propellants)
            {
                if (!firstProp) sb.Append(',');
                firstProp = false;
                sb.Append(JsonString(prop.name));
            }
            sb.Append("]}");
        }

        if (antennaMod != null)
        {
            sb.Append(",\"antenna\":{");
            sb.Append("\"range\":").Append(antennaMod.antennaPower.ToString("R"));
            sb.Append(",\"type\":").Append(JsonString(ToTitleCase(antennaMod.antennaType.ToString())));
            sb.Append(",\"combinable\":").Append(antennaMod.antennaCombinable ? "true" : "false");
            sb.Append(",\"packetSize\":").Append(((double)antennaMod.packetSize).ToString("R"));
            sb.Append(",\"packetInterval\":").Append(((double)antennaMod.packetInterval).ToString("R"));
            sb.Append('}');
        }

        if (part.Resources.Count > 0)
        {
            sb.Append(",\"tank\":{\"resources\":[");
            bool firstRes = true;
            foreach (PartResource res in part.Resources)
            {
                if (!firstRes) sb.Append(',');
                firstRes = false;
                sb.Append("{\"resourceName\":").Append(JsonString(res.resourceName));
                sb.Append(",\"maxAmount\":").Append(res.maxAmount.ToString("R"));
                sb.Append('}');
            }
            sb.Append("]}");
        }

        if (hasCommand)
        {
            sb.Append(",\"command\":{");
            sb.Append("\"crewCapacity\":").Append(part.CrewCapacity);
            sb.Append(",\"hasSas\":").Append(sasMod != null ? "true" : "false");
            sb.Append(",\"sasLevel\":").Append(sasMod != null ? sasMod.SASServiceLevel : 0);
            sb.Append(",\"hibernationCharge\":0}");
        }

        if (solarMod != null)
        {
            sb.Append(",\"solarPanel\":{");
            sb.Append("\"chargeRate\":").Append(((double)solarMod.chargeRate).ToString("R"));
            sb.Append(",\"retractable\":").Append(solarMod.retractable ? "true" : "false");
            sb.Append('}');
        }
    }

    private static string ToTitleCase(string s)
    {
        if (string.IsNullOrEmpty(s)) return s;
        return char.ToUpperInvariant(s[0]) + s.Substring(1).ToLowerInvariant();
    }

    private static string JsonString(string s) =>
        "\"" + s.Replace("\\", "\\\\").Replace("\"", "\\\"")
               .Replace("\n", "\\n").Replace("\r", "\\r").Replace("\t", "\\t") + "\"";
}
