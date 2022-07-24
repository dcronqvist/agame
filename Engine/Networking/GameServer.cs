using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Numerics;
using System.Threading.Tasks;
using AGame.Engine.Assets;
using AGame.Engine.Configuration;
using AGame.Engine.ECSys;
using AGame.Engine.ECSys.Components;
using AGame.Engine.Graphics;
using AGame.Engine.Graphics.Rendering;
using AGame.Engine.Items;
using AGame.Engine.World;
using GameUDPProtocol;

namespace AGame.Engine.Networking;

public class UserCommand : Packet
{
    public int LastReceivedServerTick { get; set; }
    public int CommandNumber { get; set; }
    public float DeltaTime { get; set; }

    public ushort PreviousButtons { get; set; }

    public ushort Buttons { get; set; }

    public int MouseTileX { get; set; }
    public int MouseTileY { get; set; }

    [PacketPropIgnore]
    public bool HasBeenRun { get; set; }

    public UserCommand()
    {

    }

    public UserCommand(ushort previousButtons, float delta, int commandNumber, Vector2i mouseTilePos)
    {
        this.PreviousButtons = previousButtons;
        this.DeltaTime = delta;
        this.CommandNumber = commandNumber;
        this.MouseTileX = mouseTilePos.X;
        this.MouseTileY = mouseTilePos.Y;
    }

    public UserCommand(ushort previousButtons, float delta, ushort buttons, int commandNumber, Vector2i mouseTilePos)
    {
        this.PreviousButtons = previousButtons;
        this.Buttons = buttons;
        this.DeltaTime = delta;
        this.CommandNumber = commandNumber;
        this.MouseTileX = mouseTilePos.X;
        this.MouseTileY = mouseTilePos.Y;
    }

    public static readonly ushort KEY_W = 1 << 0;
    public static readonly ushort KEY_A = 1 << 1;
    public static readonly ushort KEY_S = 1 << 2;
    public static readonly ushort KEY_D = 1 << 3;
    public static readonly ushort KEY_SPACE = 1 << 4;
    public static readonly ushort KEY_SHIFT = 1 << 5;

    public static readonly ushort MOUSE_SCROLL_UP = 1 << 6;
    public static readonly ushort MOUSE_SCROLL_DOWN = 1 << 7;

    public static readonly ushort USE_ITEM = 1 << 8;

    public void SetInputDown(ushort key)
    {
        this.Buttons |= key;
    }

    public bool IsInputDown(ushort key)
    {
        return (this.Buttons & key) != 0;
    }

    public bool IsInputPressed(ushort key)
    {
        return (this.Buttons & key) != 0 && (this.PreviousButtons & key) == 0;
    }

    public bool IsInputReleased(ushort key)
    {
        return (this.Buttons & key) == 0 && (this.PreviousButtons & key) != 0;
    }
}

public class GameServer : Server<ConnectRequest, ConnectResponse, QueryResponse>
{
    private ThreadSafe<ECS> _ecs;
    private WorldContainer _world;
    private WorldMetaData _worldMeta;
    private ThreadSafe<Dictionary<Connection, int>> _connectionToPlayerId;
    private ThreadSafe<Queue<(Connection, UserCommand)>> _receivedCommands;
    private ThreadSafe<Dictionary<Connection, UserCommand>> _lastProcessedCommand;
    private List<(Entity, Component)> _updatedComponents;
    private GameServerConfiguration _configuration;
    private ThreadSafe<Queue<IServerTickAction>> _nextTickActions;
    private ThreadSafe<Dictionary<Connection, List<ChunkAddress>>> _connectionsLoadedChunks;
    private Dictionary<Connection, List<Entity>> _connectionsLoadedEntities;
    private ThreadSafe<Dictionary<Connection, List<Entity>>> _connectionsViewingContainerInEntity;

    // Server tick
    private int _serverTick = 0;

    // Sends chunks of this distance around the player to clients.
    private int _chunkDistanceX = 2;
    private int _chunkDistanceY = 2;

