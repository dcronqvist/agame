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

namespace AGame.Engine
{
    class ImplGame : Game
    {
        private bool _coreLoaded = false;
        private string _lastAssetLoaded = "";

        public override void Initialize(string[] args)
        {
            ECS.Instance.Value.Initialize(SystemRunner.Client);
        }

        public unsafe override void LoadContent(string[] args)
        {
            GameConsole.Initialize();

            DisplayManager.OnFramebufferResize += (window, size) =>
            {
                glViewport(0, 0, (int)size.X, (int)size.Y);
                Renderer.DefaultCamera.FocusPosition = size / 2.0f;
            };

            AssetManager.OnFinalizeEnd += (sender, e) =>
            {
                ScriptingManager.LoadScripts();
                GameConsole.LoadCommands();

                TileManager.Init();
                Utilities.InitRNG();
            };

            AssetManager.OnAssetStartLoad += (sender, e) =>
            {
                _lastAssetLoaded = e;
            };

            AssetManager.OnAllCoreAssetsLoaded += (sender, e) =>
            {
                Renderer.Init();
                ScreenManager.Init(args);
                Localization.Init("en_US");
                GUI.Init();
                _coreLoaded = true;
            };

            glEnable(GL_BLEND);
            glDisable(GL_DEPTH_TEST);
            glBlendFunc(GL_SRC_ALPHA, GL_ONE_MINUS_SRC_ALPHA);

            glViewport(0, 0, (int)DisplayManager.GetWindowSizeInPixels().X, (int)DisplayManager.GetWindowSizeInPixels().Y);

            _ = AssetManager.LoadAllAssetsAsync();
        }

        public override void Update()
        {
            if (_coreLoaded && ScreenManager.CurrentScreen == null)
            {
                ScreenManager.GoToScreen("screen_loading_assets", _lastAssetLoaded);
            }

            // Game updating
            ScreenManager.Update();
        }

        public override void Render()
        {
            GUI.Begin();
            ScreenManager.Render();
            GUI.End();

            DisplayManager.SwapBuffers();
        }

        public override void Unload()
        {
            ScreenManager.CurrentScreen.OnLeave();
        }
    }
}