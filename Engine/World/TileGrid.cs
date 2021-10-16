using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Numerics;
using AGame.Engine.Assets;
using AGame.Engine.Graphics;
using AGame.Engine.Graphics.Rendering;

namespace AGame.Engine.World
{
    public class TileRenderable : IRenderable
    {
        public Vector2 Position { get; set; }
        public Vector2 BasePosition { get; set; }
        private Texture2D tileTexture;
        private int tileWidth;
        private int tileHeight;

        public TileRenderable(Vector2 pos, int tileID, int tileWidth, int tileHeight)
        {
            this.tileWidth = tileWidth;
            this.tileHeight = tileHeight;
            this.Position = pos;
            this.tileTexture = TileManager.GetTileFromID(tileID).Texture;

            float scale = (TileGrid.TILE_SIZE / (float)tileTexture.Width);
            this.BasePosition = pos + new Vector2(0.5f, 1f) * TileGrid.TILE_SIZE + (scale * TileManager.GetTileFromID(tileID).TopLeftInTexture);
        }

        public void Render()
        {
            Vector2 scale = Vector2.One * (TileGrid.TILE_SIZE / (float)this.tileTexture.Width) * this.tileWidth;
            Renderer.Texture.Render(this.tileTexture, this.Position, scale, 0f, ColorF.White);
        }
    }

    public class TileGrid
    {
        public const int TILE_SIZE = 32;

        public int[,] GridOfIDs { get; set; }
        public int Width { get; set; }
        public int Height { get; set; }

        private TileRenderable[] tileRenderables;

        public TileGrid(int[,] grid)
        {
            this.GridOfIDs = grid;
            this.Height = grid.GetLength(1);
            this.Width = grid.GetLength(0);

            this.tileRenderables = this.CollectTileRenderables();
        }

        private TileRenderable[] CollectTileRenderables()
        {
            List<TileRenderable> renderables = new List<TileRenderable>();

            int y = 0;

            while (y < this.Height)
            {
                int x = 0;

                while (x < this.Width)
                {
                    if (this.GridOfIDs[x, y] != 0 && this.GridOfIDs[x, y] != -1)
                    {
                        Tile t = TileManager.GetTileFromID(this.GridOfIDs[x, y]);

                        if (y - 1 > 0)
                        {
                            if (this.GridOfIDs[x, y - 1] == this.GridOfIDs[x, y] && x + 1 < this.Width)
                            {
                                if (this.GridOfIDs[x + 1, y] == -1)
                                {
                                    x += t.Width;
                                    continue;
                                }
                            }
                        }

                        float scale = ((TileGrid.TILE_SIZE / (float)t.Texture.Width) * t.Width);

                        renderables.Add(new TileRenderable(new Vector2(x, y) * TileGrid.TILE_SIZE - (scale * t.TopLeftInTexture), this.GridOfIDs[x, y], t.Width, t.Height));

                        x += t.Width;
                    }
                    else
                    {
                        x += 1;
                    }

                }

                y += 1;
            }

            // for (int y = 0; y < this.Height; y++)
            // {
            //     for (int x = 0; x < this.Width; x++)
            //     {
            //         if (this.GridOfIDs[x, y] != 0)
            //         {
            //             Tile t = TileManager.GetTileFromID(this.GridOfIDs[x, y]);
            //             float scale = (TileGrid.TILE_SIZE / (float)t.Texture.Width);

            //             renderables.Add(new TileRenderable(new Vector2(x, y) * TileGrid.TILE_SIZE - (scale * t.TopLeftInTexture), this.GridOfIDs[x, y]));
            //         }
            //     }
            // }

            return renderables.ToArray();
        }

        public bool CanUpdateTile(int x, int y, int tileID)
        {
            if (x < 0 || y < 0 || x > this.Width || y > this.Height)
            {
                return false;
            }

            Tile t = TileManager.GetTileFromID(tileID);

            for (int _y = 0; _y < t.Height; _y++)
            {
                for (int _x = 0; _x < t.Width; _x++)
                {
                    if (this.GridOfIDs[x + _x, y + _y] != 0)
                    {
                        return false;
                    }
                }
            }

            return true;
        }

        public void UpdateTile(int x, int y, int tileID)
        {
            Tile t = TileManager.GetTileFromID(tileID);

            for (int _y = 0; _y < t.Height; _y++)
            {
                for (int _x = 0; _x < t.Width; _x++)
                {
                    if (_y == 0 || _x == 0)
                        this.GridOfIDs[x + _x, y + _y] = tileID;
                    else
                        this.GridOfIDs[x + _x, y + _y] = -1;
                }
            }

            this.tileRenderables = this.CollectTileRenderables();
        }

        public int GetTileXFromPosition(Vector2 pos)
        {
            return (int)pos.X / TILE_SIZE;
        }

        public int GetTileYFromPosition(Vector2 pos)
        {
            return (int)pos.Y / TILE_SIZE;
        }

        public TileRenderable[] GetTileRenderablesInRect(RectangleF rectangle)
        {
            return this.tileRenderables.Where(tile => rectangle.IntersectsWith(new RectangleF(tile.Position.X, tile.Position.Y, TileGrid.TILE_SIZE, TileGrid.TILE_SIZE))).ToArray();
        }

        public TileRenderable[] GetTileRenderables()
        {
            return this.tileRenderables;
        }
    }
}