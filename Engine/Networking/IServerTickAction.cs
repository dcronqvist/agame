using AGame.Engine.Configuration;
using AGame.Engine.World;
using GameUDPProtocol;

namespace AGame.Engine.Networking;

public interface IServerTickAction
{
    void Tick(GameServer server);
}

public interface IClientTickAction
{
    void Tick(GameClient client);
}

public class DestroyEntityAction : IServerTickAction
{
    public int EntityID { get; set; }

    public DestroyEntityAction(int entityId)
    {
        this.EntityID = entityId;
    }

    public void Tick(GameServer server)
    {
        Logging.Log(LogLevel.Debug, $"Server: Destroying entity {this.EntityID}");
        server.DestroyEntity(EntityID);
        DestroyEntityPacket dep = new DestroyEntityPacket(this.EntityID);
        server.EnqueueBroadcastPacket(dep, true, false);
    }
}

public class SendChunkToClientAction : IServerTickAction
{
    public Connection Connection { get; set; }
    public int X { get; set; }
    public int Y { get; set; }

    public SendChunkToClientAction(Connection conn, int x, int y)
    {
        this.Connection = conn;
        this.X = x;
        this.Y = y;
    }

    public void Tick(GameServer server)
    {
        Logging.Log(LogLevel.Debug, $"Server: Sending chunk {this.X}, {this.Y} to client {this.Connection.RemoteEndPoint}");

        WholeChunkPacket wcp = new WholeChunkPacket()
        {
            X = this.X,
            Y = this.Y,
            Chunk = server.GetWorld().GetChunk(this.X, this.Y)
        };

        server.EnqueuePacket(wcp, this.Connection, true, false);
    }
}

public class TellClientToUnloadChunkAction : IServerTickAction
{
    public Connection Connection { get; set; }
    public int X { get; set; }
    public int Y { get; set; }

    public TellClientToUnloadChunkAction(Connection conn, int x, int y)
    {
        this.Connection = conn;
        this.X = x;
        this.Y = y;
    }

    public void Tick(GameServer server)
    {
        Logging.Log(LogLevel.Debug, $"Server: Telling client {this.Connection.RemoteEndPoint} to unload chunk {this.X}, {this.Y}");

        UnloadChunkPacket wcp = new UnloadChunkPacket()
        {
            X = this.X,
            Y = this.Y,
        };

        server.EnqueuePacket(wcp, this.Connection, true, false);
    }
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