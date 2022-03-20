using System;
using UnityEngine;

namespace FTKAPI.Managers.Networking;

public class StandardNetworkingObjectCreator : INetworkObjectCreator
{
    public string Id { get; }
    public bool SpawnOnRoomCreation => this.options.SpawnOnRoomCreation;
    
    private readonly Action<GameObject> initializer;
    private readonly NetworkObjectOptions options;

    public StandardNetworkingObjectCreator(string id, Action<GameObject> initializer, NetworkObjectOptions options)
    {
        this.Id = id;
        this.initializer = initializer;
        this.options = options;
    }

    public GameObject InstantiateFromNetwork(int[] viewIds)
    {
        var go = new GameObject
        {
            name = this.Id
        };
        if (this.options.CreateRootPhotonView)
        {
            go.AddComponent<PhotonView>();
        }

        this.initializer(go);
        
        var photonViews = go.GetComponentsInChildren<PhotonView>();
        if (photonViews.Length != viewIds.Length)
        {
            string error = $"Cannot create net object '{this.Id}': IDs count mismatch ({viewIds.Length} got; {photonViews.Length} expected)";
            Plugin.Log.LogError(error);
            return go;
        }

        for (var index = 0; index < photonViews.Length; index++)
        {
            photonViews[index].viewID = viewIds[index];
        }

        return go;
    }    
    
    public GameObject InstantiateNew(out int[] viewIds)
    {
        var go = new GameObject
        {
            name = this.Id
        };
        if (this.options.CreateRootPhotonView)
        {
            go.AddComponent<PhotonView>();
        }

        this.initializer(go);
        
        var photonViews = go.GetComponentsInChildren<PhotonView>();
        viewIds = new int[photonViews.Length];
        for (var index = 0; index < photonViews.Length; index++)
        {
            int allocatedId;
            if (this.options.AutoAllocateIds)
            {
                allocatedId = PhotonNetwork.AllocateViewID();
                photonViews[index].viewID = allocatedId;
            }
            else
            {
                allocatedId = photonViews[index].viewID;
            }
            viewIds[index] = allocatedId;
        }

        return go;
    }
}