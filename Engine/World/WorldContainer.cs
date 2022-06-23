using System.Text.Json;
using System.Threading.Tasks.Dataflow;
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

    // Private fields
    private bool _asynchronous;
    private BufferBlock<ChunkEvent> _chunkEvents = new BufferBlock<ChunkEvent>();

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
                ChunkAddress ca = new ChunkAddress(_x, _y);
                bool contains = Chunks.LockedAction<bool>((chunks) =>
                {
                    return chunks.ContainsKey(ca);
                });

                if (!contains)
                {
                    EnqueueChunkEvent(new ChunkEvent(new ChunkAddress(_x, _y), new ChunkEventGenerate()));
                }
            }
        }

        this.Chunks.LockedAction((chunks) =>
        {
            foreach (KeyValuePair<ChunkAddress, Chunk> chunk in chunks)
            {
                if (chunk.Key.X < fromX || chunk.Key.X > toX || chunk.Key.Y < fromY || chunk.Key.Y > toY)
                {
                    EnqueueChunkEvent(new ChunkEvent(chunk.Key, new ChunkEventRemove()));
                }
            }
        });
    }

    private void EnqueueChunkEvent(ChunkEvent ce)
    {
        _chunkEvents.SendAsync(ce);
    }

    public void Start()
    {
        _ = Task.Run(async () =>
        {
            while (true)
            {
                ChunkEvent ce = await _chunkEvents.ReceiveAsync(TimeSpan.FromMilliseconds(-1));
                await ce.ExecuteAsync(this);
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

    public string Serialize()
    {
        return Chunks.LockedAction((chunks) =>
        {
            var options = new JsonSerializerOptions()
            {
                IncludeFields = true
            };

            string json = JsonSerializer.Serialize(chunks.Values, options);
            return json;
        });
    }

    public void Deserialize(string json)
    {
        Chunks.LockedAction((chunks) =>
        {
            var options = new JsonSerializerOptions()
            {
                IncludeFields = true
            };

            var deserialized = JsonSerializer.Deserialize<List<Chunk>>(json, options);
            chunks.Clear();

            foreach (Chunk c in deserialized)
            {
                chunks.Add(new ChunkAddress(c.X, c.Y), c);
            }
        });
    }
}

public class ChunkEvent
{
    public ChunkAddress Address { get; set; }
    public IChunkEventExecutor Executor { get; set; }

    public ChunkEvent(ChunkAddress address, IChunkEventExecutor executor)
    {
        this.Address = address;
        this.Executor = executor;
    }

    public async Task ExecuteAsync(WorldContainer container)
    {
        await this.Executor.ExecuteAsync(container, this.Address);
    }
}

public interface IChunkEventExecutor
{
    Task ExecuteAsync(WorldContainer container, ChunkAddress address);
}

public class ChunkEventRemove : IChunkEventExecutor
{
    public async Task ExecuteAsync(WorldContainer container, ChunkAddress address)
    {
        container.Chunks.LockedAction((chunks) =>
        {
            chunks.Remove(address);
        });

        await Task.CompletedTask;
    }
}

public class ChunkEventGenerate : IChunkEventExecutor
{
    public async Task ExecuteAsync(WorldContainer container, ChunkAddress address)
    {
        await container.GenerateChunkAsync(address.X, address.Y);
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