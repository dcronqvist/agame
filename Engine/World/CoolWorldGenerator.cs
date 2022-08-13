using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace AGame.Engine.World;

public class CoolWorldGenerator : IWorldGenerator
{
    FastNoiseLite genNoise;
    FastNoiseLite warpNoise;

    public CoolWorldGenerator()
    {
        this.genNoise = new FastNoiseLite();
        this.warpNoise = new FastNoiseLite();

        this.genNoise.SetNoiseType(FastNoiseLite.NoiseType.Cellular);
        this.genNoise.SetSeed(1337);
        this.genNoise.SetFrequency(0.005f);

        this.genNoise.SetFractalType(FastNoiseLite.FractalType.None);

        this.genNoise.SetCellularDistanceFunction(FastNoiseLite.CellularDistanceFunction.Hybrid);
        this.genNoise.SetCellularReturnType(FastNoiseLite.CellularReturnType.CellValue);

        this.warpNoise.SetSeed(1337);
        this.warpNoise.SetDomainWarpType(FastNoiseLite.DomainWarpType.OpenSimplex2);
        this.warpNoise.SetFrequency(0.01f);
        this.warpNoise.SetFractalType(FastNoiseLite.FractalType.DomainWarpIndependent);
        this.warpNoise.SetFractalOctaves(3);
        this.warpNoise.SetFractalLacunarity(3.6f);
        this.warpNoise.SetFractalGain(0.4f);
    }

    private string GetTileFromNoise(float noise)
    {
        if (noise < 0.5f)
        {
            return "game:marsdirt";
        }
        else
        {
            return "game:marsdirt_dark";
        }
    }

    public Chunk GenerateChunk(int x, int y)
    {
        int width = Chunk.CHUNK_SIZE;
        int height = Chunk.CHUNK_SIZE;

        int[,] tileGrid = new int[width, height];

        for (int _y = 0; _y < height; _y++)
        {
            for (int _x = 0; _x < width; _x++)
            {
                CoordinateVector coord = new CoordinateVector(_x + x * Chunk.CHUNK_SIZE, _y + y * Chunk.CHUNK_SIZE);

                float xf = coord.ToWorldVector().X;
                float yf = coord.ToWorldVector().Y;

                warpNoise.DomainWarp(ref xf, ref yf);

                float noise = MathF.Round(genNoise.GetNoise(xf, yf));

                tileGrid[_x, _y] = (int)noise;
            }
        }

        int[,] darker = new int[width, height];

        for (int _y = 0; _y < height; _y++)
        {
            for (int _x = 0; _x < width; _x++)
            {
                darker[_x, _y] = 1;
            }
        }

        List<ChunkLayer> layers = new List<ChunkLayer>();
        layers.Add(new ChunkLayer("default.tileset.tileset_1_darker", 2, darker, false));
        layers.Add(new ChunkLayer("default.tileset.tileset_1", 1, tileGrid, true));
        return new Chunk(x, y, layers);
    }

    public Task<Chunk> GenerateChunkAsync(int x, int y)
    {
        return Task.FromResult(this.GenerateChunk(x, y));
    }

    public List<EntityDistributionDefinition> GetEntityDistributionDefinitions()
    {
        return new List<EntityDistributionDefinition>()
        {
            new EntityDistributionDefinition("default.entity.ore_rock", 0.08f, 4f, "default.script_type.distributor_circles"),
            new EntityDistributionDefinition("default.entity.test_rock", 0.08f, 1f, "default.script_type.distributor_squares")
    };
    }
}