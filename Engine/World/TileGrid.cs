using System;
using System.Drawing;
using System.Numerics;
using AGame.Engine.Assets;
using AGame.Engine.Graphics;
using AGame.Engine.Graphics.Cameras;
using AGame.Engine.Graphics.Rendering;
using AGame.Engine.World;

namespace AGame.World
{
    class TileGrid
    {
        public Tile[,] Grid { get; set; }
        public int Width { get; set; }
        public int Height { get; set; }

        public TileGrid(int width, int height, Tile[,] grid = null)
        {
            this.Width = width;
            this.Height = height;

            if (grid == null)
            {
                this.Grid = new Tile[width, height];

                for (int y = 0; y < this.Height; y++)
                {
                    for (int x = 0; x < this.Width; x++)
                    {
                        // Render each tile
                        Grid[x, y] = new Tile("tex_marsdirt", false);
                    }
                }
            }
            else
            {
                this.Grid = grid;
                this.Width = grid.GetLength(0);
                this.Height = grid.GetLength(1);
            }
        }

        public void Render()
        {
            int tileSize = 32;
            RectangleF visibleArea = Renderer.Camera.VisibleArea;

            int minX = (int)(visibleArea.X / tileSize);
            int maxX = (int)((visibleArea.X + visibleArea.Width) / tileSize);

            int minY = (int)(visibleArea.Y / tileSize);
            int maxY = (int)((visibleArea.Y + visibleArea.Height) / tileSize);

            // render all tiles
            for (int y = Math.Max(minY, 0); y < Math.Min(this.Height, maxY + 1); y++)
            {
                for (int x = Math.Max(minX, 0); x < Math.Min(this.Width, maxX + 1); x++)
                {
                    // Render each tile
                    if (Grid[x, y] != null)
                    {
                        Texture2D tileTex = Grid[x, y].Texture;
                        Vector2 tilePos = new Vector2(tileSize * x, tileSize * y);
                        Renderer.Texture.Render(tileTex, tilePos, new Vector2(tileSize / (float)tileTex.Width), 0f, ColorF.White);
                    }
                }
            }
        }
    }
}