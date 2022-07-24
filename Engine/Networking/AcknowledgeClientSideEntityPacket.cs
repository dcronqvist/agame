using GameUDPProtocol;

namespace AGame.Engine.Networking;

public class AcknowledgeClientSideEntityPacket : Packet
{
    public ulong Hash { get; set; }
    public int EntityID { get; set; }
}