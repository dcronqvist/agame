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