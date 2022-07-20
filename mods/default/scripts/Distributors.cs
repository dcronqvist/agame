using System.Collections.Generic;
using AGame.Engine;
using AGame.Engine.Assets.Scripting;
using AGame.Engine.World;

namespace DefaultMod
{
    [ScriptClass(Name = "distributor_squares")] // default.script.distributor_squares
    public class SquareDistributor : IDistributor
    {
        public List<SpawnEntityDefinition> GetDistribution(string entityAsset, float size, Vector2i startTile)
        {
            List<SpawnEntityDefinition> definitions = new List<SpawnEntityDefinition>();

            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    definitions.Add(new SpawnEntityDefinition(entityAsset, new Vector2i(startTile.X + x, startTile.Y + y)));
                }
            }
            return definitions;
        }
    }

    [ScriptClass(Name = "distributor_circles")] // default.script.distributor_circles
    public class CircleDistributor : IDistributor
    {
        public List<SpawnEntityDefinition> GetDistribution(string entityAsset, float size, Vector2i startTile)
        {
            List<SpawnEntityDefinition> definitions = new List<SpawnEntityDefinition>();

            int radius = (int)size / 2;
            int radiusSquared = radius * radius;
            for (int y = -radius; y <= radius; y++)
            {
                for (int x = -radius; x <= radius; x++)
                {
                    if (x * x + y * y <= radiusSquared)
                    {
                        definitions.Add(new SpawnEntityDefinition(entityAsset, new Vector2i(startTile.X + x, startTile.Y + y)));
                    }
                }
            }
            return definitions;
        }
    }
}