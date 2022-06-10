using System.Diagnostics;
using System.Numerics;
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

    public GameServer(int port) : base(port, 500, 100000000)
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
                foreach (Entity entity in ecs.GetAllEntities())
                {
                    foreach (Component comp in entity.Components)
                    {
                        UpdateEntityComponentPacket uecp = new UpdateEntityComponentPacket(entity.ID, comp);
                        this.EnqueuePacket(uecp, connection, false, false);
                    }
                }
            });

            await Task.Delay(1000);

            this.EnqueuePacket(new ConnectFinished() { PlayerEntityId = newPlayer.ID }, connection, true, true);
        });

        this.AddPacketHandler<UpdateEntityComponentPacket>((packet, connection) =>
        {
            int entityId = packet.EntityID;

            this._ecs.LockedAction((ecs) =>
            {
                if (!ecs.EntityExists(entityId))
                {
                    //GameConsole.WriteLine("CONNECT", $"<0xFF0000>Entity {entityId} does not exist, creating it...</>");
                    return; // Do nothing
                }

                Entity entity = ecs.GetEntityFromID(entityId);

                if (!entity.HasComponent(packet.Component.GetType()))
                {
                    //GameConsole.WriteLine("CONNECT", $"<0x00FF00>Adding component {packet.ComponentType} to entity {entityId}</>");
                    return; // Do nothing
                }
                else
                {
                    entity.GetComponent(packet.Component.GetType()).UpdateComponent(packet.Component);
                }
            });
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
    }

    public new async Task StartAsync()
    {
        // Generate world map
        _ecs = new ThreadSafe<ECS>(new ECS());
        _ecs.Value.Initialize();

        this._ecs.Value.ComponentChanged += (entity, component, behaviour) =>
        {
            if (behaviour == NBType.Update)
            {
                this._connections.LockedAction((conns) =>
                {
                    foreach (var conn in conns)
                    {
                        this.EnqueuePacket(new UpdateEntityComponentPacket(entity.ID, component), conn, true, true);
                    }
                });
            }
        };

        GameConsole.WriteLine("SERVER", "Generating world map...");
        this._world = new WorldContainer(new TestWorldGenerator());
        GameConsole.WriteLine("SERVER", "World map generated.");

        await base.StartAsync();

        // Snapshotting task
        Stopwatch sw = new Stopwatch();
        _ = Task.Run(async () =>
        {
            sw.Start();
            while (true)
            {
                long startTime = sw.ElapsedMilliseconds;

                this._connections.LockedAction((conns) =>
                {
                    _ecs.LockedAction((ecs) =>
                    {
                        foreach (Entity e in ecs.GetAllEntities())
                        {
                            foreach (Component c in e.Components.Where(x => x.ShouldSnapshot()))
                            {
                                foreach (Connection conn in conns)
                                {
                                    UpdateEntityComponentPacket uecp = new UpdateEntityComponentPacket(e.ID, c);
                                    this.EnqueuePacket(uecp, conn, false, false);
                                }
                            }
                        }
                    });
                });
                long endTime = sw.ElapsedMilliseconds;

                // Wait until next snapshort. Aim for 20 snapshots per second.
                if (endTime - startTime < 50)
                {
                    await Task.Delay(50 - ((int)(endTime - startTime)));
                }
            }
        });

        // await Task.Delay(10000);

        // _ = Task.Run(async () =>
        // {
        //     while (true)
        //     {
        //         Vector2 randomVector = Utilities.GetRandomVector2(0, 100, 0, 100);
        //         int x = (int)randomVector.X;
        //         int y = (int)randomVector.Y;

        //         this._crater.GroundLayer.SetTile(x, y, Utilities.GetRandomInt(0, 4));

        //         await Task.Delay(1000);
        //     }
        // });
    }

    float counter = 0f;
    float interval = 1;

    public void Update()
    {
        this._ecs.LockedAction((ecs) =>
        {
            ecs.Update();
        });

        foreach (int id in this._playersIds.Values)
        {
            Entity e = this._ecs.Value.GetEntityFromID(id);

            var pic = e.GetComponent<PlayerInputComponent>();
            var trans = e.GetComponent<TransformComponent>();

            int x = (int)trans.Position.X / TileGrid.TILE_SIZE;
            int y = (int)trans.Position.Y / TileGrid.TILE_SIZE;
        }

        // if (counter > interval && this._playersIds.Count > 0)
        // {
        //     Vector2 randomVector = Utilities.GetRandomVector2(0, 10, 0, 10);

        //     int x = (int)randomVector.X;
        //     int y = (int)randomVector.Y;
        //     int tileId = Utilities.GetRandomInt(1, 4);

        //     while (this._crater.GroundLayer.GetTileIDAtPosition(x, y) == tileId)
        //     {
        //         randomVector = Utilities.GetRandomVector2(0, 10, 0, 10);

        //         x = (int)randomVector.X;
        //         y = (int)randomVector.Y;

        //         tileId = Utilities.GetRandomInt(1, 4);
        //     }

        //     this._crater.GroundLayer.SetTile(x, y, tileId);
        //     counter = 0f;

        //     this._connections.LockedAction((conns) =>
        //     {
        //         foreach (Connection conn in conns)
        //         {
        //             this.EnqueuePacket(new GroundLayerUpdatePacket(x, y, tileId), conn, true, false);
        //         }
        //     });
        // }

        // counter += GameTime.DeltaTime;
    }
}