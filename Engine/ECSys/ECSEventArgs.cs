namespace AGame.Engine.ECSys;

public class EntityComponentChangedEventArgs : EventArgs
{
    public Entity Entity { get; set; }
    public Component Component { get; set; }

    public EntityComponentChangedEventArgs(Entity entity, Component component)
    {
        Entity = entity;
        Component = component;
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