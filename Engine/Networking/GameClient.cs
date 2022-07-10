using System.Diagnostics;
using System.Numerics;
using AGame.Engine.Assets;
using AGame.Engine.Configuration;
using AGame.Engine.ECSys;
using AGame.Engine.ECSys.Components;
using AGame.Engine.Graphics;
using AGame.Engine.Graphics.Rendering;
using AGame.Engine.World;
using GameUDPProtocol;

namespace AGame.Engine.Networking;

public class GameClient : Client<ConnectRequest, ConnectResponse>
{
    private UserCommand _lastSentCommand;
    private int _serverLastProcessedCommand;

    private ECS _ecs;
    private WorldContainer _world;
    private List<UserCommand> _pendingCommands;
    private ThreadSafe<Queue<UpdateEntitiesPacket>> _receivedEntityUpdates;
    private ThreadSafe<Queue<Packet>> _receivedPackets;
    private ThreadSafe<Queue<Packet>> _sentPackets;
    private ThreadSafe<Queue<int>> _latency;

    private Dictionary<int, Entity> _serverEntityIDToClientEntity;

    private int _playerId;
    private ChunkAddress _previousChunkAddress;
    private float _interpolationTime;
    private int _fakeLatency;
    private string _hostname;
    private int _port;

    private int _renderDistance = 4;

    public GameClient(string hostname, int port, int reliableMillisBeforeResend, int timeoutMillis) : base(hostname, port, reliableMillisBeforeResend, timeoutMillis)
    {
        this._ecs = new ECS();
        this._ecs.Initialize(SystemRunner.Client);
        this._fakeLatency = 0;
        this._hostname = hostname;
        this._port = port;

        this._latency = new ThreadSafe<Queue<int>>(new Queue<int>());
        this._receivedEntityUpdates = new ThreadSafe<Queue<UpdateEntitiesPacket>>(new Queue<UpdateEntitiesPacket>());
        this._pendingCommands = new List<UserCommand>();
        this._serverEntityIDToClientEntity = new Dictionary<int, Entity>();
        this._receivedPackets = new ThreadSafe<Queue<Packet>>(new Queue<Packet>());
        this._sentPackets = new ThreadSafe<Queue<Packet>>(new Queue<Packet>());
        this._playerId = -1;

        this.RegisterClientEventHandlers();
        this.RegisterPacketHandlers();
    }

    public void SetWorld(WorldContainer world)
    {
        this._world = world;
    }

    private void StartLatencyChecking()
    {
        Task.Run(async () =>
        {
            while (true)
            {
                QueryResponse response = await Client.QueryServerAsync<QueryResponse>(this._hostname, this._port, 5000);
                await Task.Delay(1000);

                _ = Task.Run(async () =>
                {
                    this._latency.LockedAction(q =>
                    {
                        q.Enqueue(response.GetPing());
                    });
                    await Task.Delay(5000);
                    this._latency.LockedAction(q =>
                    {
                        q.Dequeue();
                    });
                });
            }
        });
    }

    public void SetFakelatency(int milliseconds)
    {
        this._fakeLatency = milliseconds;
    }

    public int GetRemoteIDForEntity(int localEntityID)
    {
        return this._serverEntityIDToClientEntity.First(x => x.Value.ID == localEntityID).Key;
    }

