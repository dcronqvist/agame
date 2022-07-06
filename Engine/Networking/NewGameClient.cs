using System.Diagnostics;
using AGame.Engine.Configuration;
using AGame.Engine.ECSys;
using AGame.Engine.ECSys.Components;
using AGame.Engine.Graphics;
using AGame.Engine.Graphics.Rendering;
using GameUDPProtocol;

namespace AGame.Engine.Networking;

public class NewGameClient : Client<ConnectRequest, ConnectResponse>
{
    private int _sampleInputRate;
    private float _lastSampleTime;
    private UserCommand _lastSentCommand;

    private ThreadSafe<ECS> _ecs;
    private ThreadSafe<Queue<UpdateEntitiesPacket>> _receivedEntityUpdates;
    private ThreadSafe<List<UserCommand>> _pendingCommands;
    private ThreadSafe<Queue<Packet>> _receivedPackets;
    private ThreadSafe<Queue<Packet>> _sentPackets;
    private int _playerId;

    public NewGameClient(int sampleInputRate, string hostname, int port, int reliableMillisBeforeResend, int timeoutMillis) : base(hostname, port, reliableMillisBeforeResend, timeoutMillis)
    {
        this._sampleInputRate = sampleInputRate;
        ECS ecs = new ECS();
        ecs.Initialize(SystemRunner.Client);
        this._ecs = new ThreadSafe<ECS>(ecs);
        this._receivedEntityUpdates = new ThreadSafe<Queue<UpdateEntitiesPacket>>(new Queue<UpdateEntitiesPacket>());
        this._pendingCommands = new ThreadSafe<List<UserCommand>>(new List<UserCommand>());
        this._receivedPackets = new ThreadSafe<Queue<Packet>>(new Queue<Packet>());
        this._sentPackets = new ThreadSafe<Queue<Packet>>(new Queue<Packet>());
        this._playerId = -1;

        this.RegisterClientEventHandlers();
        this.RegisterPacketHandlers();
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
            //Logging.Log(LogLevel.Debug, $"Client received entity update packet from server");
            this._receivedEntityUpdates.LockedAction((rep) =>
            {
                //Logging.Log(LogLevel.Debug, $"Client received entity update packet {packet.LastProcessedCommand} from server");
                rep.Enqueue(packet);
            });
        });

        base.AddPacketHandler<WelcomePacket>((packet) =>
        {
            //Logging.Log(LogLevel.Debug, $"Client received welcome packet from server");
            this._playerId = packet.ClientId;
        });
    }

    public async Task<bool> ConnectAsync()
    {
        ConnectResponse response = await base.ConnectAsync(new ConnectRequest() { Name = "TestPlayer" }, 5000);

        if (response is not null && response.Accepted)
        {
            this.EnqueuePacket(new ReadyForDataPacket(), true, true, 0);

            while (this._playerId == -1)
            {
                await Task.Delay(1);
            }

            return true;
        }

        return false;
    }

    public int GetRX()
    {
        return this._receivedPackets.LockedAction<int>((rp) => rp.Sum((p) => p.ToBytes().Length));
    }

    public int GetTX()
    {
        return this._sentPackets.LockedAction<int>((rp) => rp.Sum((p) => p.ToBytes().Length));
    }

    private void ProcessServerPackets()
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

                foreach (EntityUpdate update in packet.Updates)
                {
                    this._ecs.LockedAction((ecs) =>
                    {
                        if (!ecs.EntityExists(update.EntityID))
                            ecs.CreateEntity(update.EntityID);

                        Entity entity = ecs.GetEntityFromID(update.EntityID);

                        if (entity.ID == this._playerId)
                        {
                            // This component belongs to me, the client.
                            // I should perform reconciliation.
                            foreach (Component component in update.Components)
                            {
                                if (!entity.HasComponent(component.GetType()))
                                    ecs.AddComponentToEntity(entity, component);

                                entity.GetComponent(component.GetType()).UpdateComponent(component);
                            }

                            List<UserCommand> commandsAfterLast = this._pendingCommands.LockedAction(pc => pc.Where(c => c.CommandNumber > packet.LastProcessedCommand).ToList());

                            for (int i = 0; i < commandsAfterLast.Count; i++)
                            {
                                UserCommand command = commandsAfterLast[i];
                                entity.ApplyInput(command);
                            }

                            this._pendingCommands.LockedAction((pc) =>
                            {
                                pc.RemoveAll(c => c.CommandNumber <= packet.LastProcessedCommand);
                            });
                        }
                        else
                        {
                            // This component belongs to an entity which
                            // is not me, so I should perform interpolation.
                            foreach (Component component in update.Components)
                            {
                                if (!entity.HasComponent(component.GetType()))
                                    ecs.AddComponentToEntity(entity, component);

                                entity.GetComponent(component.GetType()).PushComponentUpdate(component);
                            }
                        }
                    });
                }

                return false;
            });

            if (done)
            {
                break;
            }
        }
    }

    private void ProcessInputs(float delta)
    {
        List<(GLFW.Keys, int)> inputs = new List<(GLFW.Keys, int)>()
        {
            (GLFW.Keys.W, UserCommand.KEY_W),
            (GLFW.Keys.A, UserCommand.KEY_A),
            (GLFW.Keys.S, UserCommand.KEY_S),
            (GLFW.Keys.D, UserCommand.KEY_D),
            (GLFW.Keys.Space, UserCommand.KEY_SPACE),
        };

        UserCommand sendCommand = new UserCommand();
        sendCommand.DeltaTime = delta;

        foreach ((GLFW.Keys, byte) input in inputs)
        {
            if (Input.IsKeyDown(input.Item1))
            {
                sendCommand.SetKeyDown(input.Item2);
            }
        }

        if (sendCommand.Buttons == 0)
        {
            return;
        }

        sendCommand.CommandNumber = this._lastSentCommand != null ? this._lastSentCommand.CommandNumber + 1 : 0;

        base.EnqueuePacket(sendCommand, false, false);
        this._lastSentCommand = sendCommand;

        //Client side prediction
        this._ecs.LockedAction((ecs) =>
        {
            Entity playerEntity = ecs.GetEntityFromID(this._playerId);
            playerEntity.ApplyInput(sendCommand);
        });

        this._pendingCommands.LockedAction((pc) =>
        {
            pc.Add(sendCommand);
        });

    }

    public void Update()
    {
        if (this._playerId == -1)
        {
            return;
        }

        this.ProcessInputs(GameTime.DeltaTime);

        this.ProcessServerPackets();

        this.InterpolateEntities();

        DisplayManager.SetWindowTitle($"RX: {this.GetRX()} TX: {this.GetTX()} PENDING: {this._pendingCommands.Value.Count} LAST: {this._lastSentCommand?.CommandNumber}");
    }

    private void InterpolateEntities()
    {
        this._ecs.LockedAction((ecs) =>
        {
            foreach (Entity entity in ecs.GetAllEntities())
            {
                if (entity.ID != this._playerId)
                    entity.InterpolateComponents();
            }
        });
    }

    public void Render()
    {
        this._ecs.LockedAction((ecs) =>
        {
            List<Entity> entities = ecs.GetAllEntities();

            foreach (Entity entity in entities)
            {
                TransformComponent transform = entity.GetComponent<TransformComponent>();
                ColorComponent cc = entity.GetComponent<ColorComponent>();
                Renderer.Primitive.RenderCircle(transform.Position.ToWorldVector().ToVector2(), 50f, cc.Color, false);
            }
        });
    }
}