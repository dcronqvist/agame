using System.Net;
using System.Numerics;
using AGame.Engine.DebugTools;
using AGame.Engine.ECSys;
using AGame.Engine.ECSys.Components;
using AGame.Engine.Graphics;
using AGame.Engine.Graphics.Cameras;
using AGame.Engine.Graphics.Rendering;
using AGame.Engine.World;
using GameUDPProtocol;

namespace AGame.Engine.Networking;

public class GameClient : Client<ConnectRequest, ConnectResponse>
{
    bool connectDone = false;
    int receivedTiles = 0;
    int[,] tileGrid = new int[100, 100];
    public WorldContainer world;
    Queue<GroundLayerUpdatePacket> _groundLayerUpdateQueue = new Queue<GroundLayerUpdatePacket>();
    Camera2D _camera;
    Entity _player;

    ThreadSafe<Queue<Packet>> receivedPackets = new ThreadSafe<Queue<Packet>>(new Queue<Packet>());
    ThreadSafe<Queue<Packet>> sentPackets = new ThreadSafe<Queue<Packet>>(new Queue<Packet>());

    public GameClient(string host, int port) : base(host, port, 1000, 10000)
    {
        this.Connecting += (sender, e) =>
        {
            GameConsole.WriteLine("CLIENT", "<0x00FFFF>Connecting to " + e.Remote + "</>");
        };

        this.ConnectionAccepted += (sender, e) =>
        {
            GameConsole.WriteLine("CLIENT", "<0x00FF00>Connection accepted from " + e.Connection.RemoteEndPoint + "</>");
        };

        this.ConnectionRejected += (sender, e) =>
        {
            GameConsole.WriteLine("CLIENT", "<0xFF0000>Connection rejected with reason: " + e.Response.RejectReason + "</>");
        };

        this.ServerDisconnectedClient += (sender, e) =>
        {
            Environment.Exit(0);
        };

        this.TimedOut += (sender, e) =>
        {
            GameConsole.WriteLine("CLIENT", "<0xFF0000>Connection timed out</>");
        };

        this.PacketReceived += async (sender, e) =>
        {
            this.receivedPackets.LockedAction((rp) =>
            {
                rp.Enqueue(e.Packet);
            });
            await Task.Delay(1000);
            this.receivedPackets.LockedAction((rp) =>
            {
                rp.Dequeue();
            });
        };

        this.PacketSent += async (sender, e) =>
        {
            this.sentPackets.LockedAction((rp) =>
            {
                rp.Enqueue(e.Packet);
            });
            await Task.Delay(1000);
            this.sentPackets.LockedAction((rp) =>
            {
                rp.Dequeue();
            });
        };

        this.AddPacketHandler<UpdateEntitiesPacket>((packet) =>
        {
            foreach (EntityUpdate update in packet.Updates)
            {
                int entityId = update.EntityID;

                ECS.Instance.LockedAction((e) =>
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

        this.AddPacketHandler<DestroyEntityPacket>((packet) =>
        {
            //GameConsole.WriteLine("CONNECT", $"<0x00FF00>Received destroy entity packet from server: Entity {packet.EntityID}</>");

            int entityId = packet.EntityID;

            ECS.Instance.LockedAction((e) =>
            {
                if (e.EntityExists(entityId))
                {
                    e.DestroyEntity(entityId);
                }
            });
        });

        this.AddPacketHandler<ConnectFinished>((packet) =>
        {
            GameConsole.WriteLine("CLIENT", $"Connected to server with entity ID {packet.PlayerEntityId}");
            this._player = ECS.Instance.Value.GetEntityFromID(packet.PlayerEntityId);
            this._camera = new Camera2D(this._player.GetComponent<TransformComponent>().Position, 2f);
            this.world = new WorldContainer(new ServerWorldGenerator(this), true);
            this.world.Start();

            Input.OnScroll += (sender, e) =>
            {
                this._camera.Zoom *= (e > 0) ? 1.05f : 0.95f;
            };

            this.world.MaintainChunkArea(2, 1, this._player.GetComponent<TransformComponent>().GetChunkPosition().X, this._player.GetComponent<TransformComponent>().GetChunkPosition().Y);

            GameConsole.WriteLine("CONNECT", "<0x00FF00>Connected to server</>");
            connectDone = true;
        });

        this.AddPacketHandler<TriggerECEventPacket>((packet) =>
        {
            ECS.Instance.LockedAction((ecs) =>
            {
                Entity e = ecs.GetEntityFromID(packet.EntityID);

                Type compType = ecs.GetComponentType(packet.ComponentType);
                Component c = e.GetComponent(compType);
                c.TriggerComponentEvent(c.GetEventArgsType(packet.EventID), packet.EventID, packet.EventArgs);
            });
        });

        ThreadSafe<Dictionary<int, Dictionary<int, float>>> lastUpdateTimes = new ThreadSafe<Dictionary<int, Dictionary<int, float>>>(new Dictionary<int, Dictionary<int, float>>());

        ECS.Instance.Value.ComponentChanged += (sender, e) =>
        {
            if (e.Component.HasCNType(CNType.Update, NDirection.ClientToServer))
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
                        this.EnqueuePacket(new UpdateEntitiesPacket(new EntityUpdate(e.Entity.ID, e.Component)), cna.IsReliable, false);

                        entityTimes[ECS.Instance.Value.GetComponentID(e.Component.GetType())] = GameTime.TotalElapsedSeconds;
                    }
                });
            }
        };
    }

