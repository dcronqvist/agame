using System.Numerics;
using AGame.Engine.DebugTools;
using AGame.Engine.Networking;
using GameUDPProtocol;

namespace AGame.Engine.World;

public interface IWorldGenerator
{
    Chunk GenerateChunk(int x, int y);
    Task<Chunk> GenerateChunkAsync(int x, int y);
}

public class TestWorldGenerator : IWorldGenerator
{
    public Chunk GenerateChunk(int x, int y)
    {
        int width = Chunk.CHUNK_SIZE;
        int height = Chunk.CHUNK_SIZE;

        int[,] tileGrid = new int[width, height];

        for (int _y = 0; _y < height; _y++)
        {
            for (int _x = 0; _x < width; _x++)
            {
                string tile = Utilities.ChooseUniform<string>("game:dirt");
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

public class RequestChunkPacket : Packet
{
    public int X { get; set; }
    public int Y { get; set; }

    public RequestChunkPacket()
    {

    }
}

public class WholeChunkPacket : Packet
{
    public int X { get; set; }
    public int Y { get; set; }
    public Chunk Chunk { get; set; }

    public WholeChunkPacket()
    {

    }
}

public class ChunkUpdatePacket : Packet
{
    public int X { get; set; }
    public int Y { get; set; }
    public Chunk Chunk { get; set; }

    public ChunkUpdatePacket()
    {

    }
}