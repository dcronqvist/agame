using System.Net;
using System.Numerics;
using AGame.Engine.Configuration;
using AGame.Engine.DebugTools;
using AGame.Engine.ECSys;
using AGame.Engine.ECSys.Components;
using AGame.Engine.Graphics;
using AGame.Engine.Graphics.Cameras;
using AGame.Engine.Graphics.Rendering;
using AGame.Engine.World;
using GameUDPProtocol;

namespace AGame.Engine.Networking;

public class GameClient : Client<ConnectRequest, ConnectResponse>
{
    private ThreadSafe<ECS> _ecs;
    private WorldContainer _world;
    private int _playerEntityId;
    private Entity _playerEntity;

    private ThreadSafe<Dictionary<int, Dictionary<int, float>>> _updateEntityTimes;
    private ThreadSafe<Queue<Packet>> _receivedPackets;
    private ThreadSafe<Queue<Packet>> _sentPackets;
    private bool _connectFinished;
    private ChunkAddress _previousChunk;

    public GameClient(string host, int port) : base(host, port, 1000, 10000)
    {
        this._playerEntityId = -1;

        this._updateEntityTimes = new ThreadSafe<Dictionary<int, Dictionary<int, float>>>(new Dictionary<int, Dictionary<int, float>>());

        this._receivedPackets = new ThreadSafe<Queue<Packet>>(new Queue<Packet>());
        this._sentPackets = new ThreadSafe<Queue<Packet>>(new Queue<Packet>());
        this._connectFinished = false;
        this._previousChunk = new ChunkAddress(0, 0);

        this.RegisterClientEventHandlers();
    }

    public int GetPlayerEntityID()
    {
        return this._playerEntityId;
    }

    public void Initialize(ECS ecs, WorldContainer world)
    {
        this._ecs = new ThreadSafe<ECS>(ecs);
        this._world = world;

        // ECS Events
        this._ecs.Value.ComponentChanged += HandleEntityComponentChangedInECS;
    }

    public void RegisterClientEventHandlers()
    {
        this.PacketReceived += async (sender, e) =>
        {
            //Logging.Log(LogLevel.Debug, "Packet Received: " + e.Packet.GetType().Name);


            this._receivedPackets.LockedAction((rp) =>
            {
                rp.Enqueue(e.Packet);
            });
            await Task.Delay(1000);
            this._receivedPackets.LockedAction((rp) =>
            {
                rp.Dequeue();
            });
        };

        this.PacketSent += async (sender, e) =>
        {
            //Logging.Log(LogLevel.Debug, "Packet sent: " + e.Packet.GetType().Name);

            this._sentPackets.LockedAction((rp) =>
            {
                rp.Enqueue(e.Packet);
            });
            await Task.Delay(1000);
            this._sentPackets.LockedAction((rp) =>
            {
                rp.Dequeue();
            });
        };
    }

    public void HandleDestroyEntity(int entityID)
    {
        this._ecs.LockedAction((ecs) =>
        {
            if (ecs.EntityExists(entityID))
            {
                ecs.DestroyEntity(entityID);
            }
        });
    }

    public void HandleConnectFinished(ConnectFinished packet)
    {
        this._playerEntityId = packet.PlayerEntityId;

        this._ecs.LockedAction((ecs) =>
        {
            this._playerEntity = ecs.GetEntityFromID(this._playerEntityId);
        });

        TransformComponent transform = this._playerEntity.GetComponent<TransformComponent>();
        ChunkAddress chunkPos = transform.Position.ToChunkAddress();

        this._world.MaintainChunkArea(2, 1, chunkPos.X, chunkPos.Y);

        this._connectFinished = true;
    }

    private bool ShouldEntityComponentSendUpdate(int entityId, Component component, ComponentNetworkingAttribute attribute)
    {
        // If the last time this entity's component was updates was longer than the attribute's update interval, send the update
        // And update the last update time to now

        return this._updateEntityTimes.LockedAction<bool>((times) =>
        {
            // If this entity has never sent an update, first add it to the dictionary
            if (!times.ContainsKey(entityId))
                times.Add(entityId, new Dictionary<int, float>());

            Dictionary<int, float> componentTimes = times[entityId];
            int componentId = this._ecs.Value.GetComponentID(component.GetType());

            // If this entity's component has never sent an update before, first add it to the dictionary
            if (!componentTimes.ContainsKey(componentId))
                componentTimes.Add(componentId, 0);

            float lastUpdateTime = componentTimes[componentId];

            if (attribute.MaxUpdatesPerSecond == 0 || (GameTime.TotalElapsedSeconds - lastUpdateTime) > (1f / attribute.MaxUpdatesPerSecond))
            {
                componentTimes[componentId] = GameTime.TotalElapsedSeconds;
                return true;
            }

            return false;
        });
    }

    public void HandleEntityComponentChangedInECS(object sender, EntityComponentChangedEventArgs e)
    {
        if (e.Component.HasCNType(CNType.Update, NDirection.ClientToServer))
        {
            if (this.ShouldEntityComponentSendUpdate(e.Entity.ID, e.Component, e.Attrib))
            {
                EntityUpdate eu = new EntityUpdate(e.Entity.ID, e.Component);
                UpdateEntitiesPacket uep = new UpdateEntitiesPacket(eu);
                this.EnqueuePacket(uep, true, false);
            }
        }
    }

