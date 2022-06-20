using System.Drawing;
using System.Numerics;
using AGame.Engine.ECSys.Components;
using AGame.Engine.Graphics;
using AGame.Engine.Graphics.Cameras;
using AGame.Engine.Graphics.Rendering;
using AGame.Engine.Networking;
using AGame.Engine.UI;
using AGame.Engine.World;

namespace AGame.Engine.Screening;

public class ScreenPlayingSingle : Screen
{
    public GameServer Server { get; set; }
    public GameClient Client { get; set; }
    public Camera2D Camera { get; set; }

    bool _paused = false;

    public ScreenPlayingSingle() : base("screen_playing_single")
    {
    }

    public override Screen Initialize()
    {
        return this;
    }

    public override void OnEnter(string[] args)
    {
        _paused = false;
        Camera = new Camera2D(Vector2.Zero, 2f);

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
        this.Camera.FocusPosition = this.Client.GetPlayerEntity().GetComponent<TransformComponent>().Position.ToWorldVector().ToVector2();

        Renderer.SetRenderTarget(null, this.Camera);
        Renderer.Clear(ColorF.Black);
        Client.Render();

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
                _ = this.ExitSinglePlay();
            }
            GUI.End();
        }
    }

    public override void Update()
    {
        this.Server.Update();
        this.Client.Update(this.Camera, !this._paused);

        if (Input.IsKeyPressed(GLFW.Keys.Escape))
        {
            this._paused = !this._paused;
        }

        int rx = this.Client.GetRX();
        int tx = this.Client.GetTX();

        DisplayManager.SetWindowTitle($"RX: {rx} TX: {tx}");
    }

    public async Task ExitSinglePlay()
    {
        await Task.Run(async () =>
        {
            ScreenManager.GoToScreen("screen_temporary_loading");
            await this.Client.DisconnectAsync(1000);

            await Task.Delay(1000);

            await this.Server.StopAsync(1000);

            // Save the world
            this.Server.SaveServer();

            ScreenManager.GoToScreen("screen_main_menu");
        });
    }
}