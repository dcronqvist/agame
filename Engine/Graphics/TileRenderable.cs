using System.Numerics;
using AGame.Engine.Assets;
using AGame.Engine.Graphics.Rendering;
using AGame.Engine.World;

namespace AGame.Engine.Graphics
{
    public class TileRenderable : IRenderable
    {
        public Vector2 Position { get; set; }
        public Vector2 BasePosition { get; set; }
        private Texture2D tileTexture;
        private int tileWidth;
        private int tileHeight;

        public TileRenderable(Vector2 pos, Texture2D tex, Vector2 topLeft, int tileWidth, int tileHeight)
        {
            this.tileWidth = tileWidth;
            this.tileHeight = tileHeight;
            this.Position = pos;
            this.tileTexture = tex;

            Vector2 scale = new Vector2((TileGrid.TILE_SIZE / (float)this.tileTexture.Width) * this.tileWidth, (TileGrid.TILE_SIZE / (float)this.tileTexture.Height) * this.tileHeight);
            this.BasePosition = pos + new Vector2(0.5f, 1f) * TileGrid.TILE_SIZE + (scale * topLeft);
        }

        public void Render()
        {
            Vector2 scale = new Vector2((TileGrid.TILE_SIZE / (float)this.tileTexture.Width) * this.tileWidth, (TileGrid.TILE_SIZE / (float)this.tileTexture.Height) * this.tileHeight);
            Renderer.Texture.Render(this.tileTexture, this.Position, scale, 0f, ColorF.White);
        }
    }
}