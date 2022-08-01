using AGame.Engine.ECSys;
using AGame.Engine.ECSys.Components;
using AGame.Engine.World;

namespace AGame.Engine.Networking;

public interface IClientTickAction
{
    void Tick(GameClient client);
}

public class ClientUpdateChunkAction : IClientTickAction
{
    public int X { get; set; }
    public int Y { get; set; }
    public Chunk Chunk { get; set; }

    public ClientUpdateChunkAction(int x, int y, Chunk chunk)
    {
        this.X = x;
        this.Y = y;
        this.Chunk = chunk;
    }

    public void Tick(GameClient client)
    {
        client.GetWorld().UpdateChunk(this.X, this.Y, this.Chunk);
    }
}

public class ClientDiscardChunkAction : IClientTickAction
{
    public int X { get; set; }
    public int Y { get; set; }

    public ClientDiscardChunkAction(int x, int y)
    {
        this.X = x;
        this.Y = y;
    }

    public void Tick(GameClient client)
    {
        client.GetWorld().DiscardChunk(this.X, this.Y);
    }
}

public class ClientDestroyClientSideEntity : IClientTickAction
{
    int ServerSideEntityID { get; set; }

    public ClientDestroyClientSideEntity(int serverEntityID)
    {
        this.ServerSideEntityID = serverEntityID;
    }

    public void Tick(GameClient client)
    {
        client.DestroyClientSideEntity(this.ServerSideEntityID);
    }
}

public class ClientSetContainerContentAction : IClientTickAction
{
    public SetContainerContentPacket Packet { get; set; }

    public ClientSetContainerContentAction(SetContainerContentPacket packet)
    {
        this.Packet = packet;
    }

    public void Tick(GameClient client)
    {
        int serverSideEntity = Packet.EntityID;

        if (client.TryGetClientSideEntity(serverSideEntity, out Entity entity))
        {
            var container = entity.GetComponent<ContainerComponent>();
            var infos = Packet.Slots;

            foreach (var info in infos)
            {
                container.GetContainer().SetItemInSlot(info.SlotID, info.Item.Instance, info.ItemCount);
            }

            if (Packet.OpenInteract)
            {
                // The client should open up an interaction window between the player's inventory and the received container.
                client.ReceivedEntityOpenContainer = entity.ID;
            }
        }

    }
}

public class ClientSetContainerProviderDataAction : IClientTickAction
{
    public SetContainerProviderDataPacket Packet { get; set; }

    public ClientSetContainerProviderDataAction(SetContainerProviderDataPacket packet)
    {
        this.Packet = packet;
    }

    public void Tick(GameClient client)
    {
        int serverSideEntity = Packet.EntityID;

        if (client.TryGetClientSideEntity(serverSideEntity, out Entity entity))
        {
            var container = entity.GetComponent<ContainerComponent>();
            container.GetContainer().Provider.ReceiveProviderData(Packet);
        }
    }
}