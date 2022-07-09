using AGame.Engine.Configuration;

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