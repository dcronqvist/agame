using System.Diagnostics;
using System.Numerics;
using System.Reflection;
using AGame.Engine.DebugTools;
using AGame.Engine.ECSys;
using AGame.Engine.ECSys.Components;
using AGame.Engine.World;
using GameUDPProtocol;

namespace AGame.Engine.Networking;

public class GameServer : Server<ConnectRequest, ConnectResponse, QueryResponse>
{
    private ThreadSafe<ECS> _ecs;
    private WorldContainer _world;
    private Dictionary<Connection, int> _playersIds;

    public GameServer(int port) : base(port, 1000, 10000)
    {
        this._playersIds = new Dictionary<Connection, int>();

        this.ConnectionRequested += (sender, e) =>
        {
            e.Accept(new ConnectResponse());
        };

        this.ServerQueryReceived += (sender, e) =>
        {
            e.RespondWith(new QueryResponse());
        };

        this.ClientTimedOut += (sender, e) =>
        {
            GameConsole.WriteLine("SERVER", $"<0xFF0000>Client timed out: {e.Connection.RemoteEndPoint}</>");
        };

        this.AddPacketHandler<ConnectReadyForECS>(async (packet, connection) =>
        {
            Entity newPlayer = null;
            _ecs.LockedAction((ecs) =>
            {
                newPlayer = _ecs.Value.CreateEntityFromAsset("entity_player");
            });
            this._playersIds.Add(connection, newPlayer.ID);
            GameConsole.WriteLine("SERVER", $"New player with ID {newPlayer.ID} connected from {connection.RemoteEndPoint}");
            newPlayer.GetComponent<TransformComponent>().Position = Utilities.GetRandomVector2(0, 16 * 10, 0, 16 * 10);

            _ecs.LockedAction((ecs) =>
            {
                List<EntityUpdate> updates = new List<EntityUpdate>();

                foreach (Entity e in ecs.GetAllEntities())
                {
                    // Send all components, not just snapshottable ones
                    Component[] snapshottedComponents = e.Components.ToArray();

                    // With divisions at most 200, every packet should be able to fit at least 2 entities
                    List<Component[]> divisions = Utilities.DivideIPacketables(snapshottedComponents, 200);

                    foreach (Component[] division in divisions)
                    {
                        updates.Add(new EntityUpdate(e.ID, division));
                    }
                }

                List<EntityUpdate[]> entityUpdateDivisions = Utilities.DivideIPacketables(updates.ToArray(), 400);

                foreach (EntityUpdate[] entityUpdateDivision in entityUpdateDivisions)
                {
                    UpdateEntitiesPacket eup = new UpdateEntitiesPacket(entityUpdateDivision);
                    this.EnqueuePacket(eup, connection, true, true);
                }
            });

            await Task.Delay(1000);

            this.EnqueuePacket(new ConnectFinished() { PlayerEntityId = newPlayer.ID }, connection, true, true);
        });

        this.AddPacketHandler<UpdateEntitiesPacket>((packet, connection) =>
        {
            foreach (EntityUpdate update in packet.Updates)
            {
                int entityId = update.EntityID;

                this._ecs.LockedAction((e) =>
                {
                    if (!e.EntityExists(entityId))
                    {
                        //GameConsole.WriteLine("CONNECT", $"<0xFF0000>Entity {entityId} does not exist, creating it...</>");
                        e.CreateEntity(entityId);
                    }

                    Entity entity = e.GetEntityFromID(entityId);

                    foreach (Component component in update.Components)
                    {
                        if (!entity.HasComponent(component.GetType()))
                        {
                            //GameConsole.WriteLine("CONNECT", $"<0x00FF00>Adding component {packet.ComponentType} to entity {entityId}</>");
                            e.AddComponentToEntity(entity, component);
                        }
                        else
                        {
                            //GameConsole.WriteLine("CONNECT", $"<0x00FF00>Updating component {packet.ComponentType} to entity {entityId}</>");

                            entity.GetComponent(component.GetType()).UpdateComponent(component);
                        }
                    }
                });
            }
        });

        this.AddPacketHandler<RequestChunkPacket>((packet, connection) =>
        {
            int x = packet.X;
            int y = packet.Y;

            WholeChunkPacket wcp = new WholeChunkPacket()
            {
                X = x,
                Y = y,
                Chunk = this._world.GetChunk(x, y)
            };

            this.EnqueuePacket(wcp, connection, true, false);
        });

        this.ClientDisconnected += (sender, e) =>
        {
            this._ecs.LockedAction((ecs) =>
            {
                if (this._playersIds.ContainsKey(e.Connection))
                {
                    int entityId = this._playersIds[e.Connection];
                    ecs.DestroyEntity(entityId);
                    this._playersIds.Remove(e.Connection);
                }
            });
        };

        this.ClientTimedOut += (sender, e) =>
        {
            this._ecs.LockedAction((ecs) =>
            {
                if (this._playersIds.ContainsKey(e.Connection))
                {
                    int entityId = this._playersIds[e.Connection];
                    ecs.DestroyEntity(entityId);
                    this._playersIds.Remove(e.Connection);
                }
            });
        };

        this.PacketSent += (sender, e) =>
        {
            if (e.Packet is ConnectFinished cf)
            {
                int x = 2;
            }
        };
    }

