using GameUDPProtocol;

namespace AGame.Engine.Networking;

// Client bound packet
public class AcknowledgeClientSideEntityPacket : Packet
{
    public ulong Hash { get; set; }
    public int EntityID { get; set; }
}

public class AcknowledgeServerSideEntityPacket : Packet
{
    public int ServerSideEntityID { get; set; }
}