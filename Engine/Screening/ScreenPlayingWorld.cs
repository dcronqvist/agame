using System.Drawing;
using System.Numerics;
using AGame.Engine.Assets;
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
}

public class ScreenPlayingWorld : Screen<EnterPlayingWorldArgs>
{
    private GameServer _server;
    private GameClient _client;
    public Camera2D Camera { get; set; }

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

        _client.PacketReceived += (sender, e) =>
        {
            _receivedPackets.Add(e.Packet);

            if (_receivedPackets.Count > 40)
            {
                _receivedPackets.RemoveAt(0);
            }
        };

        _client.ServerDisconnectedClient += (sender, e) =>
        {
            Task.Run(async () =>
            {
                ScreenManager.GoToScreen<ScreenTemporaryLoading, EnterTemporaryLoading>(new EnterTemporaryLoading() { Text = "Disconnected from server..." });
                await Task.Delay(1000);
                ScreenManager.GoToScreen<ScreenJoinWorld, EnterJoinWorldArgs>(new EnterJoinWorldArgs());
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
        };
    }

    public override void OnLeave()
    {

    }

    public override void Render()
    {
        this.Camera.FocusPosition = this._client.GetPlayerEntity().GetComponent<TransformComponent>().Position.ToWorldVector().ToVector2();

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

        // Renderer.SetRenderTarget(null, null);
        // Font f = AssetManager.GetAsset<Font>("font_rainyhearts");

        // Vector2 top = new Vector2(20, 20);
        // for (int i = this._receivedPackets.Count - 1; i > 0; i--)
        // {
        //     // Start at the top of the screen and move down, 0th index always at the top
        //     Vector2 pos = top + new Vector2(0, (this._receivedPackets.Count - 1 - i) * 20);
        //     Packet p = this._receivedPackets[i];

        //     Renderer.Text.RenderText(f, p.ToString(), pos, 1.2f, ColorF.White, Renderer.Camera);
        // }
    }

    public override void Update()
    {
        this._server?.Update();
        this._client.Update(this.Camera, !this._paused);

        if (Input.IsKeyPressed(GLFW.Keys.Escape))
        {
            this._paused = !this._paused;
        }

        int rx = this._client.GetRX();
        int tx = this._client.GetTX();

        DisplayManager.SetWindowTitle($"RX: {rx} TX: {tx}");
    }

    public async Task ExitWorld()
    {
        await Task.Run(async () =>
        {
            ScreenManager.GoToScreen<ScreenTemporaryLoading, EnterTemporaryLoading>(new EnterTemporaryLoading() { Text = "Closing world..." });
            await this._client.DisconnectAsync(1000);

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