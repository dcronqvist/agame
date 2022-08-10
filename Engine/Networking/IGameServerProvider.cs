using AGame.Engine.ECSys;
using AGame.Engine.Items;
using AGame.Engine.World;
using GameUDPProtocol;

namespace AGame.Engine.Networking;

public interface IGameServerProvider
{
    /// <summary>
    /// Called when a client connects to the server.
    /// Must return the entity that the player is assigned when the client is spawned.
    /// You must assign a position etc. yourself, the server will do nothing for you except send the entity ID to the client.
    /// </summary>
    Entity OnClientConnecting(GameServer server, Connection connection, ConnectRequest connectRequest);

    void OnClientClickContainerSlot(GameServer server, Entity playerEntity, Container targetContainer, ClickContainerSlotPacket packet);
}