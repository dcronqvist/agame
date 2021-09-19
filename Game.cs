using AGame.Graphics;

namespace AGame
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

            while (!DisplayManager.GetWindowShouldClose())
            {
                DisplayManager.SwapBuffers();
                DisplayManager.PollEvents();

                Input.Begin();

                Update();

                Render();

                Input.End();
            }

            Unload();

            DisplayManager.CloseWindow();
        }
    }
}