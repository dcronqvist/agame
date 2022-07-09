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
                // await Task.Run(async () =>
                // {
                //     // Go to a loading screen, will do later
                //     ScreenManager.GoToScreen<ScreenTemporaryLoading, EnterTemporaryLoading>(new EnterTemporaryLoading() { Text = "Starting world..." });

                //     WorldContainer container = await world.GetAsContainerAsync();
                //     List<Entity> entities = await world.GetEntitiesAsync();

                //     ECS serverECS = new ECS();
                //     serverECS.Initialize(SystemRunner.Server, entities);

                //     GameServerConfiguration config = new GameServerConfiguration()
                //     {
                //         MaxClients = 20,
                //         OnlyAllowLocalConnections = false,
                //         Port = 28000,
                //         EntityViewDistance = 20
                //     };

                //     GameServer gameServer = new GameServer(serverECS, container, world, config);

                //     ECS clientECS = new ECS();
                //     clientECS.Initialize(SystemRunner.Client, null);

                //     GameClient gameClient = new GameClient("127.0.0.1", config.Port);
                //     ServerWorldGenerator swg = new ServerWorldGenerator(gameClient);

                //     gameClient.Initialize(clientECS, new WorldContainer(swg, true));

                //     await gameServer.StartAsync();

                //     await gameClient.ConnectAsync(ScreenManager.Args[0]); // Cannot fail, as we are connecting to our own host.

                //     ScreenManager.GoToScreen<ScreenPlayingWorld, EnterPlayingWorldArgs>(new EnterPlayingWorldArgs() { Server = gameServer, Client = gameClient });
                // });
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