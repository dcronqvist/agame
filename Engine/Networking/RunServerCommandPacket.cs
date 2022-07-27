using GameUDPProtocol;

namespace AGame.Engine.Networking;

public class RunServerCommandPacket : Packet
{
    public string LineToRun { get; set; }
}