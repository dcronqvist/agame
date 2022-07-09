using System.Numerics;
using AGame.Engine.ECSys;
using AGame.Engine.Graphics;
using AGame.Engine.Graphics.Rendering;
using AGame.Engine.Networking;
using AGame.Engine.UI;
using AGame.Engine.World;

namespace AGame.Engine.Screening;

public class EnterJoinWorldArgs : ScreenEnterArgs { }

public class ScreenJoinWorld : Screen<EnterJoinWorldArgs>
{
    private string _ip;
    private string _port;

    public override void Initialize()
    {

    }

    public override void OnEnter(EnterJoinWorldArgs args)
    {
        _ip = "";
        _port = "";
    }

    public override void OnLeave()
    {
    }

    public override void Render()
    {
        Renderer.SetRenderTarget(null, null);
        Renderer.Clear(ColorF.Darken(ColorF.LightGray, 1.05f));

        GUI.Begin();

        Vector2 middleOfScreen = DisplayManager.GetWindowSizeInPixels() / 2f - new Vector2(0, 200f);
        float width = 400f;

        GUI.TextField("ip", middleOfScreen + new Vector2(-width / 2f, 0), new Vector2(width, 40f), ref _ip);
        GUI.TextField("port", middleOfScreen + new Vector2(-width / 2f, 50f), new Vector2(width, 40f), ref _port);

        if (GUI.Button("Join", middleOfScreen + new Vector2(-width / 2f, 100f), new Vector2(width, 40f)))
        {
            Task.Run(async () =>
            {
                // Go to a loading screen, will do later
                ScreenManager.GoToScreen<ScreenTemporaryLoading, EnterTemporaryLoading>(new EnterTemporaryLoading() { Text = "Loading world..." });

                GameClient gameClient = new GameClient(Utilities.ResolveIPOrDomain(this._ip), int.Parse(this._port), 500, 5000);

                bool connected = await gameClient.ConnectAsync();

                if (connected) // Cannot fail, as we are connecting to our own host.
                {
                    ScreenManager.GoToScreen<ScreenPlayingWorld, EnterPlayingWorldArgs>(new EnterPlayingWorldArgs() { Client = gameClient });
                }
                else
                {
                    _ = Task.Run(async () =>
                    {
                        ScreenManager.GoToScreen<ScreenTemporaryLoading, EnterTemporaryLoading>(new EnterTemporaryLoading() { Text = "Failed to connect to server." });
                        await Task.Delay(2000);
                        ScreenManager.GoToScreen<ScreenJoinWorld, EnterJoinWorldArgs>(new EnterJoinWorldArgs());
                    });
                }
            });
        }

        if (GUI.Button(Localization.GetString("menu.button.back"), new Vector2(10f, DisplayManager.GetWindowSizeInPixels().Y - 50f), new Vector2(200f, 40f)))
        {
            ScreenManager.GoToScreen<ScreenMultiplayerMenu, EnterMultiplayerMenuArgs>(new EnterMultiplayerMenuArgs());
        }

        GUI.End();
    }

    public override void Update()
    {
    }
}