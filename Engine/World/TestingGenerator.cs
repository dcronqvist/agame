using System;
using System.Collections.Generic;
using System.Numerics;
using AGame.World;
using SimplexNoise;

namespace AGame.Engine.World
{
    class TestingGenerator : ICraterGenerator
    {
        const int size = 50;

        public TileGrid GenerateBackgroundLayer(int seed)
        {
            Noise.Seed = seed;

            int[,] tiles = new int[size, size];
            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    tiles[x, y] = 3;
                }
            }

            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    float scale = 1f / 20f;
                    float multiplier = 2f;
                    float threshhold = 1f;
                    float first = (Noise.CalcPixel2D(x, y, scale)) * multiplier;

                    float circularFallof = Utilities.GetLinearCircularFalloff(size, x, y);
                    if (first * circularFallof >= threshhold)
                    {
                        tiles[x, y] = 1;
                    }
                }
            }

            return new TileGrid(tiles);
        }

        public TileGrid GenerateResourceLayer(int seed)
        {
            return new TileGrid(new int[size, size]);
        }
    }
}