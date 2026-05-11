using KSPMissionControl.Career.Internal;
using System;
using UnityEngine;

namespace KSPMissionControl.Career.Services;

public static class BuildingsService
{
    internal static readonly StateCache<string> Cache = new();

    internal static string GetBuildingLevels() => Cache.Snapshot ?? "{}";

    internal static void RefreshCache()
    {
        try
        {
            Cache.Update(BuildJson());
        }
        catch (Exception ex)
        {
            Debug.LogError("[KSPMissionControl] Failed to refresh buildings cache: " + ex);
        }
    }

    private static string BuildJson()
    {
        int vab     = GetLevel("SpaceCenter/VehicleAssemblyBuilding");
        int sph     = GetLevel("SpaceCenter/SpaceplaneHangar");
        int lp      = GetLevel("SpaceCenter/LaunchPad");
        int rwy     = GetLevel("SpaceCenter/Runway");
        int ts      = GetLevel("SpaceCenter/TrackingStation");
        int rd      = GetLevel("SpaceCenter/ResearchAndDevelopment");
        int ac      = GetLevel("SpaceCenter/AstronautComplex");
        int mc      = GetLevel("SpaceCenter/MissionControl");
        int admin   = GetLevel("SpaceCenter/Administration");

        return "{\"vab\":" + vab
             + ",\"sph\":" + sph
             + ",\"launchpad\":" + lp
             + ",\"runway\":" + rwy
             + ",\"trackingStation\":" + ts
             + ",\"researchAndDevelopment\":" + rd
             + ",\"astronautComplex\":" + ac
             + ",\"missionControl\":" + mc
             + ",\"administration\":" + admin + "}";
    }

    // GetFacilityLevel returns a float 0..1 where 0 = unupgraded, 1 = max level.
    // Multiply by (levelCount - 1) to get the 0-indexed integer level.
    private static int GetLevel(string facilityId)
    {
        float normalized = ScenarioUpgradeableFacilities.GetFacilityLevel(facilityId);
        int levelCount = ScenarioUpgradeableFacilities.GetFacilityLevelCount(facilityId);
        if (levelCount <= 1) return 0;
        return Mathf.RoundToInt(normalized * (levelCount - 1));
    }
}
