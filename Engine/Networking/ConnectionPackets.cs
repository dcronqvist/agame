using System.Text.Json.Serialization;
using GameUDPProtocol;

namespace AGame.Engine.Networking;

public class ConnectRequest : Packet
{
    public string Name { get; set; }
}

// This packet should contain the configuration that the client should adhere to during the game.
public class ConnectResponse : PacketConnectionResponse
{
    public int ServerTickSpeed { get; set; }
    public int PlayerEntityID { get; set; }
}

public class ConnectReadyForData : Packet { }

public class QueryResponse : PacketQueryResponse
{

}
