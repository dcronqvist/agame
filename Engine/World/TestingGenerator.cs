using System;
using System.Collections.Generic;
using AGame.World;

namespace AGame.Engine.World
{
    class TestingGenerator : ICraterGenerator
    {
        public TileGrid[] GenerateGrids()
        {
            List<TileGrid> grids = new List<TileGrid>();

            int size = 30;
            int radius = size / 2;

            Tile[,] tiles = new Tile[size, size];

            for (int y = 0; y < 30; y++)
            {
                for (int x = 0; x < 30; x++)
                {
                    int _x = x - radius;
                    int _y = y - radius;

                    if (Math.Sqrt(_x * _x + _y * _y) <= radius)
                        tiles[x, y] = new Tile("tex_marsdirt", false);
                }
            }

            TileGrid tg = new TileGrid(tiles);

            grids.Add(tg);

            return grids.ToArray();
        }
    }
}