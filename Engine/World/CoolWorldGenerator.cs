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

                float noise = genNoise.GetNoise(xf, yf);

                string tile = GetTileFromNoise(noise);
                tileGrid[_x, _y] = TileManager.GetTileIDFromName(tile);
            }
        }

        return new Chunk(x, y, tileGrid);
    }

    public Task<Chunk> GenerateChunkAsync(int x, int y)
    {
        return Task.FromResult(this.GenerateChunk(x, y));
    }
}