using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Numerics;
using System.Threading.Tasks;
using AGame.Engine.Assets;
using AGame.Engine.Configuration;
using AGame.Engine.ECSys;
using AGame.Engine.ECSys.Components;
using AGame.Engine.ECSys.Systems;
using AGame.Engine.Graphics;
using AGame.Engine.Graphics.Rendering;
using AGame.Engine.Items;
using AGame.Engine.World;
using GameUDPProtocol;

namespace AGame.Engine.Networking;

public class TestEncoder : IByteEncoder
{
    // public byte[] Decode(byte[] buffer)
    // {
    //     using (var compressedMs = new MemoryStream(buffer))
    //     {
    //         using (var decompressedMs = new MemoryStream())
    //         {
    //             using (var gzs = new BufferedStream(new GZipStream(compressedMs, CompressionMode.Decompress)))
    //             {
    //                 gzs.CopyTo(decompressedMs);
    //             }
    //             return decompressedMs.ToArray();
    //         }
    //     }
    // }

    // public byte[] Encode(byte[] buffer)
    // {
    //     using (var compressIntoMs = new MemoryStream())
    //     {
    //         using (var gzs = new BufferedStream(new GZipStream(compressIntoMs,
    //          CompressionMode.Compress)))
    //         {
    //             gzs.Write(buffer, 0, buffer.Length);
    //         }
    //         return compressIntoMs.ToArray();
    //     }
    // }
    // public byte[] Decode(byte[] buffer)
    // {
    //     return Utilities.RunLengthDecode(buffer);
    // }

    // public byte[] Encode(byte[] buffer)
    // {
    //     return Utilities.RunLengthEncode(buffer);
    // }
    // public byte[] Decode(byte[] buffer)
    // {
    //     MemoryStream input = new MemoryStream(buffer);
    //     MemoryStream output = new MemoryStream();
    //     using (DeflateStream dstream = new DeflateStream(input, CompressionMode.Decompress))
    //     {
    //         dstream.CopyTo(output);
    //     }
    //     return output.ToArray();
    // }

    // public byte[] Encode(byte[] buffer)
    // {
    //     MemoryStream output = new MemoryStream();
    //     using (DeflateStream dstream = new DeflateStream(output, CompressionLevel.Fastest))
    //     {
    //         dstream.Write(buffer, 0, buffer.Length);
    //     }
    //     return output.ToArray();
    // }
    public byte[] Decode(byte[] buffer)
    {
        return buffer;
    }

    public byte[] Encode(byte[] buffer)
    {
        return buffer;
    }
}
public class GameClient : Client<ConnectRequest, ConnectResponse>
{
    private UserCommand _lastSentCommand;
    private int _serverLastProcessedCommand;
    private int _lastProcessedServerTick;

    private ECS _ecs;
    private WorldContainer _world;
    private List<UserCommand> _pendingCommands;
    private ThreadSafe<Queue<UpdateEntitiesPacket>> _receivedEntityUpdates;
    private ThreadSafe<Queue<Packet>> _receivedPackets;
    private ThreadSafe<Queue<Packet>> _sentPackets;
    private ThreadSafe<Queue<int>> _latency;
    private ThreadSafe<Queue<IClientTickAction>> _nextTickActions;
    private ThreadSafe<Dictionary<ulong, Entity>> _clientPredictedEntities;

    private Dictionary<int, Entity> _serverEntityIDToClientEntity;

    private int _playerId;
    private ChunkAddress _previousChunkAddress;
    private float _interpolationTime;
    private int _fakeLatency;
    private string _hostname;
    private int _port;

    private int _renderDistance = 4;

    public int ReceivedEntityOpenContainer { get; set; }

