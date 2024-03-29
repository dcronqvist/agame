using System.CommandLine;
using System.Linq;
using AGame.Engine.Assets;
using AGame.Engine.Configuration;
using AGame.Engine.ECSys;
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
                    entity.OnCreate(e);
                }
            }
        });
    }
}

public class PerformServerCommandAction : IServerTickAction
{
    public Entity CallingEntity { get; set; }
    public string Line { get; set; }

    public PerformServerCommandAction(string line, Entity callingEntity)
    {
        this.Line = line;
        this.CallingEntity = callingEntity;
    }

    public void Tick(GameServer server)
    {
        var split = this.Line.Split(' ');
        var command = split[0];

        server.PerformOnECS((ecs) =>
        {
            var c = server.GetCommandByAlias(command);
            c.Initialize(server);

            c.GetConfiguration(CallingEntity, ecs).Invoke(split.Skip(1).ToArray());
        });
    }
}