using UnityEngine;

namespace FTKAPI.Managers.Networking;

public interface INetworkObjectCreator
{
    /// <summary>
    /// Linked id for object created by this creator.
    /// </summary>
    string Id { get; }
    
    /// <summary>
    /// When true, instantiation will be triggered on room creation.
    /// </summary>
    bool SpawnOnRoomCreation { get; }

    /// <summary>
    /// Creates object when triggered from network.
    /// </summary>
    /// <param name="viewIds">Ids for all PhotonView components in spawned object</param>
    GameObject InstantiateFromNetwork(int[] viewIds);

    /// <summary>
    /// Creates object when triggered with <see cref="NetworkManager.SpawnNetworkObject"/> locally.
    /// </summary>
    /// <param name="viewIds">Ids for all PhotonView components in spawned object</param>
    GameObject InstantiateNew(out int[] viewIds);
}