    public GameClient(string hostname, int port, int reliableMillisBeforeResend, int timeoutMillis) : base(hostname, port, reliableMillisBeforeResend, timeoutMillis, new TestEncoder())
    {
        this._ecs = new ECS();
        this._ecs.Initialize(SystemRunner.Client, gameClient: this);
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
        this._nextTickActions = new ThreadSafe<Queue<IClientTickAction>>(new Queue<IClientTickAction>());
        this.ReceivedEntityOpenContainer = -1;
        this._clientPredictedEntities = new ThreadSafe<Dictionary<ulong, Entity>>(new Dictionary<ulong, Entity>());

        this.RegisterClientEventHandlers();
        this.RegisterPacketHandlers();
    }

    public WorldContainer GetWorld()
    {
        return this._world;
    }

    public ECS GetECS()
    {
        return this._ecs;
    }

    public void SetWorld(WorldContainer world)
    {
        //this._world = world;
    }

    public int GetAmountOfEntities()
    {
        return this._ecs.GetAllEntities().Count;
    }

    private void StartLatencyChecking()
    {
        Task.Run(async () =>
        {
            while (true)
            {
                QueryResponse response = await Client.QueryServerAsync<QueryResponse>(this._hostname, this._port, 5000, this.Encoder);
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
            this._nextTickActions.LockedAction((q) => q.Enqueue(new ClientDestroyClientSideEntity(packet.EntityID)));
        });

        this.AddPacketHandler<ChunkUpdatePacket>((packet) =>
        {
            packet.Chunk.ParentWorld = this._world;
            this._world.UpdateChunk(packet.X, packet.Y, packet.Chunk);
        });

        this.AddPacketHandler<UnloadChunkPacket>((packet) =>
        {
            this._nextTickActions.LockedAction((q) => q.Enqueue(new ClientDiscardChunkAction(packet.X, packet.Y)));
        });

        this.AddPacketHandler<WholeChunkPacket>((packet) =>
        {
            this.EnqueuePacket(new ReceivedChunkPacket(packet.X, packet.Y), true, false);
            packet.Chunk.ParentWorld = this._world;

            this._nextTickActions.LockedAction((q) => q.Enqueue(new ClientUpdateChunkAction(packet.X, packet.Y, packet.Chunk)));
        });

        base.AddPacketHandler<SetContainerContentPacket>((packet) =>
        {
            Logging.Log(LogLevel.Debug, $"Client: Received inventory content packet");
            this._nextTickActions.LockedAction((q) => q.Enqueue(new ClientSetContainerContentAction(packet)));
        });

        base.AddPacketHandler<AcknowledgeClientSideEntityPacket>((packet) =>
        {
            this._clientPredictedEntities.LockedAction((cpe) =>
            {
                this._serverEntityIDToClientEntity.Add(packet.EntityID, cpe[packet.Hash]);
                cpe.Remove(packet.Hash);

                this.EnqueuePacket(new AcknowledgeServerSideEntityPacket() { ServerSideEntityID = packet.EntityID }, true, false);
            });
        });
    }

    public async Task<bool> ConnectAsync(string clientName)
    {
        ConnectResponse response = await base.ConnectAsync(new ConnectRequest() { Name = clientName }, 5000);

        if (response is not null && response.Accepted)
        {
            this._playerId = response.PlayerEntityID;
            this._interpolationTime = (1f / response.ServerTickSpeed) * 2f;

            this._world = new WorldContainer(true, new ServerWorldGenerator(this));

            this.EnqueuePacket(new ConnectReadyForData(), true, true, 0);

            while (this._playerId == -1)
            {
                await Task.Delay(1);
            }

            this.StartLatencyChecking();
            return true;
        }

        return false;
    }

    public TRXStats GetTRXStats()
    {
        return new TRXStats(this._receivedPackets.LockedAction(rp => rp.ToList()), this._sentPackets.LockedAction(rp => rp.ToList()), base.Encoder);
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

    public bool TryGetClientSideEntity(int serverEntityID, out Entity clientEntity)
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

    public void DestroyClientSideEntity(int serverEntityID)
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
                this._lastProcessedServerTick = packet.ServerTick;

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
                            clientEntity.ApplyInput(command, this._world, this._ecs);
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

                // Begin by deleting entities that are part of the packet's DeleteList
                int[] deleteEntities = packet.DeleteEntities;

                foreach (int serverSideEntity in deleteEntities)
                {
                    //Logging.Log(LogLevel.Debug, $"Client: Deleting entity {serverSideEntity} since server told me to.");
                    if (TryGetClientSideEntity(serverSideEntity, out Entity clientEntity))
                    {
                        this._ecs.DestroyEntity(clientEntity.ID);
                        this._serverEntityIDToClientEntity.Remove(serverSideEntity);
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

    private void ProcessInputs(bool allowInput, Vector2i mouseTilePos)
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
        command.MouseTileX = mouseTilePos.X;
        command.MouseTileY = mouseTilePos.Y;
        command.DeltaTime = GameTime.DeltaTime;
        command.PreviousButtons = this._lastSentCommand == null ? (byte)0 : this._lastSentCommand.Buttons;
        command.LastReceivedServerTick = this._lastProcessedServerTick;

        if (allowInput)
        {
            foreach ((GLFW.Keys, byte) input in inputs)
            {
                if (Input.IsKeyDown(input.Item1))
                {
                    command.SetInputDown(input.Item2);
                }
            }

            if (Input.GetScroll() > 0)
            {
                command.SetInputDown(UserCommand.MOUSE_SCROLL_UP);
            }
            else if (Input.GetScroll() < 0)
            {
                command.SetInputDown(UserCommand.MOUSE_SCROLL_DOWN);
            }

            if (Input.IsMouseButtonDown(GLFW.MouseButton.Left))
            {
                command.SetInputDown(UserCommand.USE_ITEM);
            }
        }

        command.CommandNumber = this._lastSentCommand != null ? this._lastSentCommand.CommandNumber + 1 : 0;

        base.EnqueuePacket(command, false, false, this._fakeLatency);
        this._lastSentCommand = command;

        //Client side prediction
        Entity playerEntity = this.GetPlayerEntity();
        if (playerEntity != null)
        {
            playerEntity.ApplyInput(command, this._world, this._ecs);
            this._pendingCommands.Add(command);
        }
    }

    public void Update(bool allowInput, Vector2i mouseTilePos)
    {
        this.ProcessServerECSUpdates();

        if (this._playerId == -1 || this.GetPlayerEntity() == null)
        {
            return;
        }

        this.ProcessInputs(allowInput, mouseTilePos);

        this.InterpolateEntities();

        this._ecs.Update(null, GameTime.DeltaTime);

        this._nextTickActions.LockedAction((actions) =>
        {
            if (actions.Count > 0)
            {
                IClientTickAction action = actions.Dequeue();
                action.Tick(this);
            }
        });

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

    public void RequestOpenContainer(int localEntityID)
    {
        var remoteID = this.GetRemoteIDForEntity(localEntityID);
        base.EnqueuePacket(new RequestViewContainerPacket() { EntityID = remoteID }, true, false);
    }

    public void CloseCurrentContainer()
    {
        base.EnqueuePacket(new CloseContainerPacket(), true, false);
    }

    public void AttemptCreateEntity(string assetName, Action<Entity> onCreated)
    {
        ulong hash = this._clientPredictedEntities.LockedAction((cpe) =>
        {
            var entity = this._ecs.CreateEntityFromAsset(assetName);

            onCreated.Invoke(entity);

            ulong hash = entity.GetHash();

            cpe.Add(hash, entity);
            return hash;
        });

        // Task.Run(async () =>
        // {
        //     await Task.Delay(1000);

        //     this._clientPredictedEntities.LockedAction((cpe) =>
        //     {
        //         if (cpe.TryGetValue(hash, out Entity entity))
        //         {
        //             // This destroys the predicted entity if the server hasn't acked it within 1 second.
        //             this._ecs.DestroyEntity(entity.ID);
        //             cpe.Remove(hash);
        //         }
        //     });
        // });
    }
}