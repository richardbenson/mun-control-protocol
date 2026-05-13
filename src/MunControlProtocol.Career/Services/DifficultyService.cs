using MunControlProtocol.Career.Internal;
using System;
using UnityEngine;

namespace MunControlProtocol.Career.Services;

public static class DifficultyService
{
    internal static readonly StateCache<string> Cache = new();

    internal static string GetDifficultySettings() => Cache.Snapshot ?? "{}";

    internal static void RefreshCache()
    {
        try
        {
            Cache.Update(BuildJson());
        }
        catch (Exception ex)
        {
            Debug.LogError("[MunControlProtocol] Failed to refresh difficulty cache: " + ex);
        }
    }

    private static string BuildJson()
    {
        var pp = HighLogic.CurrentGame?.Parameters;
        if (pp == null) return "{}";

        var commNet    = pp.CustomParams<CommNet.CommNetParams>();
        var difficulty = pp.Difficulty;
        var career     = pp.Career;

        double commNetRange     = commNet?.rangeModifier            ?? 1.0;
        double dsnModifier      = commNet?.DSNModifier              ?? 1.0;
        bool   reqSignalCtrl    = commNet?.requireSignalForControl  ?? false;
        // Occlusion is active when the vacuum multiplier is > 0.
        bool   occludeBodies    = (commNet?.occlusionMultiplierVac  ?? 0f) > 0f;
        bool   plasmaBlackout   = commNet?.plasmaBlackout           ?? false;

        double sciReward        = career?.ScienceGainMultiplier ?? 1.0;
        double fundsReward      = career?.FundsGainMultiplier   ?? 1.0;
        double repReward        = career?.RepGainMultiplier     ?? 1.0;
        double fundsPenalty     = career?.FundsLossMultiplier   ?? 1.0;
        double repPenalty       = career?.RepLossMultiplier     ?? 1.0;

        bool   enableCommNet    = difficulty?.EnableCommNet         ?? true;
        double reentryHeat      = difficulty?.ReentryHeatScale     ?? 1.0;
        bool   respawn          = difficulty?.MissingCrewsRespawn  ?? true;

        return "{\"commNetRangeModifier\":"         + commNetRange.ToString("R")
             + ",\"dsnModifier\":"                  + dsnModifier.ToString("R")
             + ",\"requireSignalForControl\":"      + (reqSignalCtrl  ? "true" : "false")
             + ",\"enableCommNet\":"                + (enableCommNet  ? "true" : "false")
             + ",\"occludeBodies\":"                + (occludeBodies  ? "true" : "false")
             + ",\"plasmaBlackout\":"               + (plasmaBlackout ? "true" : "false")
             + ",\"scienceRewardsMultiplier\":"     + sciReward.ToString("R")
             + ",\"fundsRewardsMultiplier\":"       + fundsReward.ToString("R")
             + ",\"reputationRewardsMultiplier\":"  + repReward.ToString("R")
             + ",\"fundsPenaltiesMultiplier\":"     + fundsPenalty.ToString("R")
             + ",\"reputationPenaltiesMultiplier\":" + repPenalty.ToString("R")
             + ",\"reentryHeatingMultiplier\":"     + reentryHeat.ToString("R")
             + ",\"missingCrewsRespawn\":"          + (respawn ? "true" : "false") + "}";
    }
}
