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

            DisplayManager.OnFramebufferResize += (window, size) =>
            {
                glViewport(0, 0, (int)size.X, (int)size.Y);
                GameConsole.WriteLine("Window Change", $"new size: {size}");
                Renderer.DefaultCamera.FocusPosition = size / 2.0f;
            };

            AssetManager.OnAllAssetsLoaded += (sender, e) =>
            {
                GameConsole.WriteLine("ASSETS", "All assets loaded.");
            };

            AssetManager.OnAssetLoaded += (sender, asset) =>
            {
                GameConsole.WriteLine("ASSETS", $"Successfully loaded asset '{asset.Name}'");
            };

            AssetManager.OnFinalizeStart += (sender, e) =>
            {
                GameConsole.WriteLine("ASSETS", $"<0xFFFF00>Finalizing assets...</>");
            };

            AssetManager.OnFinalizeEnd += (sender, e) =>
            {
                GameConsole.WriteLine("ASSETS", $"<0x00FF00>Finalizing complete!</>");

                ScriptingManager.LoadScripts();
                GameConsole.LoadCommands();

                ScreenManager.Init();
                ScreenManager.GoToScreen("testscreen");
            };

            AssetManager.OnAllCoreAssetsLoaded += (sender, e) =>
            {
                GameConsole.WriteLine("ASSETS", "All core assets loaded!");
                DisplayManager.SetWindowIcon(AssetManager.GetAsset<Texture2D>("tex_pine_tree"));
                Renderer.Init();
            };

            AssetManager.LoadAllAssetsAsync();

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

                Vector2 middleOfScreen = DisplayManager.GetWindowSizeInPixels() / 2.0f;

                string topText = "Loading assets...";
                Vector2 topTextPos = (middleOfScreen + new Vector2(0, -50) - coreFont.MeasureString(topText, 1.0f) / 2.0f).Round();
                Renderer.Text.RenderText(coreFont, topText, topTextPos, 1.0f, ColorF.White, Renderer.Camera);

                int loadbarLength = 80;
                int hashtagAmount = (int)(AssetManager.LoadedPercentage * loadbarLength);
                string hashtags = "#".Repeat(hashtagAmount);
                string loadBar = hashtags + "_".Repeat(loadbarLength - hashtagAmount);
                Vector2 loadBarPos = (middleOfScreen - coreFont.MeasureString(loadBar, 1.0f) / 2.0f).Round();
                Renderer.Text.RenderText(coreFont, loadBar, loadBarPos, 1.0f, ColorF.White, Renderer.Camera);

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