    public GameServer(ECS ecs, WorldContainer world, WorldMetaData worldMeta, GameServerConfiguration config, int reliableMillisBeforeResend, int clientTimeoutMillis) : base(config.Port, reliableMillisBeforeResend, clientTimeoutMillis, new TestEncoder())
    {
        this._configuration = config;
        this._world = world;
        this._worldMeta = worldMeta;
        this._connectionToPlayerId = new ThreadSafe<Dictionary<Connection, int>>(new Dictionary<Connection, int>());
        this._receivedCommands = new ThreadSafe<Queue<(Connection, UserCommand)>>(new Queue<(Connection, UserCommand)>());
        this._lastProcessedCommand = new ThreadSafe<Dictionary<Connection, UserCommand>>(new Dictionary<Connection, UserCommand>());
        this._nextTickActions = new ThreadSafe<Queue<IServerTickAction>>(new Queue<IServerTickAction>());
        this._connectionsLoadedChunks = new ThreadSafe<Dictionary<Connection, List<ChunkAddress>>>(new Dictionary<Connection, List<ChunkAddress>>());
        this._ecs = new ThreadSafe<ECS>(ecs);
        this._connectionsLoadedEntities = new Dictionary<Connection, List<Entity>>();
        this._updatedComponents = new List<(Entity, Component)>();
        this._connectionsViewingContainerInEntity = new ThreadSafe<Dictionary<Connection, List<Entity>>>(new Dictionary<Connection, List<Entity>>());

        this.RegisterServerEventHandlers();
        this.RegisterPacketHandlers();
    }

    private bool IsAllowedToConnect(Connection conn, ConnectRequest request, out string reason)
    {
        if (this._connections.LockedAction((conns) => conns.Count) <= this._configuration.MaxConnections)
        {
            reason = "";
            return true;
        }
        else
        {
            reason = "The server is full";
            return false;
        }
    }

