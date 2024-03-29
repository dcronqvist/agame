using AGame.Engine.Graphics;
using AGame.Engine.GLFW;
using AGame.Engine;
using System.Threading;
using System;

namespace AGame.Engine
{
    abstract class Game
    {
        public abstract void Initialize(string[] args);
        public abstract void LoadContent(string[] args);
        public abstract void Update();
        public abstract void Render();
        public abstract void Unload();

        public void Run(int winWidth, int winHeight, string winTitle, string[] args)
        {
            bool macMove = false;

            Initialize(args);

            DisplayManager.InitWindow(winWidth, winHeight, winTitle);

            LoadContent(args);

            GameTime.DeltaTime = 0;
            GameTime.TotalElapsedSeconds = 0;

            while (!DisplayManager.GetWindowShouldClose())
            {
                GameTime.DeltaTime = (float)Glfw.Time - GameTime.TotalElapsedSeconds;
                GameTime.TotalElapsedSeconds = (float)Glfw.Time;

                DisplayManager.PollEvents();

                Input.Begin();

                Update();

                Render();

                if (!macMove)
                {
                    Glfw.SetWindowSize(DisplayManager.WindowHandle, winWidth + 1, winHeight + 1);
                    Glfw.SetWindowSize(DisplayManager.WindowHandle, winWidth, winHeight);
                    macMove = true;
                }

                Input.End();

                if (DisplayManager.TargetFPS != 0)
                {
                    float waitTime = DisplayManager.FrameTime;
                    while (Glfw.Time < GameTime.TotalElapsedSeconds + waitTime) { }
                }
            }

            Unload();

            DisplayManager.CloseWindow();
        }
    }
}