    public new async Task StartAsync()
    {
        // Generate world map
        _ecs = new ThreadSafe<ECS>(new ECS());

        ThreadSafe<Dictionary<int, Dictionary<int, float>>> lastUpdateTimes = new ThreadSafe<Dictionary<int, Dictionary<int, float>>>(new Dictionary<int, Dictionary<int, float>>());

        _ecs.Value.Initialize(SystemRunner.Server);

        _ecs.Value.ComponentChanged += (sender, e) =>
        {
            if (e.Component.HasCNType(CNType.Update, NDirection.ServerToClient))
            {
                _ecs.LockedAction((ecs) =>
                {
                    ComponentNetworkingAttribute cna = e.Component.GetCNAttrib();

                    lastUpdateTimes.LockedAction((lut) =>
                    {
                        if (!lut.ContainsKey(e.Entity.ID))
                        {
                            lut.Add(e.Entity.ID, new Dictionary<int, float>());
                        }

                        Dictionary<int, float> entityTimes = lut[e.Entity.ID];

                        if (!entityTimes.ContainsKey(ECS.Instance.Value.GetComponentID(e.Component.GetType())))
                        {
                            entityTimes.Add(ECS.Instance.Value.GetComponentID(e.Component.GetType()), 0f);
                        }

                        float lastUpdated = entityTimes[ECS.Instance.Value.GetComponentID(e.Component.GetType())];

                        if (cna.MaxUpdatesPerSecond == 0 || (GameTime.TotalElapsedSeconds - lastUpdated) > (1f / cna.MaxUpdatesPerSecond))
                        {
                            _connections.LockedAction((conns) =>
                            {
                                foreach (Connection conn in conns)
                                {
                                    this.EnqueuePacket(new UpdateEntitiesPacket(new EntityUpdate(e.Entity.ID, e.Component)), conn, cna.IsReliable, false);
                                }
                            });

                            entityTimes[ECS.Instance.Value.GetComponentID(e.Component.GetType())] = GameTime.TotalElapsedSeconds;
                        }
                    });
                });
            }
        };

        _ecs.Value.EntityAdded += (sender, e) =>
        {
            this._connections.LockedAction((conns) =>
            {
                List<EntityUpdate> updates = new List<EntityUpdate>();

                Component[] snapshottedComponents = e.Entity.Components.ToArray();

                // With divisions at most 200, every packet should be able to fit at least 2 entities
                List<Component[]> divisions = Utilities.DivideIPacketables(snapshottedComponents, 200);

                foreach (Component[] division in divisions)
                {
                    updates.Add(new EntityUpdate(e.Entity.ID, division));
                }

                List<EntityUpdate[]> entityUpdateDivisions = Utilities.DivideIPacketables(updates.ToArray(), 400);

                foreach (EntityUpdate[] entityUpdateDivision in entityUpdateDivisions)
                {
                    UpdateEntitiesPacket eup = new UpdateEntitiesPacket(entityUpdateDivision);
                    foreach (var conn in conns)
                    {
                        this.EnqueuePacket(eup, conn, false, false);
                    }
                }
            }, (e) =>
            {
                GameConsole.WriteLine("SERVER", $"{e.Message}");
            });
        };

        _ecs.Value.EntityDestroyed += (sender, e) =>
        {
            this._connections.LockedAction((conns) =>
            {
                foreach (var conn in conns)
                {
                    DestroyEntityPacket dep = new DestroyEntityPacket(e.Entity.ID);
                    this.EnqueuePacket(dep, conn, false, false);
                }
            }, (e) =>
            {
                GameConsole.WriteLine("SERVER", $"{e.Message}");
            });
        };

        GameConsole.WriteLine("SERVER", "Generating world map...");
        this._world = new WorldContainer(new TestWorldGenerator());
        this._world.Start();
        GameConsole.WriteLine("SERVER", "World map generated.");

        await base.StartAsync();

        // Snapshotting task
        Stopwatch sw = new Stopwatch();
        _ = Task.Run(async () =>
        {
            int maxBytesPerSnapshot = 490;

            sw.Start();
            while (true)
            {
                long startTime = sw.ElapsedMilliseconds;

                _ecs.LockedAction((ecs) =>
                {
                    List<EntityUpdate> updates = new List<EntityUpdate>();

                    foreach (Entity e in ecs.GetAllEntities())
                    {
                        Component[] snapshottedComponents = e.GetComponentsWithCNType(CNType.Snapshot, NDirection.ServerToClient);

                        // With divisions at most 200, every packet should be able to fit at least 2 entities
                        List<Component[]> divisions = Utilities.DivideIPacketables(snapshottedComponents, maxBytesPerSnapshot);

                        foreach (Component[] division in divisions)
                        {
                            updates.Add(new EntityUpdate(e.ID, division));
                        }
                    }

                    List<EntityUpdate[]> entityUpdateDivisions = Utilities.DivideIPacketables(updates.ToArray(), maxBytesPerSnapshot);

                    foreach (EntityUpdate[] entityUpdateDivision in entityUpdateDivisions)
                    {
                        UpdateEntitiesPacket eup = new UpdateEntitiesPacket(entityUpdateDivision);

                        this._connections.LockedAction((conns) =>
                        {
                            foreach (var conn in conns)
                            {
                                this.EnqueuePacket(eup, conn, false, false);
                            }
                        }, (e) =>
                        {
                            Console.WriteLine(e.Message);
                        });
                    }
                }, (e) =>
                {
                    Console.WriteLine(e.Message);
                });
                long endTime = sw.ElapsedMilliseconds;

                // Wait until next snapshort. Aim for 20 snapshots per second.
                if (endTime - startTime < 50)
                {
                    await Task.Delay(50 - ((int)(endTime - startTime)));
                }
            }
        });

        // Asking clients for aliveness
        _ = Task.Run(() =>
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
                Thread.Sleep(2000);
            }
        });

        this._world.ChunkUpdated += (sender, e) =>
        {
            this._connections.LockedAction((conns) =>
            {
                foreach (Connection conn in conns)
                {
                    ChunkUpdatePacket wcp = new ChunkUpdatePacket()
                    {
                        X = e.Chunk.X,
                        Y = e.Chunk.Y,
                        Chunk = e.Chunk
                    };

                    this.EnqueuePacket(wcp, conn, true, false);
                }
            }, (e) =>
            {
                GameConsole.WriteLine("SERVER", $"{e.Message}");
            });
        };
    }

    public void Update()
    {
        this._ecs.LockedAction((ecs) =>
        {
            ecs.Update(this._world);
        });

        this._world.Update();

        foreach (int id in this._playersIds.Values)
        {
            Entity e = this._ecs.Value.GetEntityFromID(id);

            var pic = e.GetComponent<KeyboardInputComponent>();
            var trans = e.GetComponent<TransformComponent>();

            Vector2i tilePos = trans.GetTilePosition();

            if (pic.IsKeyPressed(KeyboardInputComponent.KEY_SPACE))
            {
                this._world.UpdateTile(tilePos.X, tilePos.Y, "game:grass");
            }
        }
    }
}