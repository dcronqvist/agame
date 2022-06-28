namespace AGame.Engine.Networking;

[Flags]
public enum CNType
{
    /// <summary>
    /// Should be used for components which hold information that should be unreliably broadcasted to all clients.
    /// If not specified, the default is this type.
    /// </summary>
    Snapshot = 1 << 0,

    /// <summary>
    /// Should be used for components which hold information that should be reliably broadcasted to all clients,
    /// and only when a component's property has changed.
    /// </summary>
    Update = 1 << 1,
}

[Flags]
public enum NDirection
{
    ClientToServer = 1 << 0,
    ServerToClient = 1 << 1,
}

[AttributeUsage(AttributeTargets.Class)]
public class ComponentNetworkingAttribute : Attribute
{
    public CNType Type { get; set; }
    public NDirection Direction { get; set; }
    public bool IsReliable { get; set; }
    public int MaxUpdatesPerSecond { get; set; }

    /// <summary>
    /// Attribute to put on component classes to specify how they should be sent over the network.
    /// </summary>
    /// <param name="type">Snapshot to broadcast to receiver continuously, Update to only send when a property inside the component updates.</param>
    /// <param name="direction">ClientToServer to send to the server, ServerToClient to send to the client.</param>
    public ComponentNetworkingAttribute(CNType type, NDirection direction)
    {
        this.Type = type;
        this.Direction = direction;
        this.IsReliable = true;
        this.MaxUpdatesPerSecond = 0;
    }

    public bool Has(CNType type, NDirection direction)
    {
        return (type.HasFlag(this.Type) || this.Type.HasFlag(type)) && (direction.HasFlag(this.Direction) || this.Direction.HasFlag(direction));
    }
}