    protected override void HandleInvalidReceive(byte[] data, IPEndPoint remote, Exception e = null)
    {
        GameConsole.WriteLine("CLIENT", $"<0xFF0000>Received invalid packet from {remote}, {e?.Message}</>");
    }

    Vector2i previousChunkPos = new Vector2i(0, 0);

    public void Update()
    {
        string tx = "tx=";
        this.sentPackets.LockedAction((sp) =>
        {
            tx += sp.Count.ToString();
        });

        string rx = "rx=";
        this.receivedPackets.LockedAction((rp) =>
        {
            rx += rp.Count.ToString();
        });

        string txAvgSize = "tx avgSize=";
        this.sentPackets.LockedAction((sp) =>
        {
            if (sp.Count > 0)
            {
                txAvgSize += (sp.Sum(p => p.ToBytes().Length) / sp.Count).ToString();
            }
        });

        string rxAvgSize = "rx avgSize=";
        this.receivedPackets.LockedAction((rp) =>
        {
            if (rp.Count > 0)
            {
                rxAvgSize += (rp.Sum(p => p.ToBytes().Length) / rp.Count).ToString();
            }
        });

        DisplayManager.SetWindowTitle(tx + " " + rx + " " + txAvgSize + " " + rxAvgSize);

        ECS.Instance.LockedAction((ecs) =>
        {
            ecs.InterpolateProperties();
            ecs.Update(this.world);
        });

        if (connectDone)
        {
            TransformComponent tc = this._player.GetComponent<TransformComponent>();

            Vector2i chunkPos = tc.GetChunkPosition();

            this.world.Update();

            if (!chunkPos.Equals(previousChunkPos))
            {
                // Entered new chunk. Request this one.
                this.world.MaintainChunkArea(2, 1, this._player.GetComponent<TransformComponent>().GetChunkPosition().X, this._player.GetComponent<TransformComponent>().GetChunkPosition().Y);

                this.previousChunkPos = chunkPos;
            }

            this._camera.FocusPosition = tc.Position;
            if (Input.IsKeyDown(GLFW.Keys.W))
            {
                this._player.GetComponent<KeyboardInputComponent>().SetKeyDown(KeyboardInputComponent.KEY_W);
            }
            else
            {
                this._player.GetComponent<KeyboardInputComponent>().SetKeyUp(KeyboardInputComponent.KEY_W);
            }

            if (Input.IsKeyDown(GLFW.Keys.A))
            {
                this._player.GetComponent<KeyboardInputComponent>().SetKeyDown(KeyboardInputComponent.KEY_A);
            }
            else
            {
                this._player.GetComponent<KeyboardInputComponent>().SetKeyUp(KeyboardInputComponent.KEY_A);
            }

            if (Input.IsKeyDown(GLFW.Keys.S))
            {
                this._player.GetComponent<KeyboardInputComponent>().SetKeyDown(KeyboardInputComponent.KEY_S);
            }
            else
            {
                this._player.GetComponent<KeyboardInputComponent>().SetKeyUp(KeyboardInputComponent.KEY_S);
            }

            if (Input.IsKeyDown(GLFW.Keys.D))
            {
                this._player.GetComponent<KeyboardInputComponent>().SetKeyDown(KeyboardInputComponent.KEY_D);
            }
            else
            {
                this._player.GetComponent<KeyboardInputComponent>().SetKeyUp(KeyboardInputComponent.KEY_D);
            }

            if (Input.IsKeyDown(GLFW.Keys.Space))
            {
                this._player.GetComponent<KeyboardInputComponent>().SetKeyDown(KeyboardInputComponent.KEY_SPACE);
            }
            else
            {
                this._player.GetComponent<KeyboardInputComponent>().SetKeyUp(KeyboardInputComponent.KEY_SPACE);
            }

            this._player.GetComponent<MouseInputComponent>().MousePosition = Input.GetMousePosition(this._camera);
        }
    }

    public void Render()
    {
        Renderer.SetRenderTarget(null, this._camera);
        Renderer.Clear(ColorF.Black);

        if (connectDone)
        {
            this.world?.Render();
            ECS.Instance.LockedAction((ecs) =>
            {
                ecs.Render(this.world);
            });
        }
    }
}