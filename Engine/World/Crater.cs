using AGame.Engine.Graphics;
using AGame.Engine.Graphics.Cameras;
using AGame.Engine.Graphics.Rendering;
using AGame.World;

namespace AGame.Engine.World
{
    class Crater
    {
        public TileGrid[] Grids { get; set; }
        private RenderTexture groundRT;

        public Crater(ICraterGenerator generator)
        {
            // Initially only one 
            this.Grids = generator.GenerateGrids();

            groundRT = new RenderTexture(DisplayManager.GetWindowSizeInPixels());
        }

        public RenderTexture Render(Camera2D camera)
        {
            Renderer.SetRenderTarget(this.groundRT, camera);
            Renderer.Clear(ColorF.DeepBlue);

            for (int i = Grids.Length - 1; i >= 0; i--)
            {
                Grids[i].Render();
            }

            Renderer.SetRenderTarget(null, null);
            return this.groundRT;
        }
    }
}