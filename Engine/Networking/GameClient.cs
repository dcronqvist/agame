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
    WorldContainer world;
    Queue<GroundLayerUpdatePacket> _groundLayerUpdateQueue = new Queue<GroundLayerUpdatePacket>();
    Camera2D _camera;

    Entity _player;

    public GameClient(string host, int port) : base(host, port, 500, 5000)
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

        this.AddPacketHandler<UpdateEntityComponentPacket>((packet) =>
        {
            //GameConsole.WriteLine("CONNECT", $"<0x00FF00>Received component packet from server: Entity {packet.EntityID}, component {packet.ComponentType}</>");

            int entityId = packet.EntityID;

            ECS.Instance.LockedAction((e) =>
            {
                if (!e.EntityExists(entityId))
                {
                    //GameConsole.WriteLine("CONNECT", $"<0xFF0000>Entity {entityId} does not exist, creating it...</>");
                    e.CreateEntity(entityId);
                }

                Entity entity = e.GetEntityFromID(entityId);

                if (!entity.HasComponent(packet.Component.GetType()))
                {
                    //GameConsole.WriteLine("CONNECT", $"<0x00FF00>Adding component {packet.ComponentType} to entity {entityId}</>");
                    e.AddComponentToEntity(entity, packet.Component);
                }
                else
                {
                    //GameConsole.WriteLine("CONNECT", $"<0x00FF00>Updating component {packet.ComponentType} to entity {entityId}</>");

                    entity.GetComponent(packet.Component.GetType()).UpdateComponent(packet.Component);
                }
            });
        });

        this.AddPacketHandler<ConnectFinished>((packet) =>
        {
            connectDone = true;
            this._player = ECS.Instance.Value.GetEntityFromID(packet.PlayerEntityId);
            this._camera = new Camera2D(this._player.GetComponent<TransformComponent>().Position, 2f);
            this.world = new WorldContainer(new ServerWorldGenerator(this), true);

            Input.OnScroll += (sender, e) =>
            {
                this._camera.Zoom *= (e > 0) ? 1.05f : 0.95f;
            };

            _ = Task.Run(async () =>
            {
                while (true)
                {
                    this.EnqueuePacket(new UpdateEntityComponentPacket(this._player.ID, this._player.GetComponent<PlayerInputComponent>()), false, false);
                    await Task.Delay(16);
                }
            });

            _ = Task.Run(async () =>
            {
                while (true)
                {
                    TransformComponent tc = this._player.GetComponent<TransformComponent>();

                    this._camera.FocusPosition = tc.Position;

                    float fx = tc.Position.X > 0 ? tc.Position.X : (tc.Position.X - 1);
                    float fy = tc.Position.Y > 0 ? tc.Position.Y : (tc.Position.Y - 1);

                    int x = (int)MathF.Floor(fx / (Chunk.CHUNK_SIZE * TileGrid.TILE_SIZE));
                    int y = (int)MathF.Floor(fy / (Chunk.CHUNK_SIZE * TileGrid.TILE_SIZE));

                    this.world.GetChunk(x, y);
                    await Task.Delay(100);
                }
            });

            GameConsole.WriteLine("CONNECT", "<0x00FF00>Connected to server</>");
        });
    }

    protected override void HandleInvalidReceive(byte[] data, IPEndPoint remote, Exception e = null)
    {
        GameConsole.WriteLine("CLIENT", $"<0xFF0000>Received invalid packet from {remote}, {e?.Message}</>");
    }

    public void Update()
    {
        ECS.Instance.LockedAction((ecs) =>
        {
            ecs.InterpolateProperties();
        });

        if (connectDone)
        {
            TransformComponent tc = this._player.GetComponent<TransformComponent>();

            this._camera.FocusPosition = tc.Position;
            if (Input.IsKeyDown(GLFW.Keys.W))
            {
                this._player.GetComponent<PlayerInputComponent>().SetKeyDown(PlayerInputComponent.KEY_W);
            }
            else
            {
                this._player.GetComponent<PlayerInputComponent>().SetKeyUp(PlayerInputComponent.KEY_W);
            }

            if (Input.IsKeyDown(GLFW.Keys.A))
            {
                this._player.GetComponent<PlayerInputComponent>().SetKeyDown(PlayerInputComponent.KEY_A);
            }
            else
            {
                this._player.GetComponent<PlayerInputComponent>().SetKeyUp(PlayerInputComponent.KEY_A);
            }

            if (Input.IsKeyDown(GLFW.Keys.S))
            {
                this._player.GetComponent<PlayerInputComponent>().SetKeyDown(PlayerInputComponent.KEY_S);
            }
            else
            {
                this._player.GetComponent<PlayerInputComponent>().SetKeyUp(PlayerInputComponent.KEY_S);
            }

            if (Input.IsKeyDown(GLFW.Keys.D))
            {
                this._player.GetComponent<PlayerInputComponent>().SetKeyDown(PlayerInputComponent.KEY_D);
            }
            else
            {
                this._player.GetComponent<PlayerInputComponent>().SetKeyUp(PlayerInputComponent.KEY_D);
            }

            if (Input.IsKeyDown(GLFW.Keys.Space))
            {
                this._player.GetComponent<PlayerInputComponent>().SetKeyDown(PlayerInputComponent.KEY_SPACE);
            }
            else
            {
                this._player.GetComponent<PlayerInputComponent>().SetKeyUp(PlayerInputComponent.KEY_SPACE);
            }
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
                ecs.Render();
            });
        }
    }
}