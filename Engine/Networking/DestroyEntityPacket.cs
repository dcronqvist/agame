using GameUDPProtocol;

namespace AGame.Engine.Networking;

public class DestroyEntityPacket : Packet
{
    public int EntityID { get; set; }

    public DestroyEntityPacket()
    {

    }

    public DestroyEntityPacket(int entityId)
    {
        this.EntityID = entityId;
    }
}