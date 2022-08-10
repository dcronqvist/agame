using System;
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
                    definitions.Add(new SpawnEntityDefinition(entityAsset, (e) =>
                    {
                        e.GetComponent<TransformComponent>().Position = new CoordinateVector(startTile.X + x, startTile.Y + y);
                    }));
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
            int minAmountOfCircles = 2;
            int maxAmountOfCircles = 4;
            int maxRadius = (int)size / 2;

            List<SpawnEntityDefinition> definitions = new List<SpawnEntityDefinition>();

            int amountOfCircles = Utilities.GetRandomInt(minAmountOfCircles, maxAmountOfCircles + 1);

            List<Vector2i> tiles = new List<Vector2i>();

            for (int i = 0; i < amountOfCircles; i++)
            {
                int middleX = Utilities.GetRandomInt(-maxRadius, maxRadius + 1);
                int middleY = Utilities.GetRandomInt(-maxRadius, maxRadius + 1);

                int radius = Utilities.GetRandomInt(1, maxRadius + 1);
                int radiusSquared = radius * radius;

                for (int y = -radius; y <= radius; y++)
                {
                    for (int x = -radius; x <= radius; x++)
                    {
                        int xSquared = x * x;
                        int ySquared = y * y;

                        if (xSquared + ySquared <= radiusSquared)
                        {
                            var v = new Vector2i(startTile.X + middleX + x, startTile.Y + middleY + y);

                            if (!tiles.Contains(v))
                            {
                                tiles.Add(v);
                            }
                        }
                    }
                }
            }

            foreach (Vector2i tile in tiles)
            {
                definitions.Add(new SpawnEntityDefinition(entityAsset, (e) =>
                {
                    e.GetComponent<TransformComponent>().Position = new CoordinateVector(tile.X, tile.Y);
                }));
            }

            return definitions;
        }
    }
}