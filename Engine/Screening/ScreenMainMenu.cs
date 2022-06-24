using System.Numerics;
using AGame.Engine.Assets;
using AGame.Engine.Configuration;
using AGame.Engine.ECSys;
using AGame.Engine.Graphics;
using AGame.Engine.Graphics.Rendering;
using AGame.Engine.Networking;
using AGame.Engine.UI;
using AGame.Engine.World;
using OpenTK.Audio.OpenAL;

namespace AGame.Engine.Screening;

public class EnterMainMenuArgs : ScreenEnterArgs
{

}

public class ScreenMainMenu : Screen<EnterMainMenuArgs>
{
    public ScreenMainMenu()
    {

    }

    public override void Initialize()
    {
    }

    public override void OnEnter(EnterMainMenuArgs args)
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
        Vector2 middleOfScreen = DisplayManager.GetWindowSizeInPixels() / 2f - new Vector2(0, 200);
        float width = 300f;
        float height = 50f;
        float distance = 10f;

        if (GUI.Button(Localization.GetString("screen.main_menu.button.singleplayer"), new Vector2(middleOfScreen.X - width / 2f, middleOfScreen.Y), new Vector2(width, height)))
        {
            ScreenManager.GoToScreen<ScreenSelectWorld, EnterSelectWorldArgs>(new EnterSelectWorldArgs(async (world) =>
            {
                await Task.Run(async () =>
                {
                    // Go to a loading screen, will do later
                    ScreenManager.GoToScreen<ScreenTemporaryLoading, EnterTemporaryLoading>(new EnterTemporaryLoading() { Text = "Loading world..." });

                    WorldContainer container = await world.GetAsContainerAsync();
                    List<Entity> entities = await world.GetEntitiesAsync();

                    ECS serverECS = new ECS();
                    serverECS.Initialize(SystemRunner.Server, entities);

                    GameServerConfiguration config = new GameServerConfiguration()
                    {
                        MaxClients = 1,
                        OnlyAllowLocalConnections = true,
                        Port = 28000,
                        EntityViewDistance = 20
                    };

                    GameServer gameServer = new GameServer(serverECS, container, world, config);

                    ECS clientECS = new ECS();
                    clientECS.Initialize(SystemRunner.Client, null);

                    GameClient gameClient = new GameClient("127.0.0.1", config.Port);
                    ServerWorldGenerator swg = new ServerWorldGenerator(gameClient);

                    gameClient.Initialize(clientECS, new WorldContainer(swg, true));

                    await gameServer.StartAsync();

                    await gameClient.ConnectAsync(ScreenManager.Args[0]); // Cannot fail, as we are connecting to our own host.

                    ScreenManager.GoToScreen<ScreenPlayingWorld, EnterPlayingWorldArgs>(new EnterPlayingWorldArgs() { Server = gameServer, Client = gameClient });
                });
            }));
        }

        if (GUI.Button(Localization.GetString("screen.main_menu.button.multiplayer"), new Vector2(middleOfScreen.X - width / 2f, middleOfScreen.Y + height + distance), new Vector2(width, height)))
        {
            ScreenManager.GoToScreen<ScreenMultiplayerMenu, EnterMultiplayerMenuArgs>(new EnterMultiplayerMenuArgs());
        }

        if (GUI.Button(Localization.GetString("screen.main_menu.button.exit"), new Vector2(middleOfScreen.X - width / 2f, middleOfScreen.Y + height * 2 + distance * 2), new Vector2(width, height)))
        {
            Environment.Exit(0);
        }

        if (GUI.Button("Play Test Sound", new Vector2(middleOfScreen.X - width / 2f, middleOfScreen.Y + height * 3 + distance * 3), new Vector2(width, height)))
        {
            Audio.Play("audio_click", Utilities.GetRandomFloat(0.8f, 1.2f));
        }

        if (GUI.Button("Play Test Sound 2", new Vector2(middleOfScreen.X - width / 2f, middleOfScreen.Y + height * 4 + distance * 4), new Vector2(width, height)))
        {
            Audio.Play("audio_click_2", Utilities.GetRandomFloat(0.8f, 1.2f));
        }

        float volume = Settings.GetSetting<float>("volume_master");
        if (GUI.Slider("Master Volume", new Vector2(middleOfScreen.X - width / 2f, middleOfScreen.Y + height * 5 + distance * 5), new Vector2(width, height), ref volume))
        {
            AL.Listener(ALListenerf.Gain, volume);
            _ = Settings.SetSettingAsync("volume_master", volume);
        }

        Locale[] locales = Localization.GetAvailableLocales();
        int currentLocale = Array.IndexOf(locales, Localization.GetLocale());
        string[] localeNames = locales.Select(x => x.LocaleName).ToArray();
        if (GUI.Dropdown(localeNames, new Vector2(20, 20), new Vector2(200, 40), ref currentLocale))
        {
            Localization.SetLocale(locales[currentLocale].Name, prepend: false);
        }
        GUI.End();
    }

    public override void Update()
    {
    }
}