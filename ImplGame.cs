using System;
using AGame.Engine.GLFW;
using AGame.Engine.Graphics;
using AGame.Engine.Graphics.Shaders;
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

namespace AGame
{
    class ImplGame : Game
    {
        TextRenderer textRenderer;
        Font font;

        Camera2D cam;

        public override void Initialize()
        {

        }

        public unsafe override void LoadContent()
        {
            ScriptingManager.LoadScripts();
            AssetManager.LoadAllAssets();

            glEnable(GL_BLEND);
            glDisable(GL_DEPTH_TEST);
            glBlendFunc(GL_SRC_ALPHA, GL_ONE_MINUS_SRC_ALPHA);
            glViewport(0, 0, (int)DisplayManager.GetWindowSizeInPixels().X, (int)DisplayManager.GetWindowSizeInPixels().Y);

            Shader textShader = Shader.LoadFromFiles(Utilities.GetExecutableDirectory() + "/res/basic_text.vs", Utilities.GetExecutableDirectory() + "/res/basic_text.fs");

            font = AssetManager.GetAsset<Font>("font_rainyhearts");

            textRenderer = new TextRenderer(textShader);

            cam = new Camera2D(DisplayManager.GetWindowSizeInPixels() / 2f, 1f);

            ICommand bigCommand = ScriptingManager.CreateInstance<ICommand>("MyCommand");

            CommandResult cr = bigCommand.Execute();
            Console.WriteLine(cr.Message);
        }

        public override void Update()
        {
            DisplayManager.SetWindowTitle(Input.GetMousePosition().ToString());
        }

        public override void Render()
        {
            glClearColor(0, 0, 0, 0);
            glClear(GL_COLOR_BUFFER_BIT);

            textRenderer.RenderText(font, "Hello World!", Input.GetMousePosition(), 1f, ColorF.White, cam);

            DisplayManager.SwapBuffers();
        }

        public override void Unload()
        {

        }
    }
}