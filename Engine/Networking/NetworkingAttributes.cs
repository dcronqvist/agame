namespace AGame.Engine.Networking;

public enum NBType
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

    /// <summary>
    /// Should be used for components which are sent from client to server about user input
    /// </summary>
    OnlyClientToServer,
}

public class NetworkingBehaviourAttribute : Attribute
{
    public NBType Type { get; set; }

    public NetworkingBehaviourAttribute(NBType type)
    {
        Type = type;
    }
}