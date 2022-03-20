namespace FTKAPI.Managers.Networking;

public class NetworkObjectOptions
{
    /// <summary>
    /// If true, automatically spawns GameObject on room creation. Room owner will be object owner.
    /// </summary>
    public bool SpawnOnRoomCreation { get; set; } = true;

    /// <summary>
    /// If true, automatically allocate IDs for all PhotonView instances of instantiated object.
    /// </summary>
    public bool AutoAllocateIds { get; set; } = true;

    /// <summary>
    /// If true, for root GameObject, PhotonView component will be added automatically.
    /// </summary>
    public bool CreateRootPhotonView { get; set; } = true;
}