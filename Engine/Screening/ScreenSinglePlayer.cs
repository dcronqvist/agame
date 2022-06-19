using System.Numerics;
using AGame.Engine.ECSys;
using AGame.Engine.Graphics;
using AGame.Engine.Graphics.Rendering;
using AGame.Engine.Networking;
using AGame.Engine.UI;
using AGame.Engine.World;

namespace AGame.Engine.Screening;

public class ScreenSinglePlayer : Screen
{
    public ScreenSinglePlayer() : base("screen_single_player")
    {

    }

    public override Screen Initialize()
    {
        return this;
    }

    public override void OnEnter(string[] args)
    {
        WorldManager.Instance.LoadWorlds();
    }

    public override void OnLeave()
    {
    }

    public override void Update()
    {

    }

    public override void Render()
    {
        Renderer.SetRenderTarget(null, null);
        Renderer.Clear(ColorF.Darken(ColorF.LightGray, 1.05f));

        GUI.Begin();
        WorldMetaData[] worlds = WorldManager.Instance.GetAllWorlds();

        Vector2 middleOfScreen = DisplayManager.GetWindowSizeInPixels() / 2f;
        Vector2 bottomLeft = new Vector2(0, DisplayManager.GetWindowSizeInPixels().Y);

        for (int i = 0; i < worlds.Length; i++)
        {
            if (GUI.Button(worlds[i].Name + " " + worlds[i].CreatedAt.ToShortDateString(), new Vector2(middleOfScreen.X - 200f, middleOfScreen.Y + i * 50f), new Vector2(400f, 40f)))
            {
                _ = this.PlaySinglePlayerWorldAsync(worlds[i], "TestPlayer");
            }
        }

        if (GUI.Button(Localization.GetString("menu.button.back"), new Vector2(10f, bottomLeft.Y - 50f), new Vector2(200f, 40f)))
        {
            ScreenManager.GoToScreen("screen_main_menu");
        }
        GUI.End();
    }

    public async Task PlaySinglePlayerWorldAsync(WorldMetaData world, string playerName)
    {
        await Task.Run(async () =>
        {
            // Go to a loading screen, will do later
            ScreenManager.GoToScreen("screen_temporary_loading");

            WorldContainer container = await world.GetAsContainerAsync();
            List<Entity> entities = await world.GetEntitiesAsync();

            ECS serverECS = new ECS();
            serverECS.Initialize(SystemRunner.Server, entities);

            GameServerConfiguration config = new GameServerConfiguration()
            {
                MaxClients = 1,
                OnlyAllowLocalConnections = true,
                Port = Utilities.GetRandomInt(10000, 50000)
            };

            GameServer gameServer = new GameServer(serverECS, container, world, config);

            ECS clientECS = new ECS();
            clientECS.Initialize(SystemRunner.Client, null);

            GameClient gameClient = new GameClient("127.0.0.1", config.Port);
            ServerWorldGenerator swg = new ServerWorldGenerator(gameClient);

            gameClient.Initialize(clientECS, new WorldContainer(swg, true));

            await gameServer.StartAsync();

            await gameClient.ConnectAsync(playerName); // Cannot fail, as we are connecting to our own host.

            ScreenPlayingSingle sps = ScreenManager.GetScreen<ScreenPlayingSingle>("screen_playing_single");

            sps.Client = gameClient;
            sps.Server = gameServer;

            ScreenManager.GoToScreen("screen_playing_single");
        });
    }
}