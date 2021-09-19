using System;
using GLFW;

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
            if (Input.IsMouseButtonPressed(MouseButton.Left))
            {
                Console.WriteLine("Hej");
            }
        }

        public override void Render()
        {

        }

        public override void Unload()
        {

        }
    }
}