using System.Text.Json.Serialization;
using GameUDPProtocol;

namespace AGame.Engine.Networking;

public class MapDataFinishedPacket : Packet
{
    [JsonConstructor]
    public MapDataFinishedPacket()
    {

    }
}