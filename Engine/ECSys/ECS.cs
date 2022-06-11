using System.Linq;
using AGame.Engine.Assets;
using AGame.Engine.DebugTools;
using AGame.Engine.ECSys.Systems;
using AGame.Engine.Networking;
using AGame.Engine.World;
using GameUDPProtocol;

namespace AGame.Engine.ECSys;

public delegate void ComponentChangedEventHandler(Entity entity, Component component, NBType behaviourType);

public class ECS
{
    // Helper stuff
    private int _nextEntityID = 0;
    private SystemRunner _runner;

    // All entities & systems
    private List<Entity> _entities = new List<Entity>();
    private List<BaseSystem> _systems = new List<BaseSystem>();
    private Dictionary<BaseSystem, List<Entity>> _systemEntities = new Dictionary<BaseSystem, List<Entity>>();
    private List<BaseSystem> _systemsToUpdate = new List<BaseSystem>();

    // Component stuff
    private Dictionary<string, Type> _componentTypes = new Dictionary<string, Type>();
    public event ComponentChangedEventHandler ComponentChanged;

    // Events
    public event EventHandler<EntityAddedEventArgs> EntityAdded;

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

    public void Initialize(SystemRunner runner)
    {
        // Register all component types
        this._runner = runner;
        Type[] componentTypes = Utilities.FindDerivedTypes(typeof(Component)).ToArray();
        componentTypes = componentTypes.OrderBy(t => t.Name).ToArray();

        for (int i = 0; i < componentTypes.Length; i++)
        {
            _componentTypes.Add(componentTypes[i].Name.Replace("Component", ""), componentTypes[i]);
        }

        // Register system
        RegisterSystem<TestSystem>();
        RegisterSystem<PlayerInputUpdateSystem>();
        RegisterSystem<WeirdSystem>();
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

        SystemRunsOnAttribute sroa = typeof(T).GetCustomAttributes(typeof(SystemRunsOnAttribute), false).FirstOrDefault() as SystemRunsOnAttribute;

        T system = new T();
        system.ParentECS = this;
        system.Initialize();
        _systems.Add(system);

        if (sroa is not null)
        {
            if (sroa.RunsOn.HasFlag(this._runner))
            {
                this._systemsToUpdate.Add(system);
            }
        }

        RecalculateSystemEntities();
    }

    public Entity CreateEntity()
    {
        Entity entity = new Entity(_nextEntityID++);
        this.AddEntity(entity);
        return entity;
    }

    public Entity CreateEntity(int id)
    {
        Entity entity = new Entity(id);
        this.AddEntity(entity);
        return entity;
    }

    public T CreateEntity<T>() where T : Entity
    {
        T entity = Activator.CreateInstance(typeof(T), new object[] { _nextEntityID++ }) as T;
        this.AddEntity(entity);
        return entity;
    }

    public void AddEntity(Entity entity)
    {
        _entities.Add(entity);
        RecalculateSystemEntities();

        this.EntityAdded?.Invoke(this, new EntityAddedEventArgs(entity));
    }

    public Entity CreateEntityFromAsset(string assetName)
    {
        EntityDescription ed = AssetManager.GetAsset<EntityDescription>(assetName);

        Entity entity = new Entity(_nextEntityID++);

        foreach (Component c in ed.Components)
        {
            AddComponentToEntity(entity, c.Clone());
        }

        this.AddEntity(entity);
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
        c.PropertyChanged += (sender, e) =>
        {
            NetworkingBehaviourAttribute nba = c.GetType().GetCustomAttributes(typeof(NetworkingBehaviourAttribute), false).FirstOrDefault() as NetworkingBehaviourAttribute;

            if (nba is null)
            {
                ComponentChanged?.Invoke(entity, c, NBType.Snapshot);
            }
            else
            {
                ComponentChanged?.Invoke(entity, c, nba.Type);
            }
        };

        entity.Components.Add(c);
        RecalculateSystemEntities();
    }

    public void RemoveComponentFromEntity(Entity entity, Type c)
    {
        entity.Components.Remove(entity.Components.Find(e => e.GetType() == c));
        RecalculateSystemEntities();
    }

    public void DestroyEntity(int id)
    {
        Entity entity = _entities.Find(e => e.ID == id);
        _entities.Remove(entity);
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

    public void Update(WorldContainer gameWorld)
    {
        foreach (var system in _systemsToUpdate)
        {
            system.BeforeUpdate(_systemEntities[system], gameWorld);
        }

        foreach (var system in _systemsToUpdate)
        {
            system.Update(_systemEntities[system], gameWorld);
        }

        foreach (var system in _systemsToUpdate)
        {
            system.AfterUpdate(_systemEntities[system], gameWorld);
        }
    }

    public void Render(WorldContainer gameWorld)
    {
        foreach (var system in _systems)
        {
            system.Render(_systemEntities[system], gameWorld);
        }
    }

    public List<Entity> GetAllEntities()
    {
        return _entities;
    }
}