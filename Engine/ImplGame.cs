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

namespace AGame.Engine
{
    class ImplGame : Game
    {
        private bool _coreLoaded = false;
        private string _lastAssetLoaded = "";

        public override void Initialize(string[] args)
        {
            ECS.Instance.Value.Initialize(SystemRunner.Client);
            //Logging.AddLogStream(new FileLogger("log.txt"));
            Logging.AddLogStream(new ConsoleLogger());
        }

        public override void LoadContent(string[] args)
        {
            GameConsole.Initialize();

            DisplayManager.OnFramebufferResize += (window, size) =>
            {
                glViewport(0, 0, (int)size.X, (int)size.Y);
            };

            ModManager.AllAssetsFinalized += (sender, e) =>
            {
                ScriptingManager.LoadScripts();
                GameConsole.LoadCommands();

                TileManager.Init();
                Utilities.InitRNG();
            };

            ModManager.AssetLoaded += (sender, e) =>
            {
                _lastAssetLoaded = e.Asset.Name;
                Logging.Log(LogLevel.Info, $"Loaded asset {e.Asset.Name}");
            };

            ModManager.AssetFailedLoad += (sender, e) =>
            {
                Logging.Log(LogLevel.Error, $"Failed to load asset {e.FailedAsset.FileName} in mod {e.Mod.Name}");
            };

            ModManager.OverwroteAsset += (sender, e) =>
            {
                Logging.Log(LogLevel.Info, $"Overwrote asset {e.Overwrite.Original} with {e.Overwrite.New}");
            };

            ModManager.AllCoreAssetsLoaded += (sender, e) =>
            {
                Renderer.Init();
                ScreenManager.Init(args);
                Localization.Init(Settings.GetSetting<string>("locale"));
                GUI.Init();
                _coreLoaded = true;
            };

            glEnable(GL_BLEND);
            glDisable(GL_DEPTH_TEST);
            glBlendFunc(GL_SRC_ALPHA, GL_ONE_MINUS_SRC_ALPHA);

            glViewport(0, 0, (int)DisplayManager.GetWindowSizeInPixels().X, (int)DisplayManager.GetWindowSizeInPixels().Y);

            Settings.LoadSettings();

            ModManager.Init();
            _ = ModManager.LoadAllModsAsync();
        }

        public override void Update()
        {
            if (_coreLoaded && ScreenManager.CurrentScreen == null)
            {
                ScreenManager.GoToScreen<ScreenLoadingAssets, EnterLoadingAssetsArgs>(new EnterLoadingAssetsArgs() { FinalCoreAsset = _lastAssetLoaded });
            }

            // Game updating
            ScreenManager.Update();
        }

        public override void Render()
        {
            ScreenManager.Render();

            DisplayManager.SwapBuffers();
        }

        public override void Unload()
        {
            ScreenManager.CurrentScreen.OnLeave();
        }
    }
}