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
        public int[,] GridOfIDs { get; set; }
        public int Width { get; set; }
        public int Height { get; set; }

        public TileGrid(int[,] grid)
        {
            this.GridOfIDs = grid;
            this.Width = grid.GetLength(0);
            this.Height = grid.GetLength(1);
        }

        public int GetTileXFromPosition(Vector2 pos)
        {
            int tileSize = 48;
            return (int)pos.X / tileSize;
        }

        public int GetTileYFromPosition(Vector2 pos)
        {
            int tileSize = 48;
            return (int)pos.Y / tileSize;
        }

        public void Render()
        {
            int tileSize = 48;
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
                    if (GridOfIDs[x, y] != 0)
                    {
                        Tile t = TileManager.GetTileFromID(GridOfIDs[x, y]);
                        Texture2D tileTex = t.Texture;
                        Vector2 tilePos = new Vector2(tileSize * x, tileSize * y);
                        Renderer.Texture.Render(tileTex, tilePos, new Vector2(tileSize / (float)tileTex.Width), 0f, ColorF.White);
                    }
                }
            }
        }
    }
}