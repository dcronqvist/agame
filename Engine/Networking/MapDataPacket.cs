using System.Text.Json.Serialization;
using GameUDPProtocol;

namespace AGame.Engine.Networking;

public class MapDataPacket : Packet
{
    public int[] TileIDs { get; set; }

    [JsonConstructor]
    public MapDataPacket()
    {

    }

    public MapDataPacket(int[] tileIds)
    {
        this.TileIDs = tileIds;
    }
}