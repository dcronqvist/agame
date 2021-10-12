using System.Collections.Generic;
using System.Numerics;
using AGame.World;

namespace AGame.Engine.World
{
    public interface ICraterGenerator
    {
        StaticTileGrid GenerateBackgroundLayer(int seed);
        DynamicTileGrid GenerateResourceLayer(int seed);
    }
}