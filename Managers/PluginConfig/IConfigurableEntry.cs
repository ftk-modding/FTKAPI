using System;

namespace FTKAPI.Managers.PluginConfig;

public interface IConfigurableEntry
{
    public string OwnerName { get; }
    public string Section { get; }
    public string Key { get; }
    public string Description { get; }
    public Type SettingType { get; }
    public object Value { get; set; }
}