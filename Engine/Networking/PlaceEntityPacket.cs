using System.Numerics;
using AGame.Engine.World;
using GameUDPProtocol;

namespace AGame.Engine.Networking;

public class PlaceEntityPacket : Packet
{
    public string EntityAssetName { get; set; }
    public Vector2i TileAlignedPosition { get; set; }
    public int ClientSideEntityID { get; set; }

    public PlaceEntityPacket()
    {

    }

    public PlaceEntityPacket(string entityAssetName, Vector2i tileAlignedPos, int clientSideEntityID)
    {
        this.EntityAssetName = entityAssetName;
        this.TileAlignedPosition = tileAlignedPos;
        this.ClientSideEntityID = clientSideEntityID;
    }
}

public class PlaceEntityAcceptPacket : Packet
{
    public int ClientSideEntityID { get; set; }
    public int ServerSideEntityID { get; set; }

    public PlaceEntityAcceptPacket()
    {

    }

    public PlaceEntityAcceptPacket(int clientSide, int serverSideEntityID)
    {
        this.ClientSideEntityID = clientSide;
        this.ServerSideEntityID = serverSideEntityID;
    }
}