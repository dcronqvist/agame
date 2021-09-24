using AGame.Engine.Graphics;
using AGame.Engine.GLFW;
using AGame.Engine;
using System.Threading;

namespace AGame.Engine
{
    abstract class Game
    {
        public abstract void Initialize();
        public abstract void LoadContent();
        public abstract void Update();
        public abstract void Render();
        public abstract void Unload();

        public void Run(int winWidth, int winHeight, string winTitle)
        {
            Initialize();

            DisplayManager.InitWindow(winWidth, winHeight, winTitle);

            LoadContent();

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