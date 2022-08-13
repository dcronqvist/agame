using System;
using AGame.Engine.Networking;

namespace AGame.Engine.ECSys;

public class EntityComponentChangedEventArgs : EventArgs
{
    public Entity Entity { get; set; }
    public Component Component { get; set; }
    public ComponentNetworkingAttribute Attrib { get; set; }
    public string PropertyChanged { get; set; }

    public EntityComponentChangedEventArgs(Entity entity, Component component, string changedProperty)
    {
        Entity = entity;
        Component = component;
        this.Attrib = component.GetCNAttrib();
        this.PropertyChanged = changedProperty;
    }
}

public class EntityAddedEventArgs : EventArgs
{
    public Entity Entity { get; private set; }

    public EntityAddedEventArgs(Entity entity)
    {
        Entity = entity;
    }
}

public class EntityDestroyedEventArgs : EventArgs
{
    public Entity Entity { get; private set; }

    public EntityDestroyedEventArgs(Entity entity)
    {
        Entity = entity;
    }
}