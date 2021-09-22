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
        }

        public override void Update()
        {
            // Game updating

            if (Input.IsKeyPressed(Keys.Home))
            {
                inConsole = !inConsole;
                GameConsole.SetEnabled(inConsole);
            }

            GameConsole.Update();
        }

        public override void Render()
        {
            Renderer.SetRenderTarget(null, null);
            Renderer.Clear(ColorF.BlueGray);

            // Here the game rendering should be.

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