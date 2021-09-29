using System;
using System.Collections.Generic;
using AGame.World;
using SimplexNoise;

namespace AGame.Engine.World
{
    class TestingGenerator : ICraterGenerator
    {
        public TileGrid[] GenerateGrids(int seed)
        {
            Noise.Seed = seed;

            List<TileGrid> grids = new List<TileGrid>();

            int size = 300;
            int radius = size / 2;

            int[,] tiles = new int[size, size];

            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    float scale = 1f / 30f;
                    float multiplier = 5f;
                    float threshhold = 0.9f;
                    float first = (Noise.CalcPixel2D(x, y, scale) + 1.0f) * multiplier;

                    float circularFallof = Utilities.GetLinearCircularFalloff(size, x, y);
                    if (first * circularFallof >= threshhold)
                        tiles[x, y] = 1;
                }
            }

            TileGrid tg = new TileGrid(tiles);

            grids.Add(tg);

            return grids.ToArray();
        }
    }
}