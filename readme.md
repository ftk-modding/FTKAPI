# For The King API (FTKAPI) - Modding Library

FTKAPI is a work-in-progress modding framework to aid developers in creating For The King mods, by providing centralized and simplified APIs for the game.

## Documentation
There is a provided example plugin(s) made with the use of the library located in the Examples folder.

## Features
#### Objects
- **CustomItem**  
  Can create an item with full custom functionality or inherit functionality from existing items (herbs, orbs, armor, weapons).
- **CustomClass**  
  Can create a character class with full custom functionality or inherit functionality from existing classes (blacksmith, hunter, etc.).
- **CustomLocalizedString**  
  Can create a string/text that supports additional languages.

#### Managers
- **AssetManager**  
  Allows importing custom assets such as models and images, to be used with custom or existing GameObjects.
- **ItemManager**  
  Allows the modification and addition of items. As simple as ```ItemManager.AddItem(new CustomItem());``` !
- **ClassManager**  
  Allows the modification and addition of classes. As simple as ```ClassManager.AddClass(new CustomClass());``` !
- **NetworkManager**  
  Allows registering custom objects with network connectivity. Example below.
- **PluginConfigManager**  
  Allows plugins to add configuration entry directly into game menu. Example below.

## Networking
When using `NetworkManager`, you can register object that can be instantiated on your and other clients side and have working communication between users. Simplest way to do that is to use RPC-commands through your custom inheritor of `PunBehavior` class. Here is example of such implementation:
```c#
// MyPluginRpc.cs
class MyPluginRpc : PunBehavior 
{
    public void Start() 
    {
        if (PhotonNetwork.IsMasterClient) 
        {
            SendMessage("Hello from master client");
        }
    }

    public void SendMessage(string message) 
    {
        this.photonView.RPC("SendMessagePRC", PhotonTargets.Others, message);
    }
    
    [PunRPC]
    public void SendMessagePRC(string message) 
    {
        Logger.LogInfo($"Received '{message}'!");    
    }
}
```

Then, you can register `GameObject` that would have this Component. Empty `GameObject` is created by default, so you just have to add necessary components. This function will be called on both sides during creation.
> NOTE: you can modify this GameObject in any way you want, but in order for RPC component to work, it's parent GameObject should have PhotonView which is populated by default.

> NOTE: you shouldn't create a lot of there objects. Ideally it should be one per plugin.

```c#
// during plugin initialization
NetworkManager.RegisterNetObject("MyPluginGuid.SyncObject", 
    gameObject => gameObject.AddComponent<MyPluginRpc>(),
    new () { SpawnOnRoomCreation = false }
);
```

Then you can just call `NetworkManager.SpawnNetworkObject` with same id you provided, and this object would be instantiated and "network-enabled"!

```c#
// somewhere in-room
var networkGameObject = NetworkManager.SpawnNetworkObject("MyPluginGuid.SyncObject");
// other clients will log "Received 'Hello from master client!'" when object will be created."
```

Note that in example above `SpawnOnRoomCreation` is set to `false` during registration, but it is `true` by default. So if you don't provide options, you don't have to create it manually.

You can read more about network synchronization FTK uses in the official Photon reference: https://doc.photonengine.com/en-us/pun/v1/gameplay/synchronization-and-state


## In-Game configuration
  Allows plugins to add configuration entry directly into game menu. 
```c#
// create BepInEx config entry 
var entry = Config.Bind("MySection", "MyConfigKey", "MyDescription", defaultValue: 42);
// add entry to conifguration screen
PluginConfigManager.RegisterBinding(entry);
```

**NOTE**: In order for options to be saved automatically, you'll need to set `Config.SaveOnConfigSet = true;` somewhere in your code.

You can also create your custom configuration entries that aren't explicitly tied to BepInEx configurations by implementing `IConfigurableEntry` interface or use `CustomConfigurableEntry`.
```c#
PluginConfigManager.Instance.RegisterSingleBinding(new CustomConfigurableEntry() {
    Key = "MyConfigKey",
    Section = "MySection",
    Description = "MyDescription",
    OwnerName = "MyPluginName",
    SettingType = typeof(bool),
    Value = true
});
```

## Build

1. Download repository with submodules
2. dotnet restore
3. (optional) Set `BuiltPluginDestPath` and `BuiltPluginDestPath2` in FTKAPI.csproj to copy resulting binaries to your plugin directory
4. Build using with whatever tool you have: Visual Studio, Visual Studio Code, Rider or dotnet cli directly


## Special thanks to

[lulzsun](https://github.com/lulzsun)[FTKModLib](https://github.com/lulzsun/FTKModLib) that explicitly allowed me to re-use a lot of his code

And by transitivity rule, all these other folks whose work helped him: 

[999gary](https://github.com/999gary)/[FTKExampleItemMod](https://github.com/999gary/FTKExampleItemMod) used as learning reference

[CarbonNikm](https://github.com/CarbonNikm)/[FTK-Debugging-Commands](https://github.com/CarbonNikm/FTK-Debugging-Commands) used as learning reference

Valheim-Modding/[Jotunn](https://github.com/Valheim-Modding/Jotunn) used as learning reference
