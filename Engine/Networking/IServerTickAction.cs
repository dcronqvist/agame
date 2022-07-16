using AGame.Engine.Configuration;
using AGame.Engine.ECSys;
using AGame.Engine.ECSys.Components;
using AGame.Engine.World;
using GameUDPProtocol;

namespace AGame.Engine.Networking;

public interface IServerTickAction
{
    void Tick(GameServer server);
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

public class PlaceEntityAction : IServerTickAction
{
    public Connection Connection { get; set; }
    public PlaceEntityPacket Packet { get; set; }

    public PlaceEntityAction(PlaceEntityPacket packet, Connection connection)
    {
        this.Packet = packet;
        this.Connection = connection;
    }

    public void Tick(GameServer server)
    {
        server.PerformOnECS((ecs) =>
        {
            Entity entity = ecs.CreateEntityFromAsset(this.Packet.EntityAssetName);
            // Assume entity to have a Transform component

            var transform = entity.GetComponent<TransformComponent>();
            transform.Position = new CoordinateVector(this.Packet.TileAlignedPosition.X, this.Packet.TileAlignedPosition.Y);

            server.EnqueuePacket(new PlaceEntityAcceptPacket(Packet.ClientSideEntityID, entity.ID), this.Connection, true, false);
        });
    }
}