    private void RegisterServerEventHandlers()
    {
        base.ConnectionRequested += (sender, e) =>
        {
            Logging.Log(LogLevel.Debug, $"Server: Connection request from {e.RequestConnection.RemoteEndPoint}");

            if (this.IsAllowedToConnect(e.RequestConnection, e.RequestPacket, out string reason))
            {
                Entity entity = this._ecs.LockedAction((ecs) =>
                {
                    Entity entity = ecs.CreateEntityFromAsset("default.entity.player");
                    entity.GetComponent<CharacterComponent>().Name = e.RequestPacket.Name;
                    ItemInstance ii = ItemManager.GetItemDef("default.item.pebble").CreateItem();
                    ii.GetComponent<DefaultMod.Tool>().CurrentDurability = 500;

                    entity.GetComponent<ContainerComponent>().GetContainer().AddItem(ii);

                    ii = ItemManager.GetItemDef("default.item.pebble").CreateItem();
                    ii.GetComponent<DefaultMod.Tool>().CurrentDurability = 100;
                    entity.GetComponent<ContainerComponent>().GetContainer().AddItem(ii);

                    // int runs = Utilities.GetRandomInt(5, 10);
                    // for (int i = 0; i < runs; i++)
                    // {
                    //     string item = Utilities.ChooseUniform("default.item.test_item", "default.item.test_item_2");
                    //     entity.GetComponent<InventoryComponent>().GetInventory().AddItem(item, 1);
                    // }


                    return entity;
                });

                this._connectionToPlayerId.LockedAction((connectionToPlayerId) =>
                {
                    connectionToPlayerId[e.RequestConnection] = entity.ID;
                });

                this._lastProcessedCommand.LockedAction((lastProcessedCommand) =>
                {
                    lastProcessedCommand.Add(e.RequestConnection, new UserCommand());
                });

                this._connectionsViewingContainerInEntity.LockedAction((connectionsViewingContainerInEntity) =>
                {
                    connectionsViewingContainerInEntity.Add(e.RequestConnection, new List<Entity>() { entity }); // Player should always be "viewing" itself
                });

                e.Accept(new ConnectResponse() { PlayerEntityID = entity.ID, ServerTickSpeed = this._configuration.TickRate, PlayerChunkX = 0, PlayerChunkY = 0 });
            }
            else
            {
                e.Reject(new ConnectResponse(), reason);
            }
        };

        base.ConnectionAccepted += (sender, e) =>
        {
            Logging.Log(LogLevel.Debug, $"Server: Connection accepted from {e.Connection.RemoteEndPoint}");
        };

        base.ConnectionRejected += (sender, e) =>
        {
            Logging.Log(LogLevel.Debug, $"Server: Connection rejected from {e.Requester}");

            this._connectionToPlayerId.LockedAction((connectionToPlayerId) =>
            {
                int id = connectionToPlayerId[this._connections.LockedAction((c) => c.Find(conn => conn.RemoteEndPoint == e.Requester))];
                this.DestroyEntity(id);

                connectionToPlayerId.Remove(this._connections.LockedAction((c) => c.Find(conn => conn.RemoteEndPoint == e.Requester)));
            });

            this._lastProcessedCommand.LockedAction((lastProcessedCommand) =>
            {
                lastProcessedCommand.Remove(this._connections.LockedAction((c) => c.Find(conn => conn.RemoteEndPoint == e.Requester)));
            });
        };

        base.ClientDisconnected += (sender, e) =>
        {
            Logging.Log(LogLevel.Debug, $"Server: Client disconnected from {e.Connection.RemoteEndPoint}");

            int playerEntityId = this._connectionToPlayerId.LockedAction((connectionToPlayerId) =>
            {
                int id = connectionToPlayerId[e.Connection];
                connectionToPlayerId.Remove(e.Connection);
                return id;
            });

            this._lastProcessedCommand.LockedAction((lastProcessedCommand) =>
            {
                lastProcessedCommand.Remove(e.Connection);
            });

            this.PerformActionNextTick(new DestroyEntityAction(playerEntityId));
        };

        this.ServerQueryReceived += (sender, e) => e.RespondWith(new QueryResponse());

        this._ecs.Value.ComponentChanged += (sender, e) =>
        {
            if (this._updatedComponents.Contains((e.Entity, e.Component)))
            {
                return;
            }

            this._updatedComponents.Add((e.Entity, e.Component));
        };

        this._world.ChunkGenerated += (sender, e) =>
        {
            // For every chunk that is generated, run all entity distribution definitions on it.
            foreach (var definition in this._world.WorldGenerator.GetEntityDistributionDefinitions())
            {
                this.PerformActionNextTick(new ExecuteSpawnEntityDefinitionsAction(definition, e.Chunk));
            }
        };
    }

    private void PerformActionNextTick(IServerTickAction action)
    {
        this._nextTickActions.LockedAction((actions) =>
        {
            actions.Enqueue(action);
        });
    }

    public void PerformOnECS(Action<ECS> action)
    {
        this._ecs.LockedAction((ecs) =>
        {
            action(ecs);
        });
    }

    public T PerformOnECS<T>(Func<ECS, T> action)
    {
        return this._ecs.LockedAction((ecs) =>
        {
            return action(ecs);
        });
    }

    public void SendContainerContentsToViewers(Entity entity)
    {
        this._connectionsViewingContainerInEntity.LockedAction((view) =>
        {
            foreach (var connection in view.Keys)
            {
                if (view[connection].Contains(entity))
                {
                    this.EnqueuePacket(new SetContainerContentPacket(entity.ID, entity.GetComponent<ContainerComponent>().GetContainer(), false), connection, true, false);
                }
            }
        });
    }

