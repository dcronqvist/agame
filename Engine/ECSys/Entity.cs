using AGame.Engine.Networking;
using AGame.Engine.World;

namespace AGame.Engine.ECSys;

public class Entity
{
    public int ID { get; set; }

    public List<Component> Components { get; set; }

    public Entity(int id)
    {
        this.ID = id;
        this.Components = new List<Component>();
    }

    private void AddComponent(Component c)
    {
        ECS.Instance.Value.AddComponentToEntity(this, c);
    }

    public T GetComponent<T>() where T : Component
    {
        return this.Components.Find(c => c.GetType() == typeof(T)) as T;
    }

    public Component GetComponent(Type type)
    {
        return this.Components.Find(c => c.GetType() == type);
    }

    public bool HasComponent<T>() where T : Component
    {
        return this.Components.Find(c => c.GetType() == typeof(T)) != null;
    }

    public bool HasComponent(Type type)
    {
        return this.Components.Find(c => c.GetType() == type) != null;
    }

    public bool HasAllComponents(Type[] types)
    {
        foreach (var type in types)
        {
            if (!this.HasComponent(type))
            {
                return false;
            }
        }

        return true;
    }

    public Entity Clone()
    {
        return new Entity(this.ID)
        {
            Components = this.Components.Select(c => c.Clone()).ToList()
        };
    }

    public void ApplyInput(UserCommand command, WorldContainer world, ECS ecs)
    {
        foreach (var c in this.Components)
        {
            c.ApplyInput(this, command, world, ecs);
        }

        command.HasBeenRun = true;
    }

    public void InterpolateComponents(float interpolationTime)
    {
        foreach (var c in this.Components)
        {
            c.InterpolateComponent(interpolationTime);
        }
    }
}