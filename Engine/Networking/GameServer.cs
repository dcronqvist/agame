using System.Diagnostics;
using AGame.Engine.Configuration;
using AGame.Engine.ECSys;
using AGame.Engine.ECSys.Components;
using AGame.Engine.Graphics;
using AGame.Engine.Graphics.Rendering;
using AGame.Engine.World;
using GameUDPProtocol;

namespace AGame.Engine.Networking;

public class UserCommand : Packet
{
    public int CommandNumber { get; set; }
    public float DeltaTime { get; set; }

    [PacketPropIgnore]
    public byte PreviousButtons { get; set; }

    public byte Buttons { get; set; }

    public UserCommand()
    {

    }

    public UserCommand(byte previousButtons, float delta, int commandNumber)
    {
        this.PreviousButtons = previousButtons;
        this.DeltaTime = delta;
        this.CommandNumber = commandNumber;
    }

    public UserCommand(byte previousButtons, float delta, byte buttons, int commandNumber)
    {
        this.PreviousButtons = previousButtons;
        this.Buttons = buttons;
        this.DeltaTime = delta;
        this.CommandNumber = commandNumber;
    }

    public static readonly byte KEY_W = 1 << 0;
    public static readonly byte KEY_A = 1 << 1;
    public static readonly byte KEY_S = 1 << 2;
    public static readonly byte KEY_D = 1 << 3;
    public static readonly byte KEY_SPACE = 1 << 4;
    public static readonly byte KEY_SHIFT = 1 << 5;

    public void SetKeyDown(byte key)
    {
        this.Buttons |= key;
    }

    public bool IsKeyDown(int key)
    {
        return (this.Buttons & key) != 0;
    }

    public bool IsKeyPressed(int key)
    {
        return (this.Buttons & key) != 0 && (this.PreviousButtons & key) == 0;
    }
}

public class GameServer : Server<ConnectRequest, ConnectResponse, QueryResponse>
{
    private ThreadSafe<ECS> _ecs;
    private ThreadSafe<Dictionary<Connection, int>> _connectionToPlayerId;
    private ThreadSafe<Queue<(Connection, UserCommand)>> _receivedCommands;
    private ThreadSafe<Dictionary<Connection, UserCommand>> _lastProcessedCommand;
    private List<(Entity, Component)> _updatedComponents;
    private GameServerConfiguration _configuration;

    public GameServer(ECS ecs, GameServerConfiguration config, int reliableMillisBeforeResend, int clientTimeoutMillis) : base(config.Port, reliableMillisBeforeResend, clientTimeoutMillis)
    {
        this._connectionToPlayerId = new ThreadSafe<Dictionary<Connection, int>>(new Dictionary<Connection, int>());
        this._receivedCommands = new ThreadSafe<Queue<(Connection, UserCommand)>>(new Queue<(Connection, UserCommand)>());
        this._lastProcessedCommand = new ThreadSafe<Dictionary<Connection, UserCommand>>(new Dictionary<Connection, UserCommand>());
        this._ecs = new ThreadSafe<ECS>(ecs);
        this._updatedComponents = new List<(Entity, Component)>();

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
                    Entity entity = ecs.CreateEntity();
                    var ppc = new PlayerPositionComponent();
                    ppc.Position = new CoordinateVector(5f, 5f);

                    var color = new ColorComponent();
                    color.Color = Utilities.ChooseUniform(ColorF.Red, ColorF.Blue, ColorF.Green, ColorF.Orange, ColorF.DarkGoldenRod);

                    ecs.AddComponentToEntity(entity, ppc);
                    ecs.AddComponentToEntity(entity, color);
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

                e.Accept(new ConnectResponse() { PlayerEntityID = entity.ID, ServerTickSpeed = this._configuration.TickRate });
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
                connectionToPlayerId.Remove(this._connections.LockedAction((c) => c.Find(conn => conn.RemoteEndPoint == e.Requester)));
            });

            this._lastProcessedCommand.LockedAction((lastProcessedCommand) =>
            {
                lastProcessedCommand.Remove(this._connections.LockedAction((c) => c.Find(conn => conn.RemoteEndPoint == e.Requester)));
            });
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
            this.BroadcastEntireECS(connection);
        });
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
                    entity.ApplyInput(command);
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

    private void SendECSUpdate(Connection connection, List<EntityUpdate> updates)
    {
        int lastProcessedCommand = this._lastProcessedCommand.LockedAction((lastProcessedCommand) =>
        {
            return lastProcessedCommand[connection].CommandNumber;
        });

        UpdateEntitiesPacket uep = new UpdateEntitiesPacket(lastProcessedCommand, updates.ToArray());
        base.EnqueuePacket(uep, connection, false, true);
    }

    private void BroadcastEntireECS(Connection connection)
    {
        List<Entity> entities = this._ecs.LockedAction(x => x.GetAllEntities().ToList());

        List<EntityUpdate> entityUpdates = new List<EntityUpdate>();

        foreach (Entity entity in entities)
        {
            entityUpdates.Add(new EntityUpdate(entity.ID, entity.Components.ToArray()));
        }

        int lastProcessedCommand = this._lastProcessedCommand.LockedAction((lastProcessedCommand) =>
        {
            return lastProcessedCommand[connection].CommandNumber;
        });

        UpdateEntitiesPacket uep = new UpdateEntitiesPacket(lastProcessedCommand, entityUpdates.ToArray());
        base.EnqueuePacket(uep, connection, true, false);
    }

    private void Tick(float deltaTime)
    {
        this.ProcessInputs();

        this._ecs.LockedAction((ecs) =>
        {
            ecs.Update(null, deltaTime);
        });

        List<EntityUpdate> updatesToSend = Utilities.GetPackedEntityUpdatesMaxByteSize(this._updatedComponents, 1024, out List<(Entity, Component)> usedUpdates);

        this._connections.LockedAction((conns) =>
        {
            foreach (Connection conn in conns)
            {
                //this.BroadcastEntireECS(conn);
                this.SendECSUpdate(conn, updatesToSend);
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

    public void Render()
    {
        this._ecs.LockedAction((ecs) =>
        {
            List<Entity> entities = ecs.GetAllEntities();

            foreach (Entity entity in entities)
            {
                PlayerPositionComponent transform = entity.GetComponent<PlayerPositionComponent>();
                Renderer.Primitive.RenderCircle(transform.Position.ToWorldVector().ToVector2(), 30f, ColorF.LightGray, false);
            }
        });
    }
}