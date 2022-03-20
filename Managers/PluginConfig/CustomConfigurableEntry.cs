using System;

namespace FTKAPI.Managers.PluginConfig;

public class CustomConfigurableEntry : IConfigurableEntry
{
    public string OwnerName { get; set; }
    public string Section { get; set; }
    public string Key { get; set; }
    public string Description { get; set; }
    public Type SettingType { get; set; }
    public object Value { get; set; }
}