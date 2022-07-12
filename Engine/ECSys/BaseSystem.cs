using AGame.Engine.Networking;
using AGame.Engine.World;

namespace AGame.Engine.ECSys;

public abstract class BaseSystem
{
    public List<Type> ComponentTypes { get; set; }
    public ECS ParentECS { get; set; }
    public GameClient GameClient { get; set; }
    public GameServer GameServer { get; set; }
    public SystemRunner Runner { get; set; }

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

    public virtual void BeforeUpdate(List<Entity> entities, WorldContainer gameWorld)
    {

    }

    public virtual void Update(List<Entity> entities, WorldContainer gameWorld, float deltaTime)
    {

    }

    public virtual void AfterUpdate(List<Entity> entities, WorldContainer gameWorld)
    {

    }

    public virtual void Render(List<Entity> entity, WorldContainer gameWorld)
    {

    }
}

[Flags]
public enum SystemRunner
{
    Client = 1 << 0,
    Server = 1 << 1,
}

public class SystemRunsOnAttribute : Attribute
{
    public SystemRunner RunsOn { get; set; }

    public SystemRunsOnAttribute(SystemRunner runsOn)
    {
        RunsOn = runsOn;
    }
}