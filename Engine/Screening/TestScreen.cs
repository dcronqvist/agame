using System.Numerics;
using AGame.Engine;
using AGame.Engine.Assets;
using AGame.Engine.DebugTools;
using AGame.Engine.GLFW;
using AGame.Engine.Graphics;
using AGame.Engine.Graphics.Rendering;
using AGame.Engine.Screening;

namespace AGame.MyGame
{
    class TestScreen : Screen
    {
        public TestScreen() : base("testscreen")
        {

        }

        public override Screen Initialize()
        {
            return this;
        }

        public override void OnEnter()
        {

        }

        public override void OnLeave()
        {

        }

        public override void Update()
        {

        }

        public override void Render()
        {
            Renderer.SetRenderTarget(null, null);
            Renderer.Clear(ColorF.DeepBlue);
        }
    }
}