using System.Diagnostics;
using System.Net;
using System.Numerics;
using System.Reflection;
using AGame.Engine.DebugTools;
using AGame.Engine.ECSys;
using AGame.Engine.ECSys.Components;
using AGame.Engine.World;
using GameUDPProtocol;
using GameUDPProtocol.ServerEventArgs;

namespace AGame.Engine.Networking;

public class GameServerConfiguration
{
    public int Port { get; set; }
    public int MaxClients { get; set; }
    public bool OnlyAllowLocalConnections { get; set; }
}

public class GameServer : Server<ConnectRequest, ConnectResponse, QueryResponse>
{
    private ThreadSafe<ECS> _ecs;
    private WorldContainer _world;
    private WorldMetaData _worldMetaData;
    private GameServerConfiguration _config;

    private Dictionary<Connection, int> _playerIds;
    private Dictionary<Connection, string> _playerNames;
    private Dictionary<Connection, bool> _playerFullyConnected;
    private ThreadSafe<Dictionary<int, Dictionary<int, float>>> _updateEntityTimes;

    public GameServer(ECS ecs, WorldContainer world, WorldMetaData worldMeta, GameServerConfiguration config) : base(config.Port, 1000, 10000)
    {
        this._ecs = new ThreadSafe<ECS>(ecs);
        this._world = world;
        this._worldMetaData = worldMeta;
        this._config = config;

        this._playerIds = new Dictionary<Connection, int>();
        this._playerNames = new Dictionary<Connection, string>();
        this._playerFullyConnected = new Dictionary<Connection, bool>();
        this._updateEntityTimes = new ThreadSafe<Dictionary<int, Dictionary<int, float>>>(new Dictionary<int, Dictionary<int, float>>());

        this.RegisterServerEventHandlers();
        this.RegisterPacketHandlers();
    }

    public bool IsRequesterAllowedToConnect(ConnectRequest request, IPEndPoint remote)
    {
        if (this._config.OnlyAllowLocalConnections)
        {
            return remote.Address.ToString() == "127.0.0.1" && this._connections.Value.Count < this._config.MaxClients;
        }

        return this._connections.Value.Count < this._config.MaxClients;
    }

    public void HandleClientDisconnected(Connection connection)
    {
        this._ecs.LockedAction((ecs) =>
        {
            if (this._playerIds.ContainsKey(connection))
            {
                int entityId = this._playerIds[connection];
                Entity playerEntity = ecs.GetEntityFromID(entityId);

                string playerName = this._playerNames[connection];
                PlayerInfo pi = this._worldMetaData.GetPlayerInfo(playerName);

                // Save relevant data about the player
                pi.Position = playerEntity.GetComponent<TransformComponent>().Position;

                // Update the player info in the world meta data
                this._worldMetaData.UpdatePlayerInfo(playerName, pi);

                // Destroy the player's entity and then remove the player from the player ids and player names
                ecs.DestroyEntity(entityId);
                this._playerIds.Remove(connection);
                this._playerNames.Remove(connection);
            }
        });
    }

    public void RegisterServerEventHandlers()
    {
        // Server Events
        this.ConnectionRequested += (sender, e) =>
        {
            if (this.IsRequesterAllowedToConnect(e.RequestPacket, e.Requester))
            {
                e.Accept(new ConnectResponse());
            }
            else
            {
                e.Reject(new ConnectResponse(), "Server is full");
            }
        };

        this.ConnectionAccepted += (sender, e) =>
        {
            ConnectRequest cr = (ConnectRequest)e.ConnectRequestPacket;
            this._playerNames.Add(e.Connection, cr.Name);
            this._playerFullyConnected.Add(e.Connection, false);
        };

        this.ServerQueryReceived += (sender, e) => e.RespondWith(new QueryResponse());
        this.ClientDisconnected += (sender, e) => HandleClientDisconnected(e.Connection);
        this.ClientTimedOut += (sender, e) => HandleClientDisconnected(e.Connection);

        // ECS Events
        this._ecs.Value.ComponentChanged += HandleEntityComponentChangedInECS;
        this._ecs.Value.EntityAdded += HandleEntityAddedToECS;
        this._ecs.Value.EntityDestroyed += HandleEntityDestroyedInECS;

        // World Events
        this._world.ChunkUpdated += HandleWorldChunkUpdates;
    }