    private void RegisterPacketHandlers()
    {
        base.AddPacketHandler<UserCommand>((packet, connection) =>
        {
            this._receivedCommands.LockedAction((queue) =>
            {
                queue.Enqueue((connection, packet));
            });
        });

        base.AddPacketHandler<ConnectReadyForData>((packet, connection) =>
        {
            //this.BroadcastEntireECS(connection);
            var playerID = this._connectionToPlayerId.LockedAction((connectionToPlayerId) =>
            {
                return connectionToPlayerId[connection];
            });
            var playerContainer = this._ecs.LockedAction((ecs) =>
            {
                return ecs.GetEntityFromID(playerID).GetComponent<ContainerComponent>().GetContainer();
            });
            this.EnqueuePacket(new SetContainerContentPacket(playerID, playerContainer, false), connection, true, false);
        });

        base.AddPacketHandler<ReceivedChunkPacket>((packet, connection) =>
        {
            this._connectionsLoadedChunks.LockedAction((dic) =>
            {
                if (!dic.ContainsKey(connection))
                {
                    dic.Add(connection, new List<ChunkAddress>());
                }

                dic[connection].Add(new ChunkAddress(packet.X, packet.Y));
            });
        });

        base.AddPacketHandler<RequestChunkPacket>((packet, connection) =>
        {
            Logging.Log(LogLevel.Debug, $"Server: Received chunk request from {connection.RemoteEndPoint} for {packet.X}, {packet.Y}");
            Task.Run(() => new SendChunkToClientAction(connection, packet.X, packet.Y).Tick(this));
        });

        base.AddPacketHandler<ClickContainerSlotPacket>((packet, connection) =>
        {
            var entityID = packet.EntityID;

            this._ecs.LockedAction((ecs) =>
            {
                var entity = ecs.GetEntityFromID(entityID);

                var container = entity.GetComponent<ContainerComponent>().GetContainer();

                ContainerSlot mouseSlot = new ContainerSlot(new Vector2(0, 0));

                var player = ecs.GetEntityFromID(this._connectionToPlayerId.LockedAction((connectionToPlayerId) =>
                {
                    return connectionToPlayerId[connection];
                }));
                var playerState = player.GetComponent<PlayerStateComponent>();

                mouseSlot.Item = playerState.MouseSlot.Item.Instance;
                mouseSlot.Count = playerState.MouseSlot.ItemCount;

                container.ClickSlot(packet.SlotID, ref mouseSlot);

                playerState.MouseSlot = mouseSlot.ToSlotInfo(0);

                this.SendContainerContentsToViewers(entity);
            });
        });

        base.AddPacketHandler<RequestViewContainerPacket>((packet, connection) =>
        {
            var entityID = packet.EntityID;

            var entity = this._ecs.LockedAction((ecs) =>
            {
                return ecs.GetEntityFromID(entityID);
            });

            this._connectionsViewingContainerInEntity.LockedAction((view) =>
            {
                view[connection].Add(entity);
            });

            // TODO: Whether or not the client is allowed to open the container should come from the container's provider
            this.EnqueuePacket(new SetContainerContentPacket(entityID, entity.GetComponent<ContainerComponent>().GetContainer(), true), connection, true, false);
        });

        base.AddPacketHandler<CloseContainerPacket>((packet, connection) =>
        {
            var entityID = packet.EntityID;

            var entity = this._ecs.LockedAction((ecs) =>
            {
                return ecs.GetEntityFromID(entityID);
            });

            var playerID = this._connectionToPlayerId.LockedAction((connectionToPlayerId) =>
            {
                return connectionToPlayerId[connection];
            });

            if (entityID == playerID)
            {
                // DO not remove the player's own entity from it's view list.
                return;
            }

            this._connectionsViewingContainerInEntity.LockedAction((view) =>
            {
                view[connection].Remove(entity);
            });
        });
    }

