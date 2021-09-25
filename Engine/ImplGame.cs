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

namespace AGame.Engine
{
    class ImplGame : Game
    {
        bool inConsole;

        public override void Initialize()
        {
            inConsole = false;
        }

        public unsafe override void LoadContent()
        {
            GameConsole.Initialize();
            DisplayManager.SetTargetFPS(144);

            AssetManager.OnAllAssetsLoaded += (sender, e) =>
            {
                GameConsole.WriteLine("ASSETS", "All assets loaded.");
            };
            AssetManager.LoadAllAssetsAsync();
            Renderer.Init();

            glEnable(GL_BLEND);
            glDisable(GL_DEPTH_TEST);
            glBlendFunc(GL_SRC_ALPHA, GL_ONE_MINUS_SRC_ALPHA);

            glViewport(0, 0, (int)DisplayManager.GetWindowSizeInPixels().X, (int)DisplayManager.GetWindowSizeInPixels().Y);
        }

        public override void Update()
        {
            if (AssetManager.AllAssetsLoaded && ScreenManager.CurrentScreen == null)
            {
                AssetManager.FinalizeAssets();
                ScriptingManager.LoadScripts();
                GameConsole.LoadCommands();


                DisplayManager.OnFramebufferResize += (window, size) =>
                {
                    glViewport(0, 0, (int)size.X, (int)size.Y);
                    GameConsole.WriteLine("Window Change", $"new size: {size}");
                    Renderer.DefaultCamera.FocusPosition = size / 2.0f;
                };

                ScreenManager.Init();
                ScreenManager.GoToScreen("testscreen");
            }

            if (!inConsole)
            {
                // Game updating
                ScreenManager.Update();
            }


            if (Input.IsKeyPressed(Keys.Home))
            {
                inConsole = !inConsole;
                GameConsole.SetEnabled(inConsole);
            }

            GameConsole.Update();
        }

        public override void Render()
        {
            // Here the game rendering should be.
            if (ScreenManager.CurrentScreenName == "")
            {
                Renderer.Clear(ColorF.Black);
                Font coreFont = AssetManager.GetAsset<Font>("font_rainyhearts");
                Renderer.Text.RenderText(coreFont, $"Loaded {AssetManager.AssetsLoaded} / {AssetManager.TotalAssetsToLoad}", new Vector2(100, 100), 1.0f, ColorF.White, Renderer.Camera);
            }
            else
            {
                ScreenManager.Render();
            }

            Renderer.SetRenderTarget(null, null);
            if (inConsole)
            {
                RenderTexture rt = GameConsole.Render(AssetManager.GetAsset<Font>("font_rainyhearts"));
                Renderer.RenderRenderTexture(rt);
            }

            DisplayManager.SwapBuffers();
        }

        public override void Unload()
        {

        }
    }
}