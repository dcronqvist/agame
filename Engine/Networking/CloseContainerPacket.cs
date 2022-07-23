using GameUDPProtocol;

namespace AGame.Engine.Networking;

public class CloseContainerPacket : Packet
{
    public int EntityID { get; set; }
}