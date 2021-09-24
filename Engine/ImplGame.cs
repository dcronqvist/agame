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
            ScriptingManager.LoadScripts();
            GameConsole.LoadCommands();
            AssetManager.LoadAllAssets();
            Renderer.Init();

            glEnable(GL_BLEND);
            glDisable(GL_DEPTH_TEST);
            glBlendFunc(GL_SRC_ALPHA, GL_ONE_MINUS_SRC_ALPHA);

            glViewport(0, 0, (int)DisplayManager.GetWindowSizeInPixels().X, (int)DisplayManager.GetWindowSizeInPixels().Y);

            DisplayManager.OnFramebufferResize += (window, size) =>
            {
                glViewport(0, 0, (int)size.X, (int)size.Y);
                GameConsole.WriteLine("Window Change", $"new size: {size}");
                Renderer.DefaultCamera.FocusPosition = size / 2.0f;
            };

            DisplayManager.SetTargetFPS(144);
        }

        public override void Update()
        {
            // Game updating

            if (Input.IsKeyPressed(Keys.Home))
            {
                inConsole = !inConsole;
                GameConsole.SetEnabled(inConsole);
            }

            DisplayManager.SetWindowTitle(Input.GetMousePosition(Renderer.Camera).ToString());

            GameConsole.Update();
        }

        public override void Render()
        {
            Renderer.SetRenderTarget(null, null);
            Renderer.Clear(ColorF.BlueGray);

            if (inConsole)
            {
                RenderTexture rt = GameConsole.Render(AssetManager.GetAsset<Font>("font_rainyhearts"));
                Renderer.RenderRenderTexture(rt);
            }


            // Here the game rendering should be.
            Texture2D t = AssetManager.GetAsset<Texture2D>("tex_pine_tree");
            Renderer.Texture.Render(t, Input.GetMousePosition(Renderer.Camera), Vector2.One, 0f, ColorF.White, Vector2.Zero);

            DisplayManager.SwapBuffers();
        }

        public override void Unload()
        {

        }
    }
}