using System;
using GLFW;
using AGame.Graphics;

namespace AGame
{
    class ImplGame : Game
    {
        public override void Initialize()
        {

        }

        public override void LoadContent()
        {

        }

        public override void Update()
        {
            DisplayManager.SetWindowTitle(Input.GetMousePosition().ToString());
        }

        public override void Render()
        {

        }

        public override void Unload()
        {

        }
    }
}