namespace AGame.Engine.ECSys;

public class EntityAddedEventArgs : EventArgs
{
    public Entity Entity { get; private set; }

    public EntityAddedEventArgs(Entity entity)
    {
        Entity = entity;
    }
}