using GameUDPProtocol;

namespace AGame.Engine.Networking;

public class RequestViewContainerPacket : Packet
{
    public int EntityID { get; set; }
}