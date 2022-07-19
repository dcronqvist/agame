using GameUDPProtocol;

namespace AGame.Engine.Networking;

public class RequestInventoryContentPacket : Packet
{
    public int EntityID { get; set; }
}