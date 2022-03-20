using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FTKAPI.Managers.Networking;
using FTKAPI.Utils;
using HarmonyLib;
using UnityEngine;

namespace FTKAPI.Managers;

public class NetworkManager : BaseManager<NetworkManager>
{
    private const byte PhotonEventOpCode = 50;
    
    private const byte OpSpawnObject = 0;
    private const byte OpCompatibilityCheck = 1;
    
    private readonly Dictionary<string, INetworkObjectCreator> netObjectCreators = new();

    [HarmonyPatch(typeof(PhotonListener), nameof(PhotonListener.OnCreatedRoom))]
    // ReSharper disable once InconsistentNaming
    class PhotonListener_OnCreatedRoom
    {
        static void Postfix()
        {
            if (IsInited || GetPluginIds().Any()) Instance.OnCreatedRoom();
        }
    }

    internal override void Init()
    {
        PhotonNetwork.OnEventCall += OnPhotonEventCall;
        Plugin.Instance.Harmony.PatchNested<NetworkManager>();
    }

    /// <summary>
    /// Registers custom GameObject factory that can spawn it on both client and server side with sync capabilities.
    /// NOTE: Try to avoid creating a lot of net objects, since you'll have to share with game and other plugins only 1000 "object slots" per player.  
    /// </summary>
    public static void RegisterNetObject(string id, Action<GameObject> initializer, NetworkObjectOptions options = null)
    {
        Instance.RegisterNetObject(new StandardNetworkingObjectCreator(id, initializer, options ?? new NetworkObjectOptions()));
    }

    public static GameObject SpawnNetworkObject(string key)
    {
        var creators = Instance.netObjectCreators;
        if (!creators.TryGetValue(key, out var creator))
        {
            Plugin.Log.LogError($"Cannot spawn net object '{key}' - key is not registered");
            return null;
        }

        var go = creator.InstantiateNew(out var viewIds);
        RaiseCustomEvent(OpSpawnObject, key, viewIds);
        
        return go;
    }
    
    public void RegisterNetObject(INetworkObjectCreator creator)
    {
        if (this.netObjectCreators.ContainsKey(creator.Id))
        {
            Plugin.Log.LogError($"Cannot register net object '{creator.Id}' - key is already present");
            return;
        }

        this.netObjectCreators[creator.Id] = creator;
    }

    private void OnPhotonEventCall(byte eventcode, object content, int senderid)
    {
        if (eventcode == PhotonEventOpCode)
        {
            object[] data = (object[])content;
            var internalOpCode = (byte)data[0];
            switch (internalOpCode)
            {
                case OpSpawnObject:
                {
                    var key = (string)data[1];
                    if (!this.netObjectCreators.TryGetValue(key, out var creator))
                    {
                        Plugin.Log.LogError($"Cannot create net object '{key}' on client - key not registered!");
                        return;
                    }

                    creator.InstantiateFromNetwork((int[])data[2]);
                    return;
                }

                case OpCompatibilityCheck:
                {
                    CompatibilityCheck((string[])data[1]);
                    return;
                }
            }
        }
    }

    void OnCreatedRoom()
    {
        Plugin.Log.LogInfo("OnCreatedRoom");
        RaiseCustomEvent(OpCompatibilityCheck, GetPluginIds());
        
        foreach (var pair in this.netObjectCreators.Where(kv => kv.Value.SpawnOnRoomCreation))
        {
            try
            {
                SpawnNetworkObject(pair.Key);
            }
            catch (Exception ex)
            {
                Plugin.Log.LogError($"Error on spawning '{pair.Key}': {ex}");
            }
        }
    }

    static void RaiseCustomEvent(params object[] eventContent)
    {
        PhotonNetwork.RaiseEvent(
            eventCode: PhotonEventOpCode,
            eventContent: eventContent,
            sendReliable: true,
            options: new RaiseEventOptions
            {
                Receivers = ReceiverGroup.Others,
                CachingOption = EventCaching.AddToRoomCache
            });
    }

    void CompatibilityCheck(string[] hostPlugins)
    {
        var localPlugins = GetPluginIds();
        
        var hostMissingPlugins = localPlugins.Except(hostPlugins).ToArray();
        var localMissingPlugins = hostPlugins.Except(localPlugins).ToArray();

        if (hostMissingPlugins.Any() || localMissingPlugins.Any())
        {
            var sb = new StringBuilder();
            sb.AppendLine(
                "<color=red>My plugins are incompatible with host! Game may be not working properly!</color>");
            
            if (hostMissingPlugins.Any())
            {
                sb.AppendLine("Missing on host:");
                foreach (var s in hostMissingPlugins)
                {
                    sb.Append("- ").AppendLine(s);
                }
            }
            
            if (localMissingPlugins.Any())
            {
                sb.AppendLine("Missing locally:");
                foreach (var s in localMissingPlugins)
                {
                    sb.Append("- ").AppendLine(s);
                }
            }
            
            GameFlow.Instance.ChatMessage(sb.ToString());
            uiChatBox.Instance.m_TextBoxRoot.gameObject.SetActive(true);
        }
    }

    static string[] GetPluginIds() => Plugin.Instance.NetworkCompatibilityHandler.NetworkModList.ToArray();
}