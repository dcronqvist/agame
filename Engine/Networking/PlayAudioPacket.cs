using GameUDPProtocol;

namespace AGame.Engine.Networking;

public class PlayAudioPacket : Packet
{
    public string Audio { get; set; }
    public float Pitch { get; set; }
}