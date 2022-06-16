using System.Text.Json.Serialization;
using GameUDPProtocol;

namespace AGame.Engine.Networking;

public class ConnectRequest : Packet
{

}

public class ConnectResponse : PacketConnectionResponse
{

}

public class ConnectReadyForMap : Packet
{

}

public class ConnectReadyForECS : Packet
{

}

public class ConnectFinished : Packet
{
    public int PlayerEntityId { get; set; }

    public ConnectFinished()
    {

    }
}

public class ClientAlive : Packet
{

}

public class QueryResponse : PacketQueryResponse
{

}