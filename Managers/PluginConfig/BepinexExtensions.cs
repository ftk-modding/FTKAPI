using System.Reflection;
using BepInEx;
using BepInEx.Configuration;

namespace FTKAPI.Managers.PluginConfig;

public static class BepinexExtensions
{
    public static string GetPluginName(this ConfigEntryBase entry)
    {
        //private readonly BepInPlugin _ownerMetadata;
        var configFile = entry.ConfigFile;
        var plugin = (BepInPlugin)configFile
            ?.GetType().GetField("_ownerMetadata", BindingFlags.NonPublic | BindingFlags.Instance)
            ?.GetValue(configFile);
        if (plugin != null)
        {
            return plugin.Name;
        }

        return null;
    }
}