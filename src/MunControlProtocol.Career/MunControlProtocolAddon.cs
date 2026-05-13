using MunControlProtocol.Career.Services;
using UnityEngine;

namespace MunControlProtocol.Career;

/// <summary>
/// KSP addon entry point. Persists across scene changes and pumps service caches
/// on the Unity main thread so kRPC procedures never call KSP APIs directly.
/// </summary>
[KSPAddon(KSPAddon.Startup.Instantly, once: true)]
public sealed class MunControlProtocolAddon : MonoBehaviour
{
    private float _lastTechTreeRefresh  = -999f;
    private float _lastScienceRefresh   = -999f;
    private float _lastBuildingsRefresh = -999f;

    private void Awake()
    {
        DontDestroyOnLoad(gameObject);
        Debug.Log("[MunControlProtocol] Addon started.");
    }

    private void Update()
    {
        // Only refresh in scenes where ResearchAndDevelopment.Instance is initialised.
        var scene = HighLogic.LoadedScene;
        if (scene != GameScenes.SPACECENTER &&
            scene != GameScenes.EDITOR &&
            scene != GameScenes.FLIGHT &&
            scene != GameScenes.TRACKSTATION)
            return;

        // Tech tree and parts unlock state change only on R&D actions; once per second is sufficient.
        if (Time.realtimeSinceStartup - _lastTechTreeRefresh < 1f) return;
        _lastTechTreeRefresh = Time.realtimeSinceStartup;

        TechTreeService.RefreshCache();
        PartsService.RefreshCache();

        // Science matrix is large; refresh every 5 seconds is sufficient.
        if (Time.realtimeSinceStartup - _lastScienceRefresh >= 5f)
        {
            _lastScienceRefresh = Time.realtimeSinceStartup;
            ScienceService.RefreshCache();
        }

        // Buildings and difficulty change only when the player upgrades or edits settings; 5 s is plenty.
        if (Time.realtimeSinceStartup - _lastBuildingsRefresh >= 5f)
        {
            _lastBuildingsRefresh = Time.realtimeSinceStartup;
            BuildingsService.RefreshCache();
            DifficultyService.RefreshCache();
            KerbalsService.RefreshCache();
        }
    }

    private void OnDestroy()
    {
        Debug.Log("[MunControlProtocol] Addon destroyed.");
    }
}
