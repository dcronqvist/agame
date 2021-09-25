using System.Numerics;
using AGame.Engine.Assets;
using AGame.Engine.DebugTools;
using AGame.Engine.Graphics;
using AGame.Engine.Graphics.Rendering;

namespace AGame.Engine.Screening
{
    class TestScreen : Screen
    {
        Vector2 texPos;

        public TestScreen() : base("testscreen")
        {

        }

        public override void Initialize()
        {
            texPos = Vector2.Zero;
        }

        public override void OnEnter()
        {
            GameConsole.WriteLine("TestScreen", "Entered TestScreen");
        }

        public override void OnLeave()
        {

        }

        public override void Render()
        {
            Renderer.SetRenderTarget(null, null);
            Renderer.Clear(ColorF.DeepBlue);

            Texture2D t = AssetManager.GetAsset<Texture2D>("tex_pine_tree");
            Renderer.Texture.Render(t, texPos, Vector2.One * 2f, 0f, ColorF.White);
        }

        public override void Update()
        {
            texPos = Input.GetMousePosition(Renderer.Camera);
        }
    }
}