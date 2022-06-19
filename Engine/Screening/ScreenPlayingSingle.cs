using System.Drawing;
using System.Numerics;
using AGame.Engine.Graphics;
using AGame.Engine.Graphics.Rendering;
using AGame.Engine.Networking;
using AGame.Engine.UI;

namespace AGame.Engine.Screening;

public class ScreenPlayingSingle : Screen
{
    public GameServer Server { get; set; }
    public GameClient Client { get; set; }

    bool _paused = false;

    public ScreenPlayingSingle() : base("screen_playing_single")
    {
    }

    public override Screen Initialize()
    {
        return this;
    }

    public override async void OnEnter(string[] args)
    {
        _paused = false;

        await Server.StartAsync();

        ConnectResponse response = await Client.ConnectAsync(new ConnectRequest() { Name = ScreenManager.Args[0] });
        if (!(response is null || !response.Accepted))
        {
            Client.EnqueuePacket(new ConnectReadyForECS(), false, false);
        }
    }

    public override async void OnLeave()
    {
        await this.Client.DisconnectAsync(1000);
        await this.Server.StopAsync(1000);

        Console.WriteLine("Server stopped");
    }

    public override void Render()
    {
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
                ScreenManager.GoToScreen("screen_main_menu");
            }
            GUI.End();
        }
    }

    public override void Update()
    {
        this.Server.Update();
        this.Client.Update();

        if (Input.IsKeyPressed(GLFW.Keys.Escape))
        {
            this._paused = !this._paused;
        }
    }
}