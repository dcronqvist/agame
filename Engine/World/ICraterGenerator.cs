using System.Collections.Generic;
using System.Numerics;
using AGame.World;

namespace AGame.Engine.World
{
    interface ICraterGenerator
    {
        TileGrid GenerateBackgroundLayer(int seed);
        TileGrid GenerateResourceLayer(int seed);
    }
}