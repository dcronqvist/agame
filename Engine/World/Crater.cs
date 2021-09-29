using System.Drawing;
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

        public Crater(int seed, ICraterGenerator generator)
        {
            // Initially only one 
            this.Grids = generator.GenerateGrids(seed);

            groundRT = new RenderTexture(DisplayManager.GetWindowSizeInPixels());
        }

        public RenderTexture Render(Camera2D camera)
        {
            Renderer.SetRenderTarget(this.groundRT, camera);
            Renderer.Clear(ColorF.DeepBlue);

            for (int i = 0; i < Grids.Length; i++)
            {
                Grids[i].Render();
            }

            int x = Grids[0].GetTileXFromPosition(Input.GetMousePosition(camera));
            int y = Grids[0].GetTileYFromPosition(Input.GetMousePosition(camera));

            Renderer.Primitive.RenderRectangle(new RectangleF(x * 48, y * 48, 48, 48), ColorF.BlueGray * 0.5f);

            Renderer.SetRenderTarget(null, null);
            return this.groundRT;
        }
    }
}