using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Numerics;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using AGame.Engine.Assets;
using AGame.Engine.Assets.Scripting;
using AGame.Engine.Configuration;
using AGame.Engine.ECSys;
using AGame.Engine.ECSys.Components;
using AGame.Engine.Graphics;
using AGame.Engine.Graphics.Rendering;
using AGame.Engine.Items;
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
        ECS ecs = new ECS();
        ecs.Initialize(SystemRunner.Client);

        var e = ecs.CreateEntityFromAsset("default.entity.test_rock");
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
            ScreenManager.GoToScreen<ScreenSelectWorld, EnterSelectWorldArgs>(new EnterSelectWorldArgs((world) =>
            {
                ScreenManager.GoToScreen<ScreenSelectName, EnterSelectNameArgs>(new EnterSelectNameArgs(async (name) =>
                {
                    await Task.Run(async () =>
                    {
                        // Go to a loading screen, will do later
                        ScreenManager.GoToScreen<ScreenTemporaryLoading, EnterTemporaryLoading>(new EnterTemporaryLoading() { Text = "Loading world..." });

                        WorldContainer wc = await world.GetAsContainerAsync(false);
                        List<Entity> entities = await world.GetEntitiesAsync();

                        GameServerConfiguration config = new GameServerConfiguration();
                        config.SetPort(0).SetMaxConnections(1).SetOnlyAllowLocalConnections(true).SetTickRate(20);

                        ECS serverECS = new ECS();
                        GameServer gameServer = new GameServer(serverECS, wc, world, config, 500, 5000);
                        serverECS.Initialize(SystemRunner.Server, gameServer: gameServer, entities: entities);

                        await gameServer.StartAsync();
                        _ = gameServer.RunAsync();

                        int serverPort = gameServer.Port;

                        GameClient gameClient = new GameClient("127.0.0.1", serverPort, 500, 5000);
                        //ServerWorldGenerator swg = new ServerWorldGenerator(gameClient);

                        await gameClient.ConnectAsync(name); // Cannot fail, as we are connecting to our own host.

                        ScreenManager.GoToScreen<ScreenPlayingWorld, EnterPlayingWorldArgs>(new EnterPlayingWorldArgs() { Server = gameServer, Client = gameClient });
                    });
                }));
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
            Audio.Play("default.audio.click", Utilities.GetRandomFloat(0.8f, 1.2f));
        }

        if (GUI.Button("Play Test Sound 2", new Vector2(middleOfScreen.X - width / 2f, middleOfScreen.Y + height * 4 + distance * 4), new Vector2(width, height)))
        {
            Audio.Play("default.audio.click_2", Utilities.GetRandomFloat(0.8f, 1.2f));
        }

        float volume = Settings.GetSetting<float>("volume_master");
        if (GUI.Slider("Master Volume", new Vector2(middleOfScreen.X - width / 2f, middleOfScreen.Y + height * 5 + distance * 5), new Vector2(width, height), ref volume))
        {
            AL.Listener(ALListenerf.Gain, volume);
            _ = Settings.SetSettingAsync("volume_master", volume);
        }

        int fpsLimit = Settings.GetSetting<int>("fps_limit");
        float f = fpsLimit / 260f;
        if (GUI.Slider($"FPS Limit: {fpsLimit}", new Vector2(middleOfScreen.X - width / 2f, middleOfScreen.Y + height * 6 + distance * 6), new Vector2(width, height), ref f))
        {
            _ = Settings.SetSettingAsync("fps_limit", (int)(f * 260f));
            DisplayManager.SetTargetFPS((int)(f * 260f));
        }

        Locale[] locales = Localization.GetAvailableLocales();
        int currentLocale = Array.IndexOf(locales, Localization.GetLocale());
        string[] localeNames = locales.Select(x => x.LocaleName).ToArray();
        if (GUI.Dropdown(localeNames, new Vector2(20, 20), new Vector2(200, 40), ref currentLocale))
        {
            Localization.SetLocale(locales[currentLocale].Name);
        }
        GUI.End();
    }

    public override void Update()
    {
    }
}