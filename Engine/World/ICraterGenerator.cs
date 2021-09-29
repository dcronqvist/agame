using AGame.World;

namespace AGame.Engine.World
{
    interface ICraterGenerator
    {
        TileGrid[] GenerateGrids();
    }
}