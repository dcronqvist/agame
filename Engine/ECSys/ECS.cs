using System.Linq;
using AGame.Engine.Assets;
using AGame.Engine.ECSys.Systems;
using GameUDPProtocol;

namespace AGame.Engine.ECSys;

public class ECS
{
    // Helper stuff
    private int _nextEntityID = 0;

    // All entities & systems
    private List<Entity> _entities = new List<Entity>();
    private List<BaseSystem> _systems = new List<BaseSystem>();
    private Dictionary<BaseSystem, List<Entity>> _systemEntities = new Dictionary<BaseSystem, List<Entity>>();

    // Component stuff
    private Dictionary<string, Type> _componentTypes = new Dictionary<string, Type>();

    // Singleton for client
    private static ThreadSafe<ECS> _instance;
    public static ThreadSafe<ECS> Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = new ThreadSafe<ECS>(new ECS());
            }

            return _instance;
        }
    }

    public void Initialize()
    {
        // Register all component types
        Type[] componentTypes = Utilities.FindDerivedTypes(typeof(Component)).ToArray();
        componentTypes = componentTypes.OrderBy(t => t.Name).ToArray();

        for (int i = 0; i < componentTypes.Length; i++)
        {
            _componentTypes.Add(componentTypes[i].Name.Replace("Component", ""), componentTypes[i]);
        }

        RegisterSystem<TestSystem>();
    }

    public Type GetComponentType(string id)
    {
        return _componentTypes[id];
    }

    private void RegisterSystem<T>() where T : BaseSystem, new()
    {
        if (_systems == null)
        {
            _systems = new List<BaseSystem>();
        }
        T system = new T();
        system.Initialize();
        _systems.Add(system);

        RecalculateSystemEntities();
    }

    public Entity CreateEntity()
    {
        Entity entity = new Entity(_nextEntityID++);
        _entities.Add(entity);
        return entity;
    }

    public Entity CreateEntity(int id)
    {
        Entity entity = new Entity(id);
        _entities.Add(entity);
        return entity;
    }

    public T CreateEntity<T>() where T : Entity
    {
        T entity = Activator.CreateInstance(typeof(T), new object[] { _nextEntityID++ }) as T;
        _entities.Add(entity);
        return entity;
    }

    public void AddEntity(Entity entity)
    {
        _entities.Add(entity);
        RecalculateSystemEntities();
    }

    public Entity CreateEntityFromAsset(string assetName)
    {
        EntityDescription ed = AssetManager.GetAsset<EntityDescription>(assetName);

        Entity entity = new Entity(_nextEntityID++);
        _entities.Add(entity);

        foreach (Component c in ed.Components)
        {
            entity.Components.Add(c.Clone());
        }

        RecalculateSystemEntities();
        return entity;
    }

    public bool EntityExists(int id)
    {
        return _entities.Any(e => e.ID == id);
    }

    public Entity GetEntityFromID(int id)
    {
        return _entities.Find(e => e.ID == id);
    }

    public void AddComponentToEntity(Entity entity, Component c)
    {
        entity.Components.Add(c);
        RecalculateSystemEntities();
    }

    public void RemoveComponentFromEntity(Entity entity, Type c)
    {
        entity.Components.Remove(entity.Components.Find(e => e.GetType() == c));
        RecalculateSystemEntities();
    }

    public void RecalculateSystemEntities()
    {
        _systemEntities.Clear();
        foreach (var system in _systems)
        {
            List<Entity> entities = _entities.Where(e => e.HasAllComponents(system.ComponentTypes.ToArray())).ToList();
            _systemEntities.Add(system, entities);
        }
    }

    public void InterpolateProperties()
    {
        foreach (Entity e in this.GetAllEntities())
        {
            foreach (Component c in e.Components)
            {
                c.InterpolateProperties();
            }
        }
    }

    public void Update()
    {
        foreach (var system in _systems)
        {
            system.Update(_systemEntities[system]);
        }
    }

    public void Render()
    {
        foreach (var system in _systems)
        {
            system.Render(_systemEntities[system]);
        }
    }

    public List<Entity> GetAllEntities()
    {
        return _entities;
    }
}