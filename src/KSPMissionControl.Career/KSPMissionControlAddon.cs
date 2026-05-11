using KSPMissionControl.Career.Services;
using UnityEngine;

namespace KSPMissionControl.Career;

/// <summary>
/// KSP addon entry point. Persists across scene changes and pumps service caches
/// on the Unity main thread so kRPC procedures never call KSP APIs directly.
/// </summary>
[KSPAddon(KSPAddon.Startup.Instantly, once: true)]
public sealed class KSPMissionControlAddon : MonoBehaviour
{
    private float _lastTechTreeRefresh = -999f;

    private void Awake()
    {
        DontDestroyOnLoad(gameObject);
        Debug.Log("[KSPMissionControl] Addon started.");
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
    }

    private void OnDestroy()
    {
        Debug.Log("[KSPMissionControl] Addon destroyed.");
    }
}
