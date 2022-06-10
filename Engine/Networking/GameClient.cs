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

            this.world.MaintainChunkAreaAsync(2, 1, this._player.GetComponent<TransformComponent>().GetChunkPosition().X, this._player.GetComponent<TransformComponent>().GetChunkPosition().Y);

            GameConsole.WriteLine("CONNECT", "<0x00FF00>Connected to server</>");
        });
    }

    protected override void HandleInvalidReceive(byte[] data, IPEndPoint remote, Exception e = null)
    {
        GameConsole.WriteLine("CLIENT", $"<0xFF0000>Received invalid packet from {remote}, {e?.Message}</>");
    }

    Vector2i previousChunkPos = new Vector2i(0, 0);

    public void Update()
    {
        ECS.Instance.LockedAction((ecs) =>
        {
            ecs.InterpolateProperties();
        });

        if (connectDone)
        {
            TransformComponent tc = this._player.GetComponent<TransformComponent>();

            Vector2i chunkPos = tc.GetChunkPosition();

            if (!chunkPos.Equals(previousChunkPos))
            {
                // Entered new chunk. Request this one.
                this.world.MaintainChunkAreaAsync(2, 1, this._player.GetComponent<TransformComponent>().GetChunkPosition().X, this._player.GetComponent<TransformComponent>().GetChunkPosition().Y);

                this.previousChunkPos = chunkPos;
            }

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