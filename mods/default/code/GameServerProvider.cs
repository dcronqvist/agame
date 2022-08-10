using System.Numerics;
using AGame.Engine.Assets.Scripting;
using AGame.Engine.Configuration;
using AGame.Engine.ECSys;
using AGame.Engine.Items;
using AGame.Engine.Networking;
using GameUDPProtocol;

namespace DefaultMod;

[ScriptClass(Name = "game_server_provider")]
public class GameServerProvider : IGameServerProvider
{
    public Container GetContainerForEntity(GameServer server, Entity entity)
    {
        return entity.GetComponent<ContainerComponent>().GetContainer();
    }

    public void OnClientClickContainerSlot(GameServer server, Entity playerEntity, Container targetContainer, ClickContainerSlotPacket packet)
    {
        var mouseSlot = new ContainerSlot(Vector2.Zero);

        var playerState = playerEntity.GetComponent<PlayerStateComponent>();

        mouseSlot.Item = playerState.MouseSlot.Item.Instance;
        mouseSlot.Count = playerState.MouseSlot.ItemCount;

        targetContainer.ClickSlot(packet.SlotID, ref mouseSlot);

        playerState.MouseSlot = mouseSlot.ToSlotInfo(0);
    }

    public Entity OnClientConnecting(GameServer server, Connection connection, ConnectRequest connectRequest)
    {
        Logging.Log(LogLevel.Debug, "DefaultMod says hello!");

        Entity entity = server.PerformOnECS((ecs) =>
        {
            Entity entity = ecs.CreateEntityFromAsset("default.entity.player");
            entity.GetComponent<CharacterComponent>().Name = connectRequest.Name;

            return entity;
        });

        return entity;
    }
}