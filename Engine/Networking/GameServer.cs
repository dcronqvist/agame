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
    private Crater _crater;

    public GameServer(int port) : base(port, 500, 100000000)
    {
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

        this.AddPacketHandler<ConnectReadyForMap>(async (packet, connection) =>
        {
            GameConsole.WriteLine("SERVER", "Received connect ready for map from client at " + connection.RemoteEndPoint);

            int[] tileIds = this._crater.GroundLayer.GridOfIDs.Cast<int>().ToArray();

            int amountOfPackets = (int)Math.Ceiling((double)tileIds.Length / 50);

            for (int i = 0; i < amountOfPackets; i++)
            {
                int[] packetData = new int[50];

                for (int j = 0; j < 50; j++)
                {
                    if (i * 50 + j < tileIds.Length)
                    {
                        packetData[j] = tileIds[i * 50 + j];
                    }
                }

                this.EnqueuePacket(new MapDataPacket(packetData), connection, true, true);
                await Task.Delay(10);
            }

            await Task.Delay(1000);

            this.EnqueuePacket(new MapDataFinishedPacket(), connection, true, true);
        });

        this.AddPacketHandler<ConnectReadyForECS>(async (packet, connection) =>
        {
            Entity newPlayer = null;
            _ecs.LockedAction((ecs) =>
            {
                newPlayer = _ecs.Value.CreateEntityFromAsset("entity_player");
            });
            newPlayer.GetComponent<TransformComponent>().Position.TargetValue = Utilities.GetRandomVector2(0, 1000, 0, 1000);

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
    }

    public new async Task StartAsync()
    {
        // Generate world map
        _ecs = new ThreadSafe<ECS>(new ECS());
        _ecs.Value.Initialize();

        GameConsole.WriteLine("SERVER", "Generating world map...");
        _crater = new Crater(100, 100);
        GameConsole.WriteLine("SERVER", "World map generated.");

        await base.StartAsync();

        _ = Task.Run(() =>
        {
            while (true)
            {
                this._connections.LockedAction((conns) =>
                {
                    _ecs.LockedAction((ecs) =>
                    {
                        foreach (Entity e in ecs.GetAllEntities())
                        {
                            foreach (Component c in e.Components)
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

                Thread.Sleep(50);
            }
        });
    }

    public void Update()
    {
        this._ecs.LockedAction((ecs) =>
        {
            ecs.Update();
        });
    }
}