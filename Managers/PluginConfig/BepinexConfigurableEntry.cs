using System;
using BepInEx.Configuration;

namespace FTKAPI.Managers.PluginConfig;

public class BepinexConfigurableEntry : IConfigurableEntry
{
    public ConfigEntryBase BaseEntry { get; }

    public string OwnerName { get; }
    public string Section => this.BaseEntry.Definition.Section;
    public string Key => this.BaseEntry.Definition.Key;
    public string Description => this.BaseEntry.Description.Description;
    public Type SettingType => this.BaseEntry.SettingType;

    public object Value
    {
        get => this.BaseEntry.BoxedValue;
        set => this.BaseEntry.BoxedValue = value;
    }

    public BepinexConfigurableEntry(ConfigEntryBase baseEntry)
    {
        this.BaseEntry = baseEntry;
        this.OwnerName = baseEntry.GetPluginName();
    }
}