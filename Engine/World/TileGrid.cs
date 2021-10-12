using System.Numerics;

namespace AGame.Engine.World
{
    public abstract class TileGrid
    {
        public const int TILE_SIZE = 32;

        public int[,] GridOfIDs { get; set; }
        public int Width { get; set; }
        public int Height { get; set; }

        public TileGrid(int[,] grid)
        {
            this.GridOfIDs = grid;
            this.Height = grid.GetLength(1);
            this.Width = grid.GetLength(0);
        }

        public int GetTileXFromPosition(Vector2 pos)
        {
            return (int)pos.X / TILE_SIZE;
        }

        public int GetTileYFromPosition(Vector2 pos)
        {
            return (int)pos.Y / TILE_SIZE;
        }

        public abstract void Render();
    }
}