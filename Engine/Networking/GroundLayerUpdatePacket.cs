using GameUDPProtocol;

namespace AGame.Engine.Networking;

public class GroundLayerUpdatePacket : Packet
{
    public int X { get; set; }
    public int Y { get; set; }
    public int TileId { get; set; }

    public GroundLayerUpdatePacket()
    {

    }

    public GroundLayerUpdatePacket(int x, int y, int tileId)
    {
        X = x;
        Y = y;
        TileId = tileId;
    }
}