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
        int[,] tileGrid = Utilities.CreateTileGridWith("game:dirt", Chunk.CHUNK_SIZE, Chunk.CHUNK_SIZE);

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
}

public class ServerWorldGenerator : IWorldGenerator
{
    private Dictionary<ChunkAddress, Chunk> _requestedChunks = new Dictionary<ChunkAddress, Chunk>();
    private GameClient _gameClient;

    public ServerWorldGenerator(GameClient client)
    {
        this._gameClient = client;
        _gameClient.AddPacketHandler<WholeChunkPacket>((packet) =>
        {
            _requestedChunks[new ChunkAddress(packet.X, packet.Y)] = packet.Chunk;
        });
    }

    public Chunk GenerateChunk(int x, int y)
    {
        this.RequestChunk(x, y);
        ChunkAddress chunkAddress = new ChunkAddress(x, y);

        while (_requestedChunks[chunkAddress] == null)
        {

        }

        Chunk c = _requestedChunks[chunkAddress];
        _requestedChunks.Remove(chunkAddress);

        GameConsole.WriteLine("CLIENT CHUNKY", "Generated chunk " + x + " " + y);
        return c;
    }

    public async Task<Chunk> GenerateChunkAsync(int x, int y)
    {
        this.RequestChunk(x, y);
        ChunkAddress chunkAddress = new ChunkAddress(x, y);

        await Task.Run(() =>
        {
            while (_requestedChunks[chunkAddress] == null)
            {

            }
        });

        Chunk c = _requestedChunks[chunkAddress];
        _requestedChunks.Remove(chunkAddress);

        GameConsole.WriteLine("CLIENT CHUNKY", "Generated chunk " + x + " " + y);
        return c;
    }

    private void RequestChunk(int x, int y)
    {
        ChunkAddress address = new ChunkAddress(x, y);
        if (!_requestedChunks.ContainsKey(address))
        {
            _requestedChunks.Add(address, null);
            _gameClient.EnqueuePacket(new RequestChunkPacket() { X = x, Y = y }, true, false);
        }
    }
}