    private void RegisterClientEventHandlers()
    {
        base.Connecting += (sender, e) =>
        {
            Logging.Log(LogLevel.Debug, "Connecting to server...");
        };

        base.ConnectionAccepted += (sender, e) =>
        {
            Logging.Log(LogLevel.Debug, $"Connected to server {e.Connection.RemoteEndPoint}");
        };

        base.PacketReceived += async (sender, e) =>
        {
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

        base.PacketSent += async (sender, e) =>
        {
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

    private void RegisterPacketHandlers()
    {
        base.AddPacketHandler<UpdateEntitiesPacket>((packet) =>
        {
            this._receivedEntityUpdates.LockedAction((rep) =>
            {
                rep.Enqueue(packet);
            });
        });

        this.AddPacketHandler<DestroyEntityPacket>((packet) =>
        {
            this.DestroyClientSideEntity(packet.EntityID);
        });

        this.AddPacketHandler<ChunkUpdatePacket>((packet) =>
        {
            this._world.UpdateChunk(packet.X, packet.Y, packet.Chunk);
        });

        this.AddPacketHandler<UnloadChunkPacket>((packet) =>
        {
            this._world.DiscardChunk(packet.X, packet.Y);
        });

        this.AddPacketHandler<WholeChunkPacket>((packet) =>
        {
            this._world.UpdateChunk(packet.X, packet.Y, packet.Chunk);

            this.EnqueuePacket(new ReceivedChunkPacket(packet.X, packet.Y), true, false);
        });
    }

    public async Task<bool> ConnectAsync()
    {
        ConnectResponse response = await base.ConnectAsync(new ConnectRequest() { Name = "TestPlayer" }, 5000);

        if (response is not null && response.Accepted)
        {
            this._playerId = response.PlayerEntityID;
            this._interpolationTime = (1f / response.ServerTickSpeed) * 2f;

            this.EnqueuePacket(new ConnectReadyForData(), true, true, 0);

            while (this._playerId == -1)
            {
                await Task.Delay(1);
            }


            //this._world.MaintainChunkArea(this._renderDistance, this._renderDistance, response.PlayerChunkX, response.PlayerChunkY);

            this.StartLatencyChecking();
            return true;
        }

        return false;
    }

    public TRXStats GetTRXStats()
    {
        return new TRXStats(this._receivedPackets.LockedAction(rp => rp.ToList()), this._sentPackets.LockedAction(rp => rp.ToList()));
    }

    public int GetPing()
    {
        return (int)this._latency.LockedAction((queue) =>
        {
            if (queue.Count > 0)
            {
                return queue.Average();
            }

            return 0;
        });
    }

    private bool TryGetClientSideEntity(int serverEntityID, out Entity clientEntity)
    {
        return this._serverEntityIDToClientEntity.TryGetValue(serverEntityID, out clientEntity);
    }

    public Entity GetPlayerEntity()
    {
        if (TryGetClientSideEntity(this._playerId, out Entity playerEntity))
        {
            return playerEntity;
        }

        return null;
    }

    private void DestroyClientSideEntity(int serverEntityID)
    {
        if (TryGetClientSideEntity(serverEntityID, out Entity clientEntity))
        {
            this._ecs.DestroyEntity(clientEntity.ID);
            this._serverEntityIDToClientEntity.Remove(serverEntityID);
        }
    }

    private void ProcessServerECSUpdates()
    {
        while (true)
        {
            bool done = this._receivedEntityUpdates.LockedAction((rep) =>
            {
                if (rep.Count < 1)
                {
                    return true;
                }

                UpdateEntitiesPacket packet = rep.Dequeue();
                this._serverLastProcessedCommand = packet.LastProcessedCommand;

                foreach (EntityUpdate update in packet.Updates)
                {
                    if (!TryGetClientSideEntity(update.EntityID, out Entity clientEntity))
                    {
                        clientEntity = this._ecs.CreateEntity();
                        this._serverEntityIDToClientEntity.Add(update.EntityID, clientEntity);
                    }

                    // If the update is of the remote player
                    if (update.EntityID == this._playerId)
                    {
                        // This component belongs to me, the client.
                        // I should perform reconciliation.
                        foreach (Component component in update.Components)
                        {
                            if (!clientEntity.HasComponent(component.GetType()))
                                this._ecs.AddComponentToEntity(clientEntity, component);

                            clientEntity.GetComponent(component.GetType()).UpdateComponent(component);
                        }

                        List<UserCommand> commandsAfterLast = this._pendingCommands.Where(c => c.CommandNumber > this._serverLastProcessedCommand).ToList();

                        for (int i = 0; i < commandsAfterLast.Count; i++)
                        {
                            UserCommand command = commandsAfterLast[i];
                            clientEntity.ApplyInput(command, this._world);
                        }
                    }
                    else
                    {
                        // This component belongs to an entity which
                        // is not me, so I should perform interpolation.
                        foreach (Component component in update.Components)
                        {
                            if (!clientEntity.HasComponent(component.GetType()))
                                this._ecs.AddComponentToEntity(clientEntity, component);

                            clientEntity.GetComponent(component.GetType()).PushComponentUpdate(component);
                        }
                    }
                }

                return false;
            });

            if (done)
            {
                break;
            }
        }

        this._pendingCommands.RemoveAll(c => c.CommandNumber <= this._serverLastProcessedCommand);
    }

    private void ProcessInputs()
    {
        List<(GLFW.Keys, int)> inputs = new List<(GLFW.Keys, int)>()
        {
            (GLFW.Keys.W, UserCommand.KEY_W),
            (GLFW.Keys.A, UserCommand.KEY_A),
            (GLFW.Keys.S, UserCommand.KEY_S),
            (GLFW.Keys.D, UserCommand.KEY_D),
            (GLFW.Keys.Space, UserCommand.KEY_SPACE),
            (GLFW.Keys.LeftShift, UserCommand.KEY_SHIFT),
        };

        UserCommand command = new UserCommand();
        command.DeltaTime = GameTime.DeltaTime;
        command.PreviousButtons = this._lastSentCommand == null ? (byte)0 : this._lastSentCommand.Buttons;

        foreach ((GLFW.Keys, byte) input in inputs)
        {
            if (Input.IsKeyDown(input.Item1))
            {
                command.SetKeyDown(input.Item2);
            }
        }

        command.CommandNumber = this._lastSentCommand != null ? this._lastSentCommand.CommandNumber + 1 : 0;

        base.EnqueuePacket(command, false, false, this._fakeLatency);
        this._lastSentCommand = command;

        //Client side prediction
        Entity playerEntity = this.GetPlayerEntity();
        if (playerEntity != null)
        {
            playerEntity.ApplyInput(command, this._world);
            this._pendingCommands.Add(command);
        }
    }

    public void Update()
    {
        this.ProcessServerECSUpdates();

        if (this._playerId == -1 || this.GetPlayerEntity() == null)
        {
            return;
        }

        this.ProcessInputs();

        this.InterpolateEntities();

        this._ecs.Update(null, GameTime.DeltaTime);

        // Entity playerEntity = this.GetPlayerEntity();
        // PlayerPositionComponent ppc = playerEntity.GetComponent<PlayerPositionComponent>();
        // CoordinateVector position = ppc.Position;
        // ChunkAddress chunkPos = position.ToChunkAddress();

        // if (!chunkPos.Equals(this._previousChunkAddress))
        // {
        //     this._previousChunkAddress = chunkPos;
        //     _ = this._world.MaintainChunkAreaAsync(this._renderDistance, this._renderDistance, chunkPos.X, chunkPos.Y);
        // }
    }

    private void InterpolateEntities()
    {
        foreach (Entity entity in this._ecs.GetAllEntities())
        {
            if (entity.ID != this.GetPlayerEntity().ID)
                entity.InterpolateComponents(this._interpolationTime);
        }
    }

    public void Render()
    {
        this._world.Render();
        this._ecs.Render(this._world);
    }
}