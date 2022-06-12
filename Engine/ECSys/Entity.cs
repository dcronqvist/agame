using AGame.Engine.Networking;

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

    public Component[] GetComponentsWithCNType(CNType type, NDirection direction)
    {
        return this.Components.FindAll(c => c.HasCNType(type, direction)).ToArray();
    }
}