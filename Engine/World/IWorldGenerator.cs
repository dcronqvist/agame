using System.Collections.Generic;
using System.Numerics;
using System.Threading.Tasks;
using AGame.Engine.Configuration;
using AGame.Engine.DebugTools;
using AGame.Engine.Networking;
using GameUDPProtocol;

namespace AGame.Engine.World;

public interface IWorldGenerator
{
    Chunk GenerateChunk(int x, int y);
    Task<Chunk> GenerateChunkAsync(int x, int y);
    List<EntityDistributionDefinition> GetEntityDistributionDefinitions();
}

public class TestWorldGenerator : IWorldGenerator
{
    public Chunk GenerateChunk(int x, int y)
    {
        int width = Chunk.CHUNK_SIZE;
        int height = Chunk.CHUNK_SIZE;

        int[,] darker = new int[width, height];

        for (int _y = 0; _y < height; _y++)
        {
            for (int _x = 0; _x < width; _x++)
            {
                darker[_x, _y] = 1;
            }
        }

        int[,] lighter = new int[width, height];

        for (int _y = 0; _y < height; _y++)
        {
            for (int _x = 0; _x < width; _x++)
            {
                lighter[_x, _y] = Utilities.GetRandomInt(0, 2);
            }
        }

        List<ChunkLayer> layers = new List<ChunkLayer>();
        layers.Add(new ChunkLayer("default.tileset.tileset_1_darker", 2, darker, false));
        layers.Add(new ChunkLayer("default.tileset.tileset_1", 1, lighter, true));
        return new Chunk(x, y, layers);
    }

    public Task<Chunk> GenerateChunkAsync(int x, int y)
    {
        return Task.FromResult(this.GenerateChunk(x, y));
    }

    public List<EntityDistributionDefinition> GetEntityDistributionDefinitions()
    {
        return new List<EntityDistributionDefinition>();
    }
}

public class RequestChunkPacket : Packet
{
    public int X { get; set; }
    public int Y { get; set; }

    public RequestChunkPacket()
    {

    }

    public RequestChunkPacket(int x, int y)
    {
        this.X = x;
        this.Y = y;
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

public class ReceivedChunkPacket : Packet
{
    public int X { get; set; }
    public int Y { get; set; }

    public ReceivedChunkPacket()
    {

    }

    public ReceivedChunkPacket(int x, int y)
    {
        this.X = x;
        this.Y = y;
    }
}

public class UnloadChunkPacket : Packet
{
    public int X { get; set; }
    public int Y { get; set; }

    public UnloadChunkPacket()
    {

    }

    public UnloadChunkPacket(int x, int y)
    {
        this.X = x;
        this.Y = y;
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

public class ServerWorldGenerator : IWorldGenerator
{
    private ThreadSafe<Dictionary<ChunkAddress, Chunk>> _requestedChunks = new ThreadSafe<Dictionary<ChunkAddress, Chunk>>(new Dictionary<ChunkAddress, Chunk>());
    private GameClient _gameClient;

    public ServerWorldGenerator(GameClient client)
    {
        this._gameClient = client;
    }

    public Chunk GenerateChunk(int x, int y)
    {
        ChunkAddress chunkAddress = new ChunkAddress(x, y);

        while (_requestedChunks.LockedAction((rc) => { return rc[chunkAddress] == null; }))
        {

        }

        return _requestedChunks.LockedAction((rc) =>
        {
            Chunk c = rc[chunkAddress];
            rc.Remove(chunkAddress);
            return c;
        });
    }

    public async Task<Chunk> GenerateChunkAsync(int x, int y)
    {
        return await Task.Run(() =>
        {
            ChunkAddress chunkAddress = new ChunkAddress(x, y);

            while (_requestedChunks.LockedAction((rc) => { return rc[chunkAddress] == null; }))
            {

            }

            return _requestedChunks.LockedAction((rc) =>
            {
                Chunk c = rc[chunkAddress];
                rc.Remove(chunkAddress);
                return c;
            });
        });
    }

    public List<EntityDistributionDefinition> GetEntityDistributionDefinitions()
    {
        return new List<EntityDistributionDefinition>();
    }
}