    private void SendChunksToClient(Connection connection)
    {
        int playerEntityID = this._connectionToPlayerId.LockedAction((ctp) => ctp[connection]);
        Entity playerEntity = this._ecs.LockedAction((ecs) => ecs.GetEntityFromID(playerEntityID));
        ChunkAddress chunkAddress = playerEntity.GetComponent<TransformComponent>().Position.ToChunkAddress();
        int x = chunkAddress.X;
        int y = chunkAddress.Y;

        List<ChunkAddress> currentLoaded = this._connectionsLoadedChunks.LockedAction((clc) => clc.ContainsKey(connection) ? clc[connection].ToList() : new List<ChunkAddress>());

        List<ChunkAddress> chunksToSend = new List<ChunkAddress>();
        int fromX = x - this._chunkDistanceX;
        int toX = x + this._chunkDistanceX;
        int fromY = y - this._chunkDistanceY;
        int toY = y + this._chunkDistanceY;

        for (int i = fromX; i <= toX; i++)
        {
            for (int j = fromY; j <= toY; j++)
            {
                if (currentLoaded.Contains(new ChunkAddress(i, j)))
                {
                    continue;
                }

                chunksToSend.Add(new ChunkAddress(i, j));
            }
        }

        foreach (ChunkAddress chunk in chunksToSend)
        {
            Task.Run(() => new SendChunkToClientAction(connection, chunk.X, chunk.Y).Tick(this));
        }

        List<ChunkAddress> chunksToUnload = new List<ChunkAddress>();
        foreach (ChunkAddress chunk in currentLoaded)
        {
            if (chunk.X < fromX || chunk.X > toX || chunk.Y < fromY || chunk.Y > toY)
            {
                chunksToUnload.Add(chunk);
            }
        }

        foreach (ChunkAddress chunk in chunksToUnload)
        {
            this._connectionsLoadedChunks.LockedAction((clc) =>
            {
                clc[connection].Remove(chunk);
                Task.Run(() => new TellClientToUnloadChunkAction(connection, chunk.X, chunk.Y).Tick(this));
            });
        }
    }

    public WorldContainer GetWorld()
    {
        return this._world;
    }

    private void ProcessInputs()
    {
        while (true)
        {
            bool done = this._receivedCommands.LockedAction((queue) =>
            {
                if (queue.Count < 1)
                {
                    return true;
                }

                (Connection connection, UserCommand command) = queue.Dequeue();
                UserCommand commandBefore = this._lastProcessedCommand.LockedAction((lastProcessedCommand) =>
                {
                    return lastProcessedCommand[connection];
                });
                command.PreviousButtons = commandBefore?.Buttons ?? 0;

                int entityID = this._connectionToPlayerId.Value.GetValueOrDefault(connection, -1);

                if (entityID == -1)
                {
                    return true;
                }

                // Apply input to ECS and get all world updates as a result of this input
                this._ecs.LockedAction((ecs) =>
                {
                    Entity entity = ecs.GetEntityFromID(entityID);
                    entity.ApplyInput(command, this._world, ecs);
                });

                this._lastProcessedCommand.LockedAction((lastProcessedCommand) =>
                {
                    lastProcessedCommand[connection] = command;
                });

                return false;
            });

            if (done)
            {
                break;
            }
        }
    }

    private void SendECSUpdate(Connection connection, List<EntityUpdate> updates, int serverTick, int[] deleteEntities)
    {
        int lastProcessedCommand = this._lastProcessedCommand.LockedAction((lastProcessedCommand) =>
        {
            return lastProcessedCommand[connection].CommandNumber;
        });

        UpdateEntitiesPacket uep = new UpdateEntitiesPacket(lastProcessedCommand, serverTick, deleteEntities, updates.ToArray());
        base.EnqueuePacket(uep, connection, false, true);
    }

    // This is to provide lag compensation for performing actions on the server
    private void ReconstructECSForClientAtTick(Connection connection, int tick)
    {
        // Figure out 
    }

    public void DestroyEntity(int entityId)
    {
        this._ecs.LockedAction((ecs) =>
        {
            ecs.DestroyEntity(entityId);
        });
    }

    private List<Entity> GetEntitiesInRangeOfPlayer(Connection conn)
    {
        float entityDistance = 20f;

        return this._ecs.LockedAction((ecs) =>
        {
            var playerID = this._connectionToPlayerId.LockedAction((ctp) => ctp[conn]);
            var playerTransform = ecs.GetEntityFromID(playerID).GetComponent<TransformComponent>();
            var playerPosition = playerTransform.Position;

            List<Entity> inRange = ecs.GetAllEntities(e => !e.HasComponent<TransformComponent>() || e.GetComponent<TransformComponent>().Position.DistanceTo(playerPosition) <= entityDistance).ToList();
            return inRange;
        });
    }

    private List<EntityUpdate> CreateUpdatesForNewEntities(List<Entity> entities)
    {
        List<EntityUpdate> updates = new List<EntityUpdate>();
        foreach (Entity entity in entities)
        {
            updates.Add(new EntityUpdate(entity.ID, entity.Components.Where(c => c.GetCNAttrib().CreateTriggersNetworkUpdate).ToArray()));
        }
        return updates;
    }

