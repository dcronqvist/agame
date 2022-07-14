using System.Numerics;
using AGame.Engine.Configuration;
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
        // _gameClient.AddPacketHandler<WholeChunkPacket>((packet) =>
        // {
        //     Logging.Log(LogLevel.Debug, $"Client: Received chunk {packet.X}, {packet.Y}");
        //     _requestedChunks.LockedAction((rc) =>
        //     {
        //         rc[new ChunkAddress(packet.X, packet.Y)] = packet.Chunk;
        //     });

        //     _gameClient.EnqueuePacket(new ReceivedChunkPacket(packet.X, packet.Y), true, false);
        // });
    }

    public Chunk GenerateChunk(int x, int y)
    {
        //this.RequestChunk(x, y);
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
            //this.RequestChunk(x, y);
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

    private void RequestChunk(int x, int y)
    {
        // ChunkAddress address = new ChunkAddress(x, y);
        // _requestedChunks.LockedAction((rc) =>
        // {
        //     if (!rc.ContainsKey(address))
        //     {
        //         _gameClient.EnqueuePacket(new RequestChunkPacket() { X = x, Y = y }, true, false);
        //         rc.Add(address, null);
        //     }
        // });
    }
}