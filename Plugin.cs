using System.Diagnostics;
using System.IO;
using System.Reflection;
using BepInEx;
using BepInEx.Bootstrap;
using BepInEx.Logging;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace MonitorEnhance;

[BepInProcess("Lethal Company.exe")]
[BepInPlugin("Paimon-Kawaii_MonitorEnhance", "MonitorEnhance", "2024.03.12")]
[BepInDependency("LethalExpansion", BepInDependency.DependencyFlags.SoftDependency)]
[BepInDependency("com.github.lethalmods.lethalexpansioncore", BepInDependency.DependencyFlags.SoftDependency)]
[BepInDependency("ShaosilGaming.GeneralImprovements", BepInDependency.DependencyFlags.SoftDependency)]
[BepInDependency("com.rune580.LethalCompanyInputUtils", BepInDependency.DependencyFlags.SoftDependency)]
public class Plugin : BaseUnityPlugin {
    internal static ManualLogSource LOGGER;
    internal delegate R Func<R, T>(T value);
    internal delegate R Supplier<R>();

    // GeneralImprovements - support
    internal static Func<Bounds, GameObject> CREATE_BOUNDS;

    // 3rd party plugin support (to disable/enable this plugin)
    private static bool _override = true;
    private static bool _onPlanet = false;
    public static bool IsActive
    {
        get => _onPlanet && (_override || ConfigUtil.IGNORE_OVERRIDE);
        set
        {
            if (_override != value) {
                _override = value;
                MethodBase prevFrame = new StackTrace().GetFrame(1).GetMethod();
                LOGGER.LogInfo(string.Format("MonitorEnhance was {0} by {1}.{2}.{3}",
                    value ? "enabled" : "disabled",
                    prevFrame.ReflectedType.Namespace,
                    prevFrame.ReflectedType.Name,
                    prevFrame.Name
                ));
            }
        }
    }

    /*
        Enable / Disable MonitorEnhance when not on a planet
    */
    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        _onPlanet = false;
        if (PlanetUtil.IsPlanet(scene))
        {
            GameObject monitorScreen = StartOfRound.Instance?.mapScreen?.mesh.gameObject;

            if (monitorScreen != null && monitorScreen.GetComponent<ScreenScript>() == null)
                monitorScreen.AddComponent<ScreenScript>();

            _onPlanet = true;
        }
    }

    private void OnSceneUnloaded(Scene scene)
    {
        _onPlanet = false;
    }

    // Plugin Startup
    private void Awake()
    {
        LOGGER = this.Logger;
        string pluginFolder = Path.Combine(Paths.PluginPath, "MonitorEnhance");
        SceneManager.sceneLoaded += OnSceneLoaded;
        SceneManager.sceneUnloaded += OnSceneUnloaded;

        // Load config values
        ConfigUtil.Setup(this.Config, pluginFolder);
        InputUtil.Setup();

        // Lethal Expansion / Lethal Expansion (core) support
        PlanetUtil.CheckPlugins();

        // GeneralImprovements support
        Supplier<bool> _gi = () => GeneralImprovements.Plugin.UseBetterMonitors.Value;
        if (Chainloader.PluginInfos.TryGetValue("ShaosilGaming.GeneralImprovements", out PluginInfo gi) && _gi.Invoke()) {
            CREATE_BOUNDS = x => new Bounds(
                new Vector3(
                    x.transform.position.x + -.2f,
                    x.transform.position.y + -.05f,
                    x.transform.position.z + .03f
                ),
                new Vector3(0, 1.05f, 1.36f)
            );
            LOGGER.LogInfo($"> Hooked into GeneralImprovements {gi.Metadata.Version}");
        } else {
            CREATE_BOUNDS = x => new Bounds(
                new Vector3(
                    x.transform.position.x + .06f,
                    x.transform.position.y + -.05f,
                    x.transform.position.z + .84f
                ),
                new Vector3(0, 1.05f, 1.36f)
            );
        }

        LOGGER.LogInfo("Enabled MonitorEnhance");
        LOGGER.LogInfo("Special Thanks to TheDeadSnake's TouchScreen");
    }
}
