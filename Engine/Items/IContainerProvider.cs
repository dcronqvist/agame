using System.Collections.Generic;
using System.Numerics;
using AGame.Engine.Networking;
using GameUDPProtocol;

namespace AGame.Engine.Items;

public interface IContainerProvider
{
    string Name { get; }
    bool ShowPlayerContainer { get; }

    // Good practice to use yield etc. in this method.
    IEnumerable<ContainerSlot> GetSlots();
    IEnumerable<int> GetSlotSeekOrder();

    // Return true when an update inside the container has occured
    bool Update(Container owner, float deltaTime);
    Vector2 GetRenderSize();
    void RenderBackgroundUI(Vector2 topLeft, float deltaTime);

    void ReceiveProviderData(SetContainerProviderDataPacket packet);
    SetContainerProviderDataPacket GetContainerProviderData(int entityID);
    bool ShouldSendProviderData();
}

// Used for in game containers, like inventories, etc.
public abstract class ContainerProvider<T> : IContainerProvider where T : IPacketable, new()
{
    public abstract string Name { get; }
    public abstract bool ShowPlayerContainer { get; }

    public abstract void Receive(T data);
    public abstract T GetData(int entityID);
    public abstract IEnumerable<ContainerSlot> GetSlots();
    public abstract IEnumerable<int> GetSlotSeekOrder();
    public abstract bool Update(Container owner, float deltaTime);
    public abstract Vector2 GetRenderSize();
    public abstract void RenderBackgroundUI(Vector2 topLeft, float deltaTime);
    public abstract bool ShouldSendProviderData();

    public void ReceiveProviderData(SetContainerProviderDataPacket packet)
    {
        T data = new T();

        data.Populate(packet.Data, 0);

        this.Receive(data);
    }

    public SetContainerProviderDataPacket GetContainerProviderData(int entityID)
    {
        SetContainerProviderDataPacket sccp = new SetContainerProviderDataPacket();
        sccp.EntityID = entityID;
        sccp.Data = this.GetData(entityID).ToBytes();
        return sccp;
    }
}