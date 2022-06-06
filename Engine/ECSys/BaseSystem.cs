namespace AGame.Engine.ECSys;

public abstract class BaseSystem
{
    public List<Type> ComponentTypes { get; set; }

    public BaseSystem()
    {
        this.ComponentTypes = new List<Type>();
    }

    protected void RegisterComponentType<T>() where T : Component
    {
        if (ComponentTypes == null)
        {
            ComponentTypes = new List<Type>();
        }
        ComponentTypes.Add(typeof(T));
    }

    public abstract void Initialize();

    public virtual void Update(List<Entity> entities)
    {

    }

    public virtual void Render(List<Entity> entity)
    {

    }
}