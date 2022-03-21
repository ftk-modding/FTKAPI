using System;
using System.Collections.Generic;
using System.Linq;

// Stolen from https://github.com/risk-of-thunder/R2API/blob/master/R2API/Utils/NetworkCompatibility.cs
// - slightly modified

namespace FTKAPI.Compatibility;

/// <summary>
/// Enum used for telling whether or not the mod should be needed by everyone in multiplayer games.
/// Also can specify if the mod does not work in multiplayer.
/// </summary>
public enum CompatibilityLevel {
    NoNeedForSync,
    EveryoneMustHaveMod,
    //BreaksMultiplayer //todo
}

/// <summary>
/// Enum used for telling whether or not the same mod version should be used by both the server and the clients.
/// This enum is only useful if CompatibilityLevel.EveryoneMustHaveMod was chosen.
/// </summary>
public enum VersionStrictness {
    DifferentModVersionsAreOk,
    EveryoneNeedSameModVersion
}

/// <summary>
/// Attribute to have at the top of your BaseUnityPlugin class if
/// you want to specify if the mod should be installed by everyone in multiplayer games or not.
/// If the mod is required to be installed by everyone, you'll need to also specify if the same mod version should be used by everyone or not.
/// By default, it's supposed that everyone needs the mod and the same version.
/// e.g: [NetworkCompatibility(CompatibilityLevel.NoNeedForSync)]
/// </summary>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Assembly)]
public class NetworkCompatibility : Attribute {

    /// <summary>
    /// Used for telling whether or not the mod should be needed by everyone in multiplayer games.
    /// </summary>
    public CompatibilityLevel CompatibilityLevel { get; internal set; }

    /// <summary>
    /// Enum used for telling whether or not the same mod version should be used by both the server and the clients.
    /// This enum is only useful if CompatibilityLevel.EveryoneMustHaveMod was chosen.
    /// </summary>
    public VersionStrictness VersionStrictness { get; internal set; }

    public NetworkCompatibility(
        CompatibilityLevel compatibility = CompatibilityLevel.EveryoneMustHaveMod,
        VersionStrictness versionStrictness = VersionStrictness.EveryoneNeedSameModVersion) {
        this.CompatibilityLevel = compatibility;
        this.VersionStrictness = versionStrictness;
    }
}

internal class NetworkCompatibilityHandler {
    internal const char ModGuidAndModVersionSeparator = ';';
    internal readonly HashSet<string> ModList = new();
    public List<string> NetworkModList = new();

    public void ScanPluginsForNetworkCompat() {
        foreach (var pair in BepInEx.Bootstrap.Chainloader.PluginInfos)
        {
            var pluginInfo = pair.Value;
            try {
                var modGuid = pluginInfo.Metadata.GUID;
                var modVer = pluginInfo.Metadata.Version;

                if (modGuid == PluginInfo.PLUGIN_GUID) {
                    continue;
                }

                if (pluginInfo.Dependencies.All(dependency => dependency.DependencyGUID != PluginInfo.PLUGIN_GUID || dependency.Flags == BepInEx.BepInDependency.DependencyFlags.SoftDependency)) {
                    continue;
                }

                TryGetNetworkCompatibility(pluginInfo.Instance.GetType(), out var networkCompatibility);
                if (networkCompatibility.CompatibilityLevel == CompatibilityLevel.EveryoneMustHaveMod) {
                    this.ModList.Add(networkCompatibility.VersionStrictness == VersionStrictness.EveryoneNeedSameModVersion
                        ? modGuid + ModGuidAndModVersionSeparator + modVer
                        : modGuid);
                }
            }
            catch (Exception e) {
                Plugin.Log.LogError($"Exception in ScanPluginsForNetworkCompat while scanning plugin {pluginInfo.Metadata.GUID}");
                Plugin.Log.LogError($"{PluginInfo.PLUGIN_NAME} Failed to properly scan the assembly." + Environment.NewLine +
                                    "Please make sure you are compiling against net35 " +
                                    "and not anything else when making a plugin for FTK !" +
                                    Environment.NewLine + e);
            }
        }

        AddToNetworkModList();
    }

    public void SetNetworkCompatibility(BepInEx.PluginInfo pluginInfo, NetworkCompatibility networkCompatibility) 
    {
        var modGuid = pluginInfo.Metadata.GUID;
        var modVer = pluginInfo.Metadata.Version;
        
        if (networkCompatibility.CompatibilityLevel == CompatibilityLevel.EveryoneMustHaveMod)
        {
            var entry = networkCompatibility.VersionStrictness == VersionStrictness.EveryoneNeedSameModVersion
                ? modGuid + ModGuidAndModVersionSeparator + modVer
                : modGuid;
            if (!this.ModList.Contains(entry))
            {
                this.ModList.Add(entry);
            }
        }
    }

    private static void TryGetNetworkCompatibility(Type baseUnityPluginType, out NetworkCompatibility networkCompatibility) {
        networkCompatibility = new NetworkCompatibility();

        foreach (NetworkCompatibility assemblyAttribute in baseUnityPluginType.Assembly.GetCustomAttributes(typeof(NetworkCompatibility), true))
        {
            networkCompatibility.CompatibilityLevel = assemblyAttribute.CompatibilityLevel;
            networkCompatibility.VersionStrictness = assemblyAttribute.VersionStrictness;
            return;
        }

        foreach (NetworkCompatibility assemblyAttribute in baseUnityPluginType.GetCustomAttributes(typeof(NetworkCompatibility), true))
        {
            networkCompatibility.CompatibilityLevel = assemblyAttribute.CompatibilityLevel;
            networkCompatibility.VersionStrictness = assemblyAttribute.VersionStrictness;
            return;
        }
    }

    private void AddToNetworkModList() {
        if (this.ModList.Count != 0) {
            this.ModList.Add(PluginInfo.PLUGIN_GUID + ModGuidAndModVersionSeparator + PluginInfo.PLUGIN_VERSION);
            var sortedModList = this.ModList.ToList();
            sortedModList.Sort(StringComparer.InvariantCulture);
            Plugin.Log.LogInfo("[NetworkCompatibility] Adding to the networkModList : ");
            foreach (var mod in sortedModList) {
                Plugin.Log.LogInfo(mod);
                NetworkModList.Add(mod);
            }
        }
    }
}