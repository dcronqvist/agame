using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Numerics;
using AGame.Engine.Assets;
using AGame.Engine.Graphics;
using AGame.Engine.Graphics.Rendering;

namespace AGame.Engine.World
{
    public class TileGrid
    {
        public const int TILE_SIZE = 32;

        public Tile[,] GridOfTiles { get; set; }

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

                        Vector2 scale = new Vector2((TileGrid.TILE_SIZE / (float)t.Texture.Width) * t.Width, (TileGrid.TILE_SIZE / (float)t.Texture.Height) * t.Height);
                        renderables.Add(new TileRenderable(new Vector2(x, y) * TileGrid.TILE_SIZE - (scale * t.TopLeftInTexture), t.Texture, t.TopLeftInTexture, t.Width, t.Height));

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

        public Tile GetTileFromPosition(int x, int y)
        {
            if (this.GridOfIDs[x, y] == 0)
            {
                return null;
            }

            int _y = y;
            while (this.GridOfIDs[x, _y] == -1)
            {
                _y--;
            }
            return TileManager.GetTileFromID(this.GridOfIDs[x, _y]);
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