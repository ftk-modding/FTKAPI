using System.Collections.Generic;
using BepInEx;
using BepInEx.Logging;
using FTKAPI.Compatibility;
using FTKAPI.Utils;
using HarmonyLib;

namespace FTKAPI
{
    [BepInPlugin(PluginInfo.PLUGIN_GUID, PluginInfo.PLUGIN_NAME, PluginInfo.PLUGIN_VERSION)]
    [BepInProcess("FTK.exe")]
    public class Plugin : BaseUnityPlugin
    {
        internal static Plugin Instance;
        internal static ManualLogSource Log { get; set; }

        internal readonly List<object> Managers = new();

        internal readonly NetworkCompatibilityHandler NetworkCompatibilityHandler = new();

        internal Harmony Harmony = new(PluginInfo.PLUGIN_GUID);

        private void Awake()
        {
            Instance = this;
            Log = Logger;
            this.Harmony.PatchNested<Plugin>();
            Utils.Logger.Init();

            // This is required for proper serialization for CustomObjects
            // which use an Enum as an identifier.
            // Otherwise, saving and loading games would not work if this
            // was false.
            FullSerializer.fsConfig.SerializeEnumsAsInteger = true;
            NetworkCompatibilityHandler.ScanPluginsForNetworkCompat();

            // Plugin startup logic
            Log.LogInfo($"Plugin {PluginInfo.PLUGIN_GUID} is loaded!");
        }
        
        [HarmonyPatch(typeof(SerializeGO), "ShowBugForm")]
        class NukeShowBugForm
        {
            static bool Prefix()
            {
                Log.LogWarning("Mods are loaded - do not report bugs to IronOak Games. Please report all bugs to the mod developer.");
                return false;
            }
        }
    }
}