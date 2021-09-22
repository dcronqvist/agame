using System;
using AGame.Engine.GLFW;
using AGame.Engine.Graphics;
using AGame.Engine.Graphics.Cameras;
using AGame.Engine.Graphics.Textures;
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
        Camera2D cam;

        public override void Initialize()
        {

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

            cam = new Camera2D(DisplayManager.GetWindowSizeInPixels() / 2f, 1f);
        }

        public override void Update()
        {
            DisplayManager.SetWindowTitle(Input.GetMousePosition().ToString());

            GameConsole.Update();
        }

        public override void Render()
        {
            glClearColor(0, 0, 0, 0);
            glClear(GL_COLOR_BUFFER_BIT);

            RenderTexture rt = GameConsole.Render(AssetManager.GetAsset<Font>("font_rainyhearts"));

            Renderer.RenderRenderTexture(rt);

            DisplayManager.SwapBuffers();
        }

        public override void Unload()
        {

        }
    }
}