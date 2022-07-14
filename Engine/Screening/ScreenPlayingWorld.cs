using System.Drawing;
using System.Numerics;
using AGame.Engine.Assets;
using AGame.Engine.ECSys;
using AGame.Engine.ECSys.Components;
using AGame.Engine.Graphics;
using AGame.Engine.Graphics.Cameras;
using AGame.Engine.Graphics.Rendering;
using AGame.Engine.Networking;
using AGame.Engine.UI;
using AGame.Engine.World;
using GameUDPProtocol;

namespace AGame.Engine.Screening;

public class EnterPlayingWorldArgs : ScreenEnterArgs
{
    public GameServer Server { get; set; } = null;
    public GameClient Client { get; set; } = null;
    public int PlayerEntityID { get; set; } = -1;
}

public class ScreenPlayingWorld : Screen<EnterPlayingWorldArgs>
{
    private bool _disconnected = false;
    private GameServer _server;
    private GameClient _client;
    public Camera2D Camera { get; set; }
    private Vector2 _cameraTargetPosition;

    List<Packet> _receivedPackets = new List<Packet>();

    bool _paused = false;

    public override void Initialize()
    {

    }

    public override void OnEnter(EnterPlayingWorldArgs args)
    {
        _paused = false;
        Camera = new Camera2D(Vector2.Zero, 2f);
        _server = args.Server;
        _client = args.Client;

        _client.ServerDisconnectedClient += (sender, e) =>
        {
            Task.Run(async () =>
            {
                ScreenManager.GoToScreen<ScreenTemporaryLoading, EnterTemporaryLoading>(new EnterTemporaryLoading() { Text = "Disconnected from server..." });
                await Task.Delay(1000);
                ScreenManager.GoToScreen<ScreenMainMenu, EnterMainMenuArgs>(new EnterMainMenuArgs());
            });
        };

        Input.OnScroll += (sender, e) =>
        {
            if (e > 0)
            {
                Camera.Zoom *= 1.05f;
            }
            else
            {
                Camera.Zoom *= 1 / 1.05f;
            }

            Camera.Zoom = Utilities.Clamp(1.3f, 3f, Camera.Zoom);
        };
    }

    public override async void OnLeave()
    {
        if (!this._disconnected)
        {
            await this.ExitWorld();
        }
    }

    public void SetCameraPosition(CoordinateVector position, bool snap = false)
    {
        if (snap)
        {
            this.Camera.FocusPosition = position.ToWorldVector().ToVector2();
            this._cameraTargetPosition = position.ToWorldVector().ToVector2();
        }
        else
        {
            this._cameraTargetPosition = position.ToWorldVector().ToVector2();
        }
    }

    public override void Render()
    {
        this.Camera.FocusPosition += (this._cameraTargetPosition - this.Camera.FocusPosition) * GameTime.DeltaTime * 7f;

        Renderer.SetRenderTarget(null, this.Camera);
        Renderer.Clear(ColorF.Black);
        _client.Render();

        if (_paused)
        {
            GUI.Begin();

            // Render pause screen
            Renderer.SetRenderTarget(null, null);
            Renderer.Primitive.RenderRectangle(new RectangleF(0, 0, DisplayManager.GetWindowSizeInPixels().X, DisplayManager.GetWindowSizeInPixels().Y), ColorF.Black * 0.6f);

            Vector2 middleOfScreen = DisplayManager.GetWindowSizeInPixels() / 2;

            if (GUI.Button("Back to game", new Vector2(middleOfScreen.X - 150, middleOfScreen.Y - 50), new Vector2(300, 50f)))
            {
                _paused = false;
            }
            if (GUI.Button("Exit to menu", new Vector2(middleOfScreen.X - 150, middleOfScreen.Y + 10f), new Vector2(300, 50f)))
            {
                _ = this.ExitWorld();
            }

            GUI.End();
        }

        Renderer.SetRenderTarget(null, null);
        TRXStats stats = this._client.GetTRXStats();

        Font f = ModManager.GetAsset<Font>("default.font.rainyhearts");
        Renderer.Text.RenderText(f, $"RX: {stats.GetRXBytesString()}", new Vector2(20, 20), 1f, ColorF.White, Renderer.Camera);
        Renderer.Text.RenderText(f, $"TX: {stats.GetTXBytesString()}", new Vector2(20, 40), 1f, ColorF.White, Renderer.Camera);
        Renderer.Text.RenderText(f, $"Ping: {this._client.GetPing()}ms", new Vector2(20, 60), 1f, ColorF.White, Renderer.Camera);

        if (this._client.GetPlayerEntity() != null)
        {
            Entity localPlayer = this._client.GetPlayerEntity();
            int remotePlayerID = this._client.GetRemoteIDForEntity(localPlayer.ID);
            Renderer.Text.RenderText(f, $"RemotePlayerID: {remotePlayerID}", new Vector2(20, 80), 1f, ColorF.White, Renderer.Camera);

            CoordinateVector position = localPlayer.GetComponent<PlayerPositionComponent>().Position;
            Renderer.Text.RenderText(f, $"X: {MathF.Round(position.X, 1)} Y: {MathF.Round(position.Y, 1)}", new Vector2(200, 80), 1f, ColorF.White, Renderer.Camera);

            this.SetCameraPosition(localPlayer.GetComponent<PlayerPositionComponent>().Position, false);
        }

        foreach ((Type t, int b) in stats.ComponentUpdatesReceivedBytesByType)
        {
            Renderer.Text.RenderText(f, $"{t.Name}: {b}", new Vector2(20, 100 + 20 * stats.ComponentUpdatesReceivedBytesByType.IndexOf((t, b))), 1f, ColorF.White, Renderer.Camera);
        }
    }

    public override void Update()
    {
        // Set audio's listener's position to player's position
        //Audio.SetListenerPosition(this._client.GetPlayerEntity().GetComponent<TransformComponent>().Position);

        //this._server?.Update();
        this._client.Update();

        if (Input.IsKeyPressed(GLFW.Keys.Escape))
        {
            this._paused = !this._paused;
        }
    }

    public async Task ExitWorld()
    {
        await Task.Run(async () =>
        {
            ScreenManager.GoToScreen<ScreenTemporaryLoading, EnterTemporaryLoading>(new EnterTemporaryLoading() { Text = "Closing world..." });
            await this._client.DisconnectAsync(1000);
            this._disconnected = true;

            await Task.Delay(1000);

            if (this._server is not null)
            {
                await this._server.StopAsync(1000);

                // Save the world
                this._server.SaveServer();
            }

            ScreenManager.GoToScreen<ScreenMainMenu, EnterMainMenuArgs>(new EnterMainMenuArgs());
        });
    }
}