    public void SendEntityUpdatePacketsWithCNType(CNType cnType, NDirection direction, Action<List<UpdateEntitiesPacket>> sendAction)
    {
        _ecs.LockedAction((ecs) =>
        {
            List<UpdateEntitiesPacket> packets = Utilities.CreateEntityUpdatePackets(cnType, direction, ecs.GetAllEntities().ToArray());
            sendAction(packets);
        });
    }

    public void BroadcastSnapshotEntityUpdatesToAllClients()
    {
        this.SendEntityUpdatePacketsWithCNType(CNType.Snapshot, NDirection.ServerToClient, (packets) =>
        {
            this._connections.LockedAction((conns) =>
            {
                foreach (UpdateEntitiesPacket packet in packets)
                {
                    foreach (var conn in conns.Where(x => this._playerFullyConnected[x]))
                    {
                        this.EnqueuePacket(packet, conn, false, false);
                    }
                }
            });
        });
    }

    public void SendCompleteECSToClient(Connection connection)
    {
        this.SendEntityUpdatePacketsWithCNType(CNType.Snapshot | CNType.Update, NDirection.ServerToClient | NDirection.ClientToServer, (packets) =>
        {
            foreach (UpdateEntitiesPacket packet in packets)
            {
                this.EnqueuePacket(packet, connection, true, false);
            }
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
                    e.AddComponentToEntity(entity, component.Clone());

                // Perform component update.
                entity.GetComponent(component.GetType()).UpdateComponent(component);
            }
        });
    }

    public void RegisterPacketHandlers()
    {
        // This packet is part of the connection protocol/sequence, and is the final step towards letting the player connect
        this.AddPacketHandler<ConnectReadyForECS>(async (packet, connection) =>
        {
            Entity newPlayer = null;
            _ecs.LockedAction((ecs) =>
            {
                // Create a new entity for the player
                newPlayer = ecs.CreateEntityFromAsset("entity_player");
            });

            // Assign the new entity's ID to this connection
            this._playerIds.Add(connection, newPlayer.ID);

            // Get the player's name from the connection
            string playerName = this._playerNames[connection];

            // Assign the player's position from the world's meta data
            newPlayer.GetComponent<TransformComponent>().Position = this._worldMetaData.GetPlayerInfo(playerName).Position;
            // Assign the player's name to the new entity
            newPlayer.GetComponent<PlayerInfoComponent>().Name = playerName;

            // Send the entire ECS to the client so that they can reconstruct the world state
            this.SendCompleteECSToClient(connection);

            await Task.Delay(1000);

            // Tell the client that the ECS has been sent and that they can start playing
            // Also tell the client their entity ID so they can control the correct character
            this.EnqueuePacket(new ConnectFinished() { PlayerEntityId = newPlayer.ID }, connection, true, true);
            this._playerFullyConnected[connection] = true;
        });

        // This packet is whenever a client tells the server that a component has changed client side and that it should be 
        // updated on the server as well
        this.AddPacketHandler<UpdateEntitiesPacket>((packet, connection) =>
        {
            foreach (EntityUpdate update in packet.Updates)
            {
                this.PerformEntityUpdate(update);
            }
        });

        // This packet is for whenever a client requests a specific chunk in the world
        this.AddPacketHandler<RequestChunkPacket>((packet, connection) =>
        {
            // Get the chunk address from the packet
            int x = packet.X;
            int y = packet.Y;

            // Construct chunk packet to be sent to requester
            WholeChunkPacket wcp = new WholeChunkPacket()
            {
                X = x,
                Y = y,
                Chunk = this._world.GetChunk(x, y) // This will get the chunk from the world
            };

            // Send the packet to the requester.
            this.EnqueuePacket(wcp, connection, true, false);
        });
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
        if (e.Component.HasCNType(CNType.Update, NDirection.ServerToClient))
        {
            if (this.ShouldEntityComponentSendUpdate(e.Entity.ID, e.Component, e.Attrib))
            {
                EntityUpdate eu = new EntityUpdate(e.Entity.ID, e.Component);
                UpdateEntitiesPacket uep = new UpdateEntitiesPacket(eu);

                this._connections.LockedAction((conns) =>
                {
                    foreach (Connection conn in conns.Where(x => this._playerFullyConnected[x]))
                    {
                        this.EnqueuePacket(uep, conn, true, false);
                    }
                });
            }
        }
    }

    public void HandleEntityAddedToECS(object sender, EntityAddedEventArgs e)
    {
        this._connections.LockedAction((conns) =>
        {
            List<UpdateEntitiesPacket> packets = Utilities.CreateEntityUpdatePackets(CNType.Update | CNType.Snapshot, NDirection.ClientToServer | NDirection.ServerToClient, e.Entity);

            foreach (UpdateEntitiesPacket packet in packets)
            {
                foreach (var conn in conns.Where(x => this._playerFullyConnected[x]))
                {
                    this.EnqueuePacket(packet, conn, false, false);
                }
            }
        });
    }

    public void HandleEntityDestroyedInECS(object sender, EntityDestroyedEventArgs e)
    {
        this._connections.LockedAction((conns) =>
        {
            DestroyEntityPacket dep = new DestroyEntityPacket(e.Entity.ID);
            foreach (var conn in conns.Where(x => this._playerFullyConnected[x]))
            {
                this.EnqueuePacket(dep, conn, true, false);
            }
        });
    }

    public void HandleWorldChunkUpdates(object sender, ChunkUpdatedEventArgs e)
    {
        this._connections.LockedAction((conns) =>
        {
            foreach (Connection conn in conns.Where(x => this._playerFullyConnected[x]))
            {
                ChunkUpdatePacket wcp = new ChunkUpdatePacket()
                {
                    X = e.Chunk.X,
                    Y = e.Chunk.Y,
                    Chunk = e.Chunk
                };

                this.EnqueuePacket(wcp, conn, true, false);
            }
        });
    }

    private void StartSnapshotting()
    {
        int snapshotsPerSecond = 20;
        int millisPerSnapshot = 1000 / snapshotsPerSecond;

        Stopwatch sw = new Stopwatch();
        _ = Task.Run(async () =>
        {
            sw.Start();
            while (true)
            {
                long startTime = sw.ElapsedMilliseconds;
                this.BroadcastSnapshotEntityUpdatesToAllClients();
                long endTime = sw.ElapsedMilliseconds;

                // Wait until next snapshort. Aim for 20 snapshots per second.
                if (endTime - startTime < millisPerSnapshot)
                {
                    await Task.Delay(millisPerSnapshot - ((int)(endTime - startTime)));
                }
            }
        });
    }

    private void StartAutosaving()
    {
        int autoSaveIntervalMillis = 1000 * 60 * 1; // 1 minute

        _ = Task.Run(async () =>
        {
            while (true)
            {
                await Task.Delay(autoSaveIntervalMillis);

                _ = Task.Run(() => this.SaveServer());
            }
        });
    }

    private void StartClientAliveCheck()
    {
        _ = Task.Run(async () =>
        {
            while (true)
            {
                this._connections.LockedAction((conns) =>
                {
                    foreach (var conn in conns)
                    {
                        this.EnqueuePacket(new ClientAlive(), conn, true, false);
                    }
                }, (e) =>
                {
                    Console.WriteLine(e.Message);
                });
                await Task.Delay(2000);
            }
        });
    }

    public new async Task StartAsync()
    {
        // Start world, which basically just starts a task
        // that allows for asynchronous generation of the world.
        this._world.Start();

        // Start UDP server in the background.
        await base.StartAsync();

        // Snapshotting task
        this.StartSnapshotting();

        // Asking clients for aliveness
        this.StartClientAliveCheck();

        // Autosaving task
        this.StartAutosaving();
    }

    public void Update()
    {
        this._ecs.LockedAction((ecs) =>
        {
            ecs.Update(this._world);
        });
    }

    public WorldContainer GetWorldContainer()
    {
        return this._world;
    }

    public WorldMetaData GetWorldMetaData()
    {
        return this._worldMetaData;
    }

    public ECS GetECS()
    {
        return this._ecs.Value;
    }

    public void SaveServer()
    {
        this._worldMetaData.SaveWorld(this._world);

        this._connections.LockedAction((conns) =>
        {
            this._ecs.LockedAction((ecs) =>
            {
                foreach (Connection connection in conns)
                {
                    if (this._playerIds.ContainsKey(connection))
                    {
                        int entityId = this._playerIds[connection];
                        Entity playerEntity = ecs.GetEntityFromID(entityId);

                        string playerName = this._playerNames[connection];
                        PlayerInfo pi = this._worldMetaData.GetPlayerInfo(playerName);

                        // Save relevant data about the player
                        pi.Position = playerEntity.GetComponent<TransformComponent>().Position;

                        // Update the player info in the world meta data
                        this._worldMetaData.UpdatePlayerInfo(playerName, pi);
                    }
                }

                List<Entity> entities = ecs.GetAllEntities(x => !x.HasComponent(typeof(PlayerInfoComponent)));

                this._worldMetaData.SaveEntities(entities);
            });
        });
    }
}