namespace AGame.Engine.Networking;

public class GameServerConfiguration
{
    public int Port { get; set; }
    public int MaxConnections { get; set; }
    public bool OnlyAllowLocalConnections { get; set; }
    public int TickRate { get; set; }

    public bool Validate()
    {
        return MaxConnections > 0 && TickRate > 0;
    }

    public GameServerConfiguration SetPort(int port)
    {
        Port = port;
        return this;
    }

    public GameServerConfiguration SetMaxConnections(int maxConnections)
    {
        MaxConnections = maxConnections;
        return this;
    }

    public GameServerConfiguration SetOnlyAllowLocalConnections(bool onlyAllowLocalConnections)
    {
        OnlyAllowLocalConnections = onlyAllowLocalConnections;
        return this;
    }

    public GameServerConfiguration SetTickRate(int tickRate)
    {
        TickRate = tickRate;
        return this;
    }
}