using System.Numerics;
using AGame.Engine.ECSys;
using AGame.Engine.Graphics;
using AGame.Engine.Graphics.Rendering;
using AGame.Engine.Networking;
using AGame.Engine.UI;
using AGame.Engine.World;

namespace AGame.Engine.Screening;

public class EnterMultiplayerMenuArgs : ScreenEnterArgs { }

public class ScreenMultiplayerMenu : Screen<EnterMultiplayerMenuArgs>
{
    public override void Initialize()
    {

    }

    public override void OnEnter(EnterMultiplayerMenuArgs args)
    {
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

        if (GUI.Button("Host Game", new Vector2(middleOfScreen.X - 200f, middleOfScreen.Y), new Vector2(400f, 40f)))
        {
            ScreenManager.GoToScreen<ScreenSelectWorld, EnterSelectWorldArgs>(new EnterSelectWorldArgs(async (world) =>
            {
                await Task.Run(async () =>
                {
                    // Go to a loading screen, will do later
                    ScreenManager.GoToScreen<ScreenTemporaryLoading, EnterTemporaryLoading>(new EnterTemporaryLoading() { Text = "Loading world..." });

                    ECS serverECS = new ECS();
                    serverECS.Initialize(SystemRunner.Server);

                    GameServerConfiguration config = new GameServerConfiguration();
                    config.SetPort(28000).SetMaxConnections(10).SetOnlyAllowLocalConnections(false).SetTickRate(20);

                    GameServer gameServer = new GameServer(serverECS, config, 500, 5000);

                    ECS clientECS = new ECS();
                    clientECS.Initialize(SystemRunner.Client);

                    GameClient gameClient = new GameClient("127.0.0.1", 28000, 500, 5000);

                    await gameServer.StartAsync();
                    _ = gameServer.RunAsync();
                    await gameClient.ConnectAsync(); // Cannot fail, as we are connecting to our own host.

                    ScreenManager.GoToScreen<ScreenPlayingWorld, EnterPlayingWorldArgs>(new EnterPlayingWorldArgs() { Server = gameServer, Client = gameClient });
                });
            }));
        }
        if (GUI.Button("Join Game", new Vector2(middleOfScreen.X - 200f, middleOfScreen.Y + 50f), new Vector2(400f, 40f)))
        {
            ScreenManager.GoToScreen<ScreenJoinWorld, EnterJoinWorldArgs>(new EnterJoinWorldArgs());
        }

        if (GUI.Button(Localization.GetString("menu.button.back"), new Vector2(10f, DisplayManager.GetWindowSizeInPixels().Y - 50f), new Vector2(200f, 40f)))
        {
            ScreenManager.GoToScreen<ScreenMainMenu, EnterMainMenuArgs>(new EnterMainMenuArgs());
        }

        GUI.End();
    }

    public override void Update()
    {
    }
}