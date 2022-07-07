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
    private UserCommand _lastSentCommand;

    private ECS _ecs;
    private List<UserCommand> _pendingCommands;
    private Dictionary<int, ECSSnapshot> _pendingSnapshots;

    private ThreadSafe<Queue<UpdateEntitiesPacket>> _receivedEntityUpdates;
    private ThreadSafe<Queue<Packet>> _receivedPackets;
    private ThreadSafe<Queue<Packet>> _sentPackets;

    private int _playerId;
    private float _interpolationTime;
    private int _fakeLatency;

    public NewGameClient(string hostname, int port, int reliableMillisBeforeResend, int timeoutMillis) : base(hostname, port, reliableMillisBeforeResend, timeoutMillis)
    {
        this._ecs = new ECS();
        this._ecs.Initialize(SystemRunner.Client);
        this._fakeLatency = 0;

        this._receivedEntityUpdates = new ThreadSafe<Queue<UpdateEntitiesPacket>>(new Queue<UpdateEntitiesPacket>());
        this._pendingCommands = new List<UserCommand>();
        this._pendingSnapshots = new Dictionary<int, ECSSnapshot>();
        this._receivedPackets = new ThreadSafe<Queue<Packet>>(new Queue<Packet>());
        this._sentPackets = new ThreadSafe<Queue<Packet>>(new Queue<Packet>());
        this._playerId = -1;

        this.RegisterClientEventHandlers();
        this.RegisterPacketHandlers();
    }

    public void SetFakelatency(int milliseconds)
    {
        this._fakeLatency = milliseconds;
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

                    if (!this._ecs.EntityExists(update.EntityID))
                        this._ecs.CreateEntity(update.EntityID);

                    Entity entity = this._ecs.GetEntityFromID(update.EntityID);

                    if (entity.ID == this._playerId)
                    {
                        // if (this._pendingSnapshots.ContainsKey(packet.LastProcessedCommand))
                        // {
                        //     ECSSnapshot snapshot = this._pendingSnapshots[packet.LastProcessedCommand];
                        //     this._ecs.RestoreSnapshot(snapshot);
                        // }

                        // This component belongs to me, the client.
                        // I should perform reconciliation.
                        foreach (Component component in update.Components)
                        {
                            if (!entity.HasComponent(component.GetType()))
                                this._ecs.AddComponentToEntity(entity, component);

                            entity.GetComponent(component.GetType()).UpdateComponent(component);
                        }

                        List<UserCommand> commandsAfterLast = this._pendingCommands.Where(c => c.CommandNumber > packet.LastProcessedCommand).ToList();

                        for (int i = 0; i < commandsAfterLast.Count; i++)
                        {
                            UserCommand command = commandsAfterLast[i];
                            entity.ApplyInput(command);
                        }

                        List<UserCommand> toBeDeleted = this._pendingCommands.Where(c => c.CommandNumber <= packet.LastProcessedCommand).ToList();
                        this._pendingCommands.RemoveAll(c => c.CommandNumber <= packet.LastProcessedCommand);

                        foreach (UserCommand command in toBeDeleted)
                        {
                            this._pendingSnapshots.Remove(command.CommandNumber);
                        }
                    }
                    else
                    {
                        // This component belongs to an entity which
                        // is not me, so I should perform interpolation.
                        foreach (Component component in update.Components)
                        {
                            if (!entity.HasComponent(component.GetType()))
                                this._ecs.AddComponentToEntity(entity, component);

                            entity.GetComponent(component.GetType()).PushComponentUpdate(component);
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
        };

        UserCommand command = new UserCommand();
        command.DeltaTime = GameTime.DeltaTime;

        foreach ((GLFW.Keys, byte) input in inputs)
        {
            if (Input.IsKeyDown(input.Item1))
            {
                command.SetKeyDown(input.Item2);
            }
        }

        // if (command.Buttons == 0)
        // {
        //     return;
        // }

        command.CommandNumber = this._lastSentCommand != null ? this._lastSentCommand.CommandNumber + 1 : 0;

        base.EnqueuePacket(command, false, false, this._fakeLatency);
        this._lastSentCommand = command;

        //Client side prediction
        Entity playerEntity = this._ecs.GetEntityFromID(this._playerId);
        if (playerEntity != null)
        {

            playerEntity.ApplyInput(command);

            this._pendingCommands.Add(command);
            this._pendingSnapshots.Add(command.CommandNumber, this._ecs.GetSnapshot());
        }
    }

    public void Update()
    {
        if (this._playerId == -1)
        {
            return;
        }

        this.ProcessServerPackets();

        this.ProcessInputs();

        this.InterpolateEntities();

        this._ecs.Update(null, GameTime.DeltaTime);

        DisplayManager.SetWindowTitle($"RX: {this.GetRX()} TX: {this.GetTX()} PENDING: {this._pendingCommands.Count} LAST: {this._lastSentCommand?.CommandNumber} PENDING ECS: {this._pendingSnapshots.Count} FAKE_LATENCY: {this._fakeLatency}");
    }

    private void InterpolateEntities()
    {
        foreach (Entity entity in this._ecs.GetAllEntities())
        {
            if (entity.ID != this._playerId)
                entity.InterpolateComponents(this._interpolationTime);
        }
    }

    public void Render()
    {
        List<Entity> entities = this._ecs.GetAllEntities();

        foreach (Entity entity in entities)
        {
            PlayerPositionComponent transform = entity.GetComponent<PlayerPositionComponent>();
            ColorComponent cc = entity.GetComponent<ColorComponent>();
            Renderer.Primitive.RenderCircle(transform.Position.ToWorldVector().ToVector2(), 50f, cc.Color, false);
        }
    }
}