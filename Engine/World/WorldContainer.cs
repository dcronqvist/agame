using System.Text.Json;
using System.Threading.Tasks.Dataflow;
using AGame.Engine.Configuration;
using AGame.Engine.Graphics.Rendering;
using GameUDPProtocol;

namespace AGame.Engine.World;

public class WorldContainer
{
    // Chunks
    public ThreadSafe<Dictionary<ChunkAddress, Chunk>> Chunks { get; set; }
    public IWorldGenerator WorldGenerator { get; set; }

    // Events
    public event EventHandler<ChunkUpdatedEventArgs> ChunkUpdated;

    public WorldContainer(IWorldGenerator generator)
    {
        this.Chunks = new ThreadSafe<Dictionary<ChunkAddress, Chunk>>(new Dictionary<ChunkAddress, Chunk>());
        this.WorldGenerator = generator;
    }

    public void AddChunk(int x, int y, Chunk chunk)
    {
        Chunks.LockedAction((chunks) =>
        {
            chunks[new ChunkAddress(x, y)] = chunk;
        });
    }

    public void UpdateTile(int x, int y, string tileName)
    {
        // Assume x and y to be tile coordinates
        int cx = x > 0 ? x / Chunk.CHUNK_SIZE : -((-x + Chunk.CHUNK_SIZE - 1) / Chunk.CHUNK_SIZE);
        int cy = y > 0 ? y / Chunk.CHUNK_SIZE : -((-y + Chunk.CHUNK_SIZE - 1) / Chunk.CHUNK_SIZE);

        int cTopLeftX = cx * Chunk.CHUNK_SIZE;
        int cTopLeftY = cy * Chunk.CHUNK_SIZE;

        int diffX = (x - cTopLeftX);
        int diffY = (y - cTopLeftY);

        bool hasChunk = Chunks.LockedAction<bool>((chunks) =>
        {
            ChunkAddress ca = new ChunkAddress(cx, cy);
            return chunks.ContainsKey(ca);
        });

        if (!hasChunk)
        {
            return; // Do nothing if we don't have the chunk
        }

        Chunk c = GetChunk(cx, cy);

        int tileID = TileManager.GetTileIDFromName(tileName);
        Tile tile = TileManager.GetTileFromID(tileID);

        TileType tileType = tile.Type;

        switch (tileType)
        {
            case TileType.Ground:
                c.GroundLayer[diffX, diffY] = tileID;
                break;
        }

        ChunkUpdated?.Invoke(this, new ChunkUpdatedEventArgs(c));
    }

    public void UpdateChunk(int x, int y, Chunk chunk)
    {
        Chunks.LockedAction((chunks) =>
        {
            chunks[new ChunkAddress(x, y)] = chunk;
        });

        ChunkUpdated?.Invoke(this, new ChunkUpdatedEventArgs(chunk));
    }

    public Chunk GetChunk(int x, int y)
    {
        return Chunks.LockedAction<Chunk>((chunks) =>
        {
            ChunkAddress addr = new ChunkAddress(x, y);

            if (!chunks.ContainsKey(addr))
            {
                this.GenerateChunk(x, y);
            }

            return chunks[addr];
        });
    }

    public async Task<Chunk> GetChunkAsync(int x, int y)
    {
        return await Chunks.LockedAction<Task<Chunk>>(async (chunks) =>
        {
            ChunkAddress addr = new ChunkAddress(x, y);

            if (!chunks.ContainsKey(addr))
            {
                await this.GenerateChunkAsync(x, y);
            }

            return chunks[addr];
        });
    }

    public Chunk GenerateChunk(int x, int y)
    {
        Chunk chunk = this.WorldGenerator.GenerateChunk(x, y);
        AddChunk(x, y, chunk);
        return chunk;
    }

    public async Task<Chunk> GenerateChunkAsync(int x, int y)
    {
        Chunk chunk = await this.WorldGenerator.GenerateChunkAsync(x, y);
        AddChunk(x, y, chunk);
        return chunk;
    }

    public void DiscardChunk(int x, int y)
    {
        Chunks.LockedAction((chunks) =>
        {
            chunks.Remove(new ChunkAddress(x, y));
        });
    }

    public async Task DiscardChunkAsync(int x, int y)
    {
        await Task.Run(() =>
        {
            Chunks.LockedAction((chunks) =>
            {
                chunks.Remove(new ChunkAddress(x, y));
            });
        });
    }

    public void Render()
    {
        Chunks.LockedAction((chunks) =>
        {
            foreach (Chunk chunk in chunks.Values)
            {
                chunk.Render();
            }
        });
    }

    public string Serialize()
    {
        var options = new JsonSerializerOptions()
        {
            IncludeFields = true
        };

        string json = JsonSerializer.Serialize(Chunks.Value.Values, options);
        return json;
    }

    public void Deserialize(string json)
    {
        var options = new JsonSerializerOptions()
        {
            IncludeFields = true
        };

        var deserialized = JsonSerializer.Deserialize<List<Chunk>>(json, options);
        Chunks.Value.Clear();

        Chunks.LockedAction((chunks) =>
        {
            foreach (Chunk c in deserialized)
            {
                chunks.Add(new ChunkAddress(c.X, c.Y), c);
            }
        });
    }
}
public class ChunkUpdatedEventArgs : EventArgs
{
    public Chunk Chunk { get; set; }

    public ChunkUpdatedEventArgs(Chunk chunk)
    {
        this.Chunk = chunk;
    }
}