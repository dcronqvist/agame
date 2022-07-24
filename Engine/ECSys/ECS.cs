using System;
using System.Collections.Generic;
using System.Linq;
using AGame.Engine.Assets;
using AGame.Engine.Assets.Scripting;
using AGame.Engine.DebugTools;
using AGame.Engine.ECSys.Systems;
using AGame.Engine.Networking;
using AGame.Engine.World;
using GameUDPProtocol;

namespace AGame.Engine.ECSys;

public class ECS
{
    // Helper stuff
    private int _nextEntityID = 0;
    private SystemRunner _runner;
    private GameClient _client;
    private GameServer _server;

    // All entities & systems
    private List<Entity> _entities = new List<Entity>();
    private List<Entity> _entitiesToDestroy = new List<Entity>();
    private List<BaseSystem> _systems = new List<BaseSystem>();
    private Dictionary<BaseSystem, List<Entity>> _systemEntities = new Dictionary<BaseSystem, List<Entity>>();
    private List<BaseSystem> _systemsToUpdate = new List<BaseSystem>();

    // Component stuff
    private Dictionary<string, Type> _componentTypes = new Dictionary<string, Type>();
    private Dictionary<Type, int> _componentTypeIDs = new Dictionary<Type, int>();
    public event EventHandler<EntityComponentChangedEventArgs> ComponentChanged;
    public float InterpolationTime { get; private set; }

    // Events
    public event EventHandler<EntityAddedEventArgs> EntityAdded;
    public event EventHandler<EntityDestroyedEventArgs> EntityDestroyed;

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

    public void Initialize(SystemRunner runner, GameClient gameClient = null, GameServer gameServer = null, List<Entity> entities = null)
    {
        this._runner = runner;
        this._entities = entities ?? new List<Entity>();
        this._client = gameClient;
        this._server = gameServer;
        this._nextEntityID = this._entities.Count > 0 ? this._entities.Max(x => x.ID) + 1 : 0;
        this.InterpolationTime = 0f;

        // Register all component types
        this.RegisterComponentTypes();

        // Register system
        this.RegisterAllSystems();
    }

    public bool IsRunner(SystemRunner runner)
    {
        return this._runner == runner;
    }

    public void SetInterpolationTime(float time)
    {
        this.InterpolationTime = time;
    }

    public ECSSnapshot GetSnapshot()
    {
        ECSSnapshot snapshot = new ECSSnapshot();
        snapshot.Entities = this._entities.Select(x => x.Clone()).ToList();
        return snapshot;
    }

    public void RestoreSnapshot(ECSSnapshot snapshot)
    {
        this._entities = snapshot.Entities;
        this._nextEntityID = this._entities.Count > 0 ? this._entities.Max(x => x.ID) + 1 : 0;
    }

    public void RegisterAllSystems()
    {
        this._systems.Clear();
        //List<Type> systems = ScriptingManager.GetAllTypesWithBaseType<BaseSystem>().ToList();
        List<Type> systems = Utilities.FindDerivedTypes(typeof(BaseSystem)).Where(x => x != typeof(BaseSystem)).ToList();

        foreach (Type systemType in systems)
        {
            RegisterSystem(systemType);
        }
    }

    public void RegisterComponentTypes()
    {
        this._componentTypes.Clear();
        this._componentTypeIDs.Clear();

        Type[] componentTypes = Utilities.FindDerivedTypes(typeof(Component)).Where(x => x != typeof(Component)).ToArray();
        componentTypes = componentTypes.OrderBy(t => t.Name).DistinctBy(x => x.Name).ToArray();

        for (int i = 0; i < componentTypes.Length; i++)
        {
            string typeName = componentTypes[i].Name.Replace("Component", "");
            _componentTypes.Add(typeName, componentTypes[i]);
            _componentTypeIDs.Add(componentTypes[i], i);
        }
    }

    public Type GetComponentType(string id)
    {
        return _componentTypes[id];
    }

    public Type GetComponentType(int id)
    {
        return _componentTypeIDs.FirstOrDefault(x => x.Value == id).Key;
    }

    public int GetComponentID(Type type)
    {
        return _componentTypeIDs[type];
    }

    public int GetComponentID(string name)
    {
        return _componentTypeIDs[GetComponentType(name)];
    }

    private void RegisterSystem(Type type)
    {
        if (_systems == null)
        {
            _systems = new List<BaseSystem>();
        }

        SystemRunsOnAttribute sroa = type.GetCustomAttributes(typeof(SystemRunsOnAttribute), false).FirstOrDefault() as SystemRunsOnAttribute;

        BaseSystem system = (BaseSystem)Activator.CreateInstance(type);
        system.ParentECS = this;
        system.GameClient = this._client;
        system.GameServer = this._server;
        system.Runner = this._runner;
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

    private void RegisterSystem<T>() where T : BaseSystem, new()
    {
        if (_systems == null)
        {
            _systems = new List<BaseSystem>();
        }

        SystemRunsOnAttribute sroa = typeof(T).GetCustomAttributes(typeof(SystemRunsOnAttribute), false).FirstOrDefault() as SystemRunsOnAttribute;

        T system = new T();
        system.ParentECS = this;
        system.GameClient = this._client;
        system.GameServer = this._server;
        system.Runner = this._runner;
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
        EntityDescription ed = ModManager.GetAsset<EntityDescription>(assetName);

        if (ed.Extends is not null)
        {
            Entity extend = CreateEntityFromAsset(ed.Extends);

            foreach (Component c in ed.Components)
            {
                if (extend.HasComponent(c.GetType()))
                {
                    extend.GetComponent(c.GetType()).UpdateComponent(c.Clone());
                }
                else
                {
                    AddComponentToEntity(extend, c.Clone());
                }
            }

            return extend;
        }
        else
        {
            Entity entity = new Entity(_nextEntityID++);

            foreach (Component c in ed.Components)
            {
                AddComponentToEntity(entity, c.Clone());
            }

            this.AddEntity(entity);
            return entity;
        }
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
        if (c.GetCNAttrib().UpdateTriggersNetworkUpdate)
        {
            c.PropertyChanged += (sender, e) =>
            {
                this.ComponentChanged?.Invoke(this, new EntityComponentChangedEventArgs(entity, c));
            };
        }

        if (c.GetCNAttrib().CreateTriggersNetworkUpdate)
        {
            this.ComponentChanged?.Invoke(this, new EntityComponentChangedEventArgs(entity, c));
        }
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
        _entitiesToDestroy.Add(entity);
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

    public void Update(WorldContainer gameWorld, float deltaTime)
    {
        bool destroying = this._entitiesToDestroy.Count > 0;
        foreach (Entity e in this._entitiesToDestroy)
        {
            this._entities.Remove(e);
            this.EntityDestroyed?.Invoke(this, new EntityDestroyedEventArgs(e));
        }
        if (destroying)
        {
            this.RecalculateSystemEntities();
            this._entitiesToDestroy.Clear();
        }

        foreach (var system in _systemsToUpdate)
        {
            system.BeforeUpdate(_systemEntities[system], gameWorld);
        }

        foreach (var system in _systemsToUpdate)
        {
            system.Update(_systemEntities[system], gameWorld, deltaTime);
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

    public List<Entity> GetAllEntities(Func<Entity, bool> predicate)
    {
        return _entities.Where(e => predicate(e)).ToList();
    }
}