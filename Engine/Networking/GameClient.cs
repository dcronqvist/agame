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
    Crater _crater;
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

        this.AddPacketHandler<MapDataPacket>((packet) =>
        {
            int[] tiles = packet.TileIDs;

            for (int i = 0; i < tiles.Length; i++)
            {
                tileGrid[receivedTiles % 100, receivedTiles / 100] = tiles[i];
                receivedTiles++;
            }

            GameConsole.WriteLine("CLIENT", $"<0x00FF00>Received {tiles.Length} tiles from server. Total: {receivedTiles}</>");
        });

        this.AddPacketHandler<MapDataFinishedPacket>((packet) =>
        {
            GameConsole.WriteLine("CLIENT", $"<0x00FF00>Received map data from server, {receivedTiles} tiles received</>");

            Input.OnScroll += (sender, e) =>
            {
                _camera.Zoom *= e > 0 ? 1.05f : 1 / 1.05f;
            };

            this.EnqueuePacket(new ConnectReadyForECS(), true, true);
        });

        this.AddPacketHandler<ConnectFinished>((packet) =>
        {
            connectDone = true;
            this._player = ECS.Instance.Value.GetEntityFromID(packet.PlayerEntityId);
            this._camera = new Camera2D(this._player.GetComponent<TransformComponent>().Position, 2f);

            _ = Task.Run(async () =>
            {
                while (true)
                {
                    this.EnqueuePacket(new UpdateEntityComponentPacket(this._player.ID, this._player.GetComponent<PlayerInputComponent>()), false, false);
                    await Task.Delay(16);
                }
            });
        });

        this.AddPacketHandler<GroundLayerUpdatePacket>((packet) =>
        {
            GameConsole.WriteLine("CLIENT", $"<0x00FF00>Ground update: x={packet.X}, y={packet.Y}, tileID={packet.TileId}</>");
            this._groundLayerUpdateQueue.Enqueue(packet);
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

        if (connectDone && this._crater == null)
        {
            this._crater = new Crater(tileGrid);
        }

        if (connectDone)
        {
            this._camera.FocusPosition = this._player.GetComponent<TransformComponent>().Position;

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

            while (this._groundLayerUpdateQueue.Count > 0)
            {
                GroundLayerUpdatePacket packet = this._groundLayerUpdateQueue.Dequeue();

                this._crater.GroundLayer.SetTile(packet.X, packet.Y, packet.TileId);
            }
        }
    }

    public void Render()
    {
        if (this._crater != null)
        {
            Renderer.SetRenderTarget(null, _camera);
            Renderer.Clear(ColorF.Black);

            this._crater.Render();

            List<IRenderable> craterRenderables = this._crater.GetRenderables().ToList();

            craterRenderables.Sort((a, b) =>
            {
                if (a.BasePosition.Y > b.BasePosition.Y)
                {
                    return 1;
                }
                else if (a.BasePosition.Y == b.BasePosition.Y)
                {
                    return 0;
                }
                else
                {
                    return -1;
                }
            });

            foreach (IRenderable ir in craterRenderables)
            {
                ir.Render();
            }

            ECS.Instance.LockedAction((ecs) =>
            {
                ecs.Render();
            });
        }

    }
}