    private void Tick(float deltaTime)
    {
        this._serverTick += 1;

        this.ProcessInputs();

        this._ecs.LockedAction((ecs) =>
        {
            ecs.Update(null, deltaTime);
        });

        List<EntityUpdate> updatesToSend = Utilities.GetPackedEntityUpdatesMaxByteSize(this.Encoder, this._updatedComponents, 800, out List<(Entity, Component)> usedUpdates);

        this._connections.LockedAction((conns) =>
        {
            foreach (Connection conn in conns)
            {
                this.SendChunksToClient(conn);

                if (!this._connectionsLoadedEntities.ContainsKey(conn))
                    this._connectionsLoadedEntities.Add(conn, new List<Entity>());

                List<Entity> lastEntitiesInRange = this._connectionsLoadedEntities[conn];
                List<Entity> entitiesInRange = this.GetEntitiesInRangeOfPlayer(conn);

                this._connectionsLoadedEntities[conn] = entitiesInRange;

                List<Entity> newEntities = entitiesInRange.Except(lastEntitiesInRange).ToList();
                // Somehow create these entities on the client
                // These will be new to the client, so the client receiving them will mean
                // that they will create them locally and bind their server side entity id to the client side entity id
                List<EntityUpdate> newEntityUpdates = this.CreateUpdatesForNewEntities(newEntities);

                List<Entity> entitiesToRemove = lastEntitiesInRange.Except(entitiesInRange).ToList();
                // Destroy these entities on the client
                int[] entityIDsToRemove = entitiesToRemove.Select(e => e.ID).ToArray();

                this.SendECSUpdate(conn, updatesToSend.Concat(newEntityUpdates).ToList(), this._serverTick, entityIDsToRemove);
            }
        });

        this._nextTickActions.LockedAction((actions) =>
        {
            while (actions.Count > 0)
            {
                IServerTickAction action = actions.Dequeue();
                action.Tick(this);
            }
        });

        foreach ((Entity, Component) update in usedUpdates)
        {
            this._updatedComponents.Remove(update);
        }
    }

    public async Task RunAsync()
    {
        await Task.Run(async () =>
        {
            Stopwatch watch = new Stopwatch();
            watch.Start();
            double delta = 0;

            float currentTickTime = 0f;
            float lastTickTime = 0f;
            float tickDelta = 0f;

            while (true)
            {
                double start = watch.Elapsed.TotalMilliseconds;
                currentTickTime = (float)start;
                tickDelta = (currentTickTime - lastTickTime) / 1000f;

                this.Tick(tickDelta);
                double end = watch.Elapsed.TotalMilliseconds;
                delta = end - start;


                if (delta < (1000L / this._configuration.TickRate))
                {
                    await Task.Delay(TimeSpan.FromMilliseconds((1000L / this._configuration.TickRate) - delta));
                }
                else
                {
                    Logging.Log(LogLevel.Debug, $"Server: Took {delta}ms to tick, which is too long");
                }

                lastTickTime = currentTickTime;
            }
        });
    }

    public void SaveServer()
    {

    }

    private float CalcPriorityForEntity(Connection connection, Entity entity, float cutoffDistance)
    {
        var transform = entity.GetComponent<TransformComponent>();
        var entityPos = transform.Position;

        var playerID = this._connectionToPlayerId.LockedAction((c) => c[connection]);
        var playerEntity = this._ecs.LockedAction((ecs) => ecs.GetEntityFromID(playerID));
        var playerTransform = playerEntity.GetComponent<TransformComponent>();
        var playerPos = playerTransform.Position;

        var distance = (entityPos - playerPos).Length();

        // Currently follows a simple quadratic formula, maybe try using a exponential one?
        // Exponential gets kinda weird and is slow as hell, so I'm not using it for now.
        return Utilities.CalcQuadratic((1f / (cutoffDistance * cutoffDistance)), 0f, 1f, -distance);
    }
}