    public void HandleECEventTrigger(TriggerECEventPacket packet)
    {
        this._ecs.LockedAction((ecs) =>
        {
            Entity entity = ecs.GetEntityFromID(packet.EntityID);

            Type compType = ecs.GetComponentType(packet.ComponentTypeID);
            Component c = entity.GetComponent(compType);
            c.TriggerComponentEvent(c.GetEventArgsType(packet.EventID), packet.EventID, packet.EventArgs);
        });
    }

    public void RegisterPacketHandlers()
    {
        this.AddPacketHandler<UpdateEntitiesPacket>((packet) =>
        {
            foreach (EntityUpdate update in packet.Updates)
            {
                this.PerformEntityUpdate(update);
            }
        });

        this.AddPacketHandler<DestroyEntityPacket>((packet) => this.HandleDestroyEntity(packet.EntityID));

        this.AddPacketHandler<ConnectFinished>((packet) => this.HandleConnectFinished(packet));

        this.AddPacketHandler<TriggerECEventPacket>((packet) => this.HandleECEventTrigger(packet));

        this.AddPacketHandler<ChunkUpdatePacket>((packet) =>
        {
            this._world.UpdateChunk(packet.X, packet.Y, packet.Chunk);
        });
    }

    public void PerformEntityUpdate(EntityUpdate update)
    {
        int entityId = update.EntityID;
        this._ecs.LockedAction((e) =>
        {
            // Check that the entity exists here on the server, if it doesn't, create it.
            // This does seem quite unlikely, that the client would have an entity that the server doesn't.
            // TODO: Look into this later.
            if (!e.EntityExists(entityId))
                e.CreateEntity(entityId);

            // Get the entity from the ECS
            Entity entity = e.GetEntityFromID(entityId);

            // Go through each component in the update and update the entity's component with the updated component in the update
            foreach (Component component in update.Components)
            {
                // If it doesn't exist on the entity, first add it.
                if (!entity.HasComponent(component.GetType()))
                    e.AddComponentToEntity(entity, component);

                // Perform component update.
                entity.GetComponent(component.GetType()).UpdateComponent(component);
            }
        });
    }

    protected override void HandleInvalidReceive(byte[] data, IPEndPoint remote, Exception e = null)
    {
        // TODO: Must do something to know that the client received an invalid packet.
    }

    public int GetRX()
    {
        return this._receivedPackets.LockedAction<int>((rp) => rp.Count);
    }

    public int GetTX()
    {
        return this._sentPackets.LockedAction<int>((rp) => rp.Count);
    }

    public async Task<bool> ConnectAsync(string clientName)
    {
        ConnectResponse response = await base.ConnectAsync(new ConnectRequest() { Name = clientName }, 100000);

        if (response is not null && response.Accepted)
        {
            this.EnqueuePacket(new ConnectReadyForECS(), true, false);

            this.RegisterPacketHandlers();

            while (!_connectFinished)
            {
                await Task.Delay(1);
            }

            this._world.Start();

            return true;
        }
        else
        {
            return false;
        }
    }

    public void Update(Camera2D camera, bool updateInput = true)
    {
        this._ecs.LockedAction((ecs) =>
        {
            ecs.InterpolateProperties();
            ecs.Update(this._world);
        });

        if (updateInput)
            this.UpdatePlayerInput(camera);

        TransformComponent transform = this._playerEntity.GetComponent<TransformComponent>();
        ChunkAddress chunkPos = transform.Position.ToChunkAddress();

        if (!chunkPos.Equals(this._previousChunk))
        {
            // Entered new chunk. Request this one.
            this._world.MaintainChunkArea(2, 1, chunkPos.X, chunkPos.Y);
            this._previousChunk = chunkPos;
        }
    }

    public void UpdatePlayerInput(Camera2D camera)
    {
        List<(GLFW.Keys, int)> keysToCheck = new List<(GLFW.Keys, int)>()
        {
            (GLFW.Keys.W, KeyboardInputComponent.KEY_W),
            (GLFW.Keys.A, KeyboardInputComponent.KEY_A),
            (GLFW.Keys.S, KeyboardInputComponent.KEY_S),
            (GLFW.Keys.D, KeyboardInputComponent.KEY_D),
            (GLFW.Keys.Space, KeyboardInputComponent.KEY_SPACE),
            (GLFW.Keys.LeftShift, KeyboardInputComponent.KEY_SHIFT),
        };

        KeyboardInputComponent playerInputComponent = this._playerEntity.GetComponent<KeyboardInputComponent>();

        foreach ((GLFW.Keys, int) key in keysToCheck)
        {
            if (Input.IsKeyDown(key.Item1))
            {
                playerInputComponent.SetKeyDown(key.Item2);
            }
            else
            {
                playerInputComponent.SetKeyUp(key.Item2);
            }
        }
    }

    public void Render()
    {
        this._world.Render();
        this._ecs.LockedAction((ecs) =>
        {
            ecs.Render(this._world);
        });
    }

    public Entity GetPlayerEntity()
    {
        return this._playerEntity;
    }
}