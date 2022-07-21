using AGame.Engine.Assets;
using AGame.Engine.Configuration;
using AGame.Engine.ECSys;
using AGame.Engine.ECSys.Components;
using AGame.Engine.Items;
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
        UnloadChunkPacket wcp = new UnloadChunkPacket()
        {
            X = this.X,
            Y = this.Y,
        };

        server.EnqueuePacket(wcp, this.Connection, true, false);
    }
}

public class RespondToInventoryRequestAction : IServerTickAction
{
    Connection Connection { get; set; }
    RequestInventoryContentPacket Packet { get; set; }

    public RespondToInventoryRequestAction(RequestInventoryContentPacket packet, Connection connection)
    {
        this.Packet = packet;
        this.Connection = connection;
    }

    public void Tick(GameServer server)
    {
        var inventory = server.PerformOnECS((ecs) =>
        {
            return ecs.GetEntityFromID(Packet.EntityID).GetComponent<InventoryComponent>().GetInventory();
        });

        server.EnqueuePacket(new SetInventoryContentPacket(Packet.EntityID, inventory), this.Connection, true, false);
        Logging.Log(LogLevel.Debug, $"Server: Sent InventoryContentPacket to client {this.Connection.RemoteEndPoint}");
    }
}

public class ExecuteSpawnEntityDefinitionsAction : IServerTickAction
{
    EntityDistributionDefinition Definition { get; set; }
    Chunk Chunk { get; set; }

    public ExecuteSpawnEntityDefinitionsAction(EntityDistributionDefinition definition, Chunk chunk)
    {
        this.Definition = definition;
        this.Chunk = chunk;
    }

    public void Tick(GameServer server)
    {
        server.PerformOnECS((ecs) =>
        {
            if (this.Definition.Frequency > Utilities.GetRandomFloat())
            {
                var randomVec2 = Utilities.GetRandomVector2(0, Chunk.CHUNK_SIZE, 0, Chunk.CHUNK_SIZE);
                var tileVec = new Vector2i((int)randomVec2.X + this.Chunk.X * Chunk.CHUNK_SIZE, (int)randomVec2.Y + this.Chunk.Y * Chunk.CHUNK_SIZE);
                var spawnEntities = this.Definition.GetDistribution(tileVec);

                foreach (var entity in spawnEntities)
                {
                    var e = ecs.CreateEntityFromAsset(this.Definition.EntityAsset);

                    // Assume a transform exists for this entity
                    var transform = e.GetComponent<TransformComponent>();
                    transform.Position = new CoordinateVector(entity.TileAlignedPosition.X, entity.TileAlignedPosition.Y);
                }
            }
        });
    }
}

public class HarvestEntityAction : IServerTickAction
{
    public int EntityID { get; set; }

    public HarvestEntityAction(int entityId)
    {
        this.EntityID = entityId;
    }

    public void Tick(GameServer server)
    {
        server.PerformOnECS((ecs) =>
        {
            var entity = ecs.GetEntityFromID(EntityID);
            var transform = entity.GetComponent<TransformComponent>();
            var harvestable = entity.GetComponent<HarvestableComponent>();

            foreach (var yield in harvestable.Yields)
            {
                var item = yield.Item;

                int amount = Utilities.GetRandomInt(yield.MinAmount, yield.MaxAmount);

                for (int i = 0; i < amount; i++)
                {
                    var newEntity = ecs.CreateEntityFromAsset("default.entity.ground_item");

                    var newPos = transform.Position + new CoordinateVector(Utilities.GetRandomFloat(-2f, 2f), Utilities.GetRandomFloat(-2f, 2f));

                    newEntity.GetComponent<TransformComponent>().Position = newPos;
                    newEntity.GetComponent<GroundItemComponent>().Item = item;
                }
            }

            ecs.DestroyEntity(entity.ID);
        });
    }
}