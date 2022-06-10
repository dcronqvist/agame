using AGame.Engine.Graphics.Rendering;
using GameUDPProtocol;

namespace AGame.Engine.World;

public class WorldContainer
{
    // Chunks
    public ThreadSafe<Dictionary<ChunkAddress, Chunk>> Chunks { get; set; }
    public IWorldGenerator WorldGenerator { get; set; }

    private bool _asynchronous;

    public WorldContainer(IWorldGenerator generator, bool useAsync = false)
    {
        this.Chunks = new ThreadSafe<Dictionary<ChunkAddress, Chunk>>(new Dictionary<ChunkAddress, Chunk>());
        this.WorldGenerator = generator;
        this._asynchronous = useAsync;
    }

    public void AddChunk(int x, int y, Chunk chunk)
    {
        Chunks.LockedAction((chunks) =>
        {
            chunks[new ChunkAddress(x, y)] = chunk;
        });
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
        return await Task.Run(() =>
        {
            return this.GetChunk(x, y);
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
        //Chunks.Remove(new ChunkAddress(x, y));
    }

    public void GenerateChunkArea(int fromX, int toX, int fromY, int toY)
    {
        for (int x = fromX; x <= toX; x++)
        {
            for (int y = fromY; y <= toY; y++)
            {
                GenerateChunk(x, y);
            }
        }
    }

    public void MaintainChunkArea(int width, int height, int x, int y)
    {
        // Get/generate chunks in this area and discard chunks that aren't in this area
        int fromX = x - width;
        int toX = x + width;
        int fromY = y - height;
        int toY = y + height;

        for (int _x = fromX; _x <= toX; _x++)
        {
            for (int _y = fromY; _y <= toY; _y++)
            {
                GetChunk(_x, _y);
            }
        }

        this.Chunks.LockedAction((chunks) =>
        {
            foreach (KeyValuePair<ChunkAddress, Chunk> chunk in chunks)
            {
                if (chunk.Key.X < fromX || chunk.Key.X > toX || chunk.Key.Y < fromY || chunk.Key.Y > toY)
                {
                    chunks.Remove(chunk.Key);
                }
            }
        });
    }

    public async Task MaintainChunkAreaAsync(int width, int height, int x, int y)
    {
        // Get/generate chunks in this area and discard chunks that aren't in this area
        int fromX = x - width;
        int toX = x + width;
        int fromY = y - height;
        int toY = y + height;

        for (int _x = fromX; _x <= toX; _x++)
        {
            for (int _y = fromY; _y <= toY; _y++)
            {
                await GetChunkAsync(_x, _y);
            }
        }

        this.Chunks.LockedAction((chunks) =>
        {
            foreach (KeyValuePair<ChunkAddress, Chunk> chunk in chunks)
            {
                if (chunk.Key.X < fromX || chunk.Key.X > toX || chunk.Key.Y < fromY || chunk.Key.Y > toY)
                {
                    chunks.Remove(chunk.Key);
                }
            }
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
}