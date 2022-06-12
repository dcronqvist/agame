namespace AGame.Engine.Networking;

public enum CNType
{
    /// <summary>
    /// Should be used for components which hold information that should be unreliably broadcasted to all clients.
    /// If not specified, the default is this type.
    /// </summary>
    Snapshot,

    /// <summary>
    /// Should be used for components which hold information that should be reliably broadcasted to all clients,
    /// and only when a component's property has changed.
    /// </summary>
    Update,
}

public enum NDirection
{
    ClientToServer,
    ServerToClient
}

[AttributeUsage(AttributeTargets.Class)]
public class ComponentNetworkingAttribute : Attribute
{
    public CNType Type { get; set; }
    public NDirection Direction { get; set; }

    public ComponentNetworkingAttribute(CNType type, NDirection direction)
    {
        this.Type = type;
        this.Direction = direction;
    }

    public bool Has(CNType type, NDirection direction)
    {
        return this.Type == type && this.Direction == direction;
    }
}