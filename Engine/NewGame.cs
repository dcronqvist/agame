using System;
using AGame.Engine.GLFW;
using AGame.Engine.Graphics;
using AGame.Engine.Graphics.Cameras;
using static AGame.Engine.OpenGL.GL;
using System.IO;
using System.Numerics;
using AGame.Engine.Graphics.Rendering;
using AGame.Engine;
using AGame.Engine.Assets;
using AGame.Engine.Assets.Scripting;
using AGame.Engine.DebugTools;
using AGame.Engine.Screening;
using System.Threading;
using AGame.Engine.ECSys;
using AGame.Engine.World;
using AGame.Engine.UI;
using AGame.Engine.Configuration;
using AGame.Engine.Networking;

namespace AGame.Engine
{
    class NewGame : Game
    {
        private bool _coreLoaded = false;
        private string _lastAssetLoaded = "";

        public override void Initialize(string[] args)
        {
            ECS.Instance.Value.Initialize(SystemRunner.Client);
            //Logging.AddLogStream(new FileLogger("log.txt"));
            Logging.AddLogStream(new ConsoleLogger());

            Logging.StartLogging();

            DisplayManager.SetTargetFPS(144);
            Utilities.InitRNG();


            Logging.Log(LogLevel.Info, "Starting game...");
        }

        public override void LoadContent(string[] args)
        {
            GameConsole.Initialize();

            DisplayManager.OnFramebufferResize += (window, size) =>
            {
                glViewport(0, 0, (int)size.X, (int)size.Y);
                Logging.Log(LogLevel.Info, $"glViewport set to {size.X}x{size.Y}");
            };

            ModManager.AllAssetsFinalized += (sender, e) =>
            {
                Logging.Log(LogLevel.Info, $"All assets loaded");
                ScriptingManager.LoadScripts();
                GameConsole.LoadCommands();

                TileManager.Init();
            };

            ModManager.AssetLoaded += (sender, e) =>
            {
                _lastAssetLoaded = e.Asset.Name;
                Logging.Log(LogLevel.Info, $"Loaded asset {e.Asset.Name}");
            };

            ModManager.AssetFailedLoad += (sender, e) =>
            {
                Logging.Log(LogLevel.Error, $"Failed to load asset {e.FailedAsset.FileName} in mod {e.Mod.Name}, reason: {e.FailedAsset.Exception.Message}");
            };

            ModManager.OverwroteAsset += (sender, e) =>
            {
                Logging.Log(LogLevel.Info, $"Overwrote asset {e.Overwrite.Original} with {e.Overwrite.New}");
            };

            ModManager.AllCoreAssetsLoaded += async (sender, e) =>
            {
                Renderer.Init();
                Logging.Log(LogLevel.Info, "All core assets loaded!");
                //ScreenManager.Init(args);
                Logging.Log(LogLevel.Info, "Screen manager initialized!");

                bool success = Localization.Init(Settings.GetSetting<string>("locale"));

                if (!success)
                {
                    Logging.Log(LogLevel.Info, "Failed to load localization, falling back to default locale");
                    Localization.Init("default.locale.en_US");
                }

                GUI.Init();
                _coreLoaded = true;

                if (args.Length > 0)
                {
                    newClient = new NewGameClient(args[0], 28000, 1000, 500000);
                    await newClient.ConnectAsync();
                }
                else
                {
                    ECS serverECS = new ECS();
                    serverECS.Initialize(SystemRunner.Server);
                    newServer = new NewGameServer(serverECS, 20, 28000, 300, 500000);
                    await newServer.StartAsync();
                    _ = newServer.RunAsync();

                    newClient = new NewGameClient("213.89.14.216", 28000, 300, 500000);
                    await newClient.ConnectAsync();
                }


                done = true;
            };

            glEnable(GL_BLEND);
            glDisable(GL_DEPTH_TEST);
            glBlendFunc(GL_SRC_ALPHA, GL_ONE_MINUS_SRC_ALPHA);

            glViewport(0, 0, (int)DisplayManager.GetWindowSizeInPixels().X, (int)DisplayManager.GetWindowSizeInPixels().Y);

            Settings.LoadSettings();

            ModManager.Init();
            _ = ModManager.LoadAllModsAsync();
        }

        bool done = false;
        NewGameServer newServer = null;
        NewGameClient newClient = null;

        public override void Update()
        {
            if (done)
            {
                newClient.Update();
            }
        }

        float fakeLatency = 0f;

        public override void Render()
        {
            Renderer.SetRenderTarget(null, null);
            Renderer.Clear(ColorF.Black);

            GUI.Begin();

            if (done)
            {
                newClient.Render();
                //newServer?.Render();

                GUI.Slider("Client Latency", new Vector2(100, 100), new Vector2(200, 50), ref fakeLatency);

                this.newClient.SetFakelatency((int)(fakeLatency * 2000f));
            }

            GUI.End();

            DisplayManager.SwapBuffers();
        }

        public override async void Unload()
        {
            await newClient.DisconnectAsync(1);
            await newServer?.StopAsync(1);
        }
    }
}