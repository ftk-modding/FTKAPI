using UnityEngine;

namespace FTKAPI.Managers.Networking;

public interface INetworkObjectCreator
{
    string Id { get; }
    
    bool SpawnOnRoomCreation { get; }

    GameObject InstantiateFromNetwork(int[] viewIds);

    GameObject InstantiateNew(out int[] viewIds);
}