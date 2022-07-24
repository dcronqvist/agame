using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text.Json;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;
using AGame.Engine.Assets;
using AGame.Engine.Configuration;
using AGame.Engine.Graphics;
using AGame.Engine.Graphics.Rendering;
using GameUDPProtocol;

namespace AGame.Engine.World;

public class WorldContainer
{
    // Chunks
    public ThreadSafe<Dictionary<ChunkAddress, Chunk>> Chunks { get; set; }
    public IWorldGenerator WorldGenerator { get; set; }
    private bool _rendering;

    public SortedDictionary<int, DynamicTileGrid> WorldLayers { get; set; }

    // Events
    public event EventHandler<ChunkUpdatedEventArgs> ChunkUpdated;
    public event EventHandler<ChunkGeneratedEventArgs> ChunkGenerated;

    public WorldContainer(bool rendering, IWorldGenerator generator)
    {
        this._rendering = rendering;
        this.Chunks = new ThreadSafe<Dictionary<ChunkAddress, Chunk>>(new Dictionary<ChunkAddress, Chunk>());
        this.WorldGenerator = generator;
        this.WorldLayers = new SortedDictionary<int, DynamicTileGrid>();
    }

    public void AddChunk(int x, int y, Chunk chunk)
    {
        Chunks.LockedAction((chunks) =>
        {
            chunks[new ChunkAddress(x, y)] = chunk;
            chunk.ParentWorld = this;

            // Reset all chunk layers around this chunk
            // for (int i = -1; i <= 1; i++)
            // {
            //     for (int j = -1; j <= 1; j++)
            //     {
            //         if (i == 0 && j == 0)
            //         {
            //             continue;
            //         }

            //         ChunkAddress ca = new ChunkAddress(x + i, y + j);
            //         if (chunks.ContainsKey(ca))
            //         {
            //             chunks[ca].ResetLayers();
            //         }
            //     }
            // }
        });
    }

    public void UpdateTile(int x, int y, string tileName)
    {
        // // Assume x and y to be tile coordinates
        // int cx = x > 0 ? x / Chunk.CHUNK_SIZE : -((-x + Chunk.CHUNK_SIZE - 1) / Chunk.CHUNK_SIZE);
        // int cy = y > 0 ? y / Chunk.CHUNK_SIZE : -((-y + Chunk.CHUNK_SIZE - 1) / Chunk.CHUNK_SIZE);

        // int cTopLeftX = cx * Chunk.CHUNK_SIZE;
        // int cTopLeftY = cy * Chunk.CHUNK_SIZE;

        // int diffX = (x - cTopLeftX);
        // int diffY = (y - cTopLeftY);

        // bool hasChunk = Chunks.LockedAction<bool>((chunks) =>
        // {
        //     ChunkAddress ca = new ChunkAddress(cx, cy);
        //     return chunks.ContainsKey(ca);
        // });

        // if (!hasChunk)
        // {
        //     return; // Do nothing if we don't have the chunk
        // }

        // Chunk c = GetChunk(cx, cy);

        // int tileID = TileManager.GetTileIDFromName(tileName);
        // Tile tile = TileManager.GetTileFromID(tileID);

        // TileType tileType = tile.Type;

        // switch (tileType)
        // {
        //     case TileType.Ground:
        //         c.GroundLayer[diffX, diffY] = tileID;
        //         break;
        // }

        // ChunkUpdated?.Invoke(this, new ChunkUpdatedEventArgs(c));
    }

    public void UpdateChunk(int x, int y, Chunk chunk)
    {
        Chunks.LockedAction((chunks) =>
        {
            chunks[new ChunkAddress(x, y)] = chunk;
            chunk.ParentWorld = this;

            if (this._rendering)
            {
                List<ChunkLayer> layers = chunk.Layers;

                foreach (ChunkLayer layer in layers)
                {
                    if (!this.WorldLayers.ContainsKey(layer.Order))
                    {
                        this.WorldLayers.Add(layer.Order, new DynamicTileGrid(ModManager.GetAsset<TileSet>(layer.TileSet), layer.Order, Vector2.Zero, this));
                    }

                    DynamicTileGrid dtg = this.WorldLayers[layer.Order];

                    dtg.UpdateChunk(x, y, layer);
                }

                //Update all chunks around this chunk
                // for (int i = -1; i <= 1; i++)
                // {
                //     for (int j = -1; j <= 1; j++)
                //     {
                //         if (i == 0 && j == 0)
                //         {
                //             continue;
                //         }

                //         ChunkAddress ca = new ChunkAddress(x + i, y + j);
                //         if (chunks.ContainsKey(ca))
                //         {
                //             foreach (ChunkLayer layer in chunks[ca].Layers)
                //             {
                //                 DynamicTileGrid dtg = this.WorldLayers[layer.Order];

                //                 dtg.UpdateChunk(x + i, y + j, layer);
                //             }
                //         }
                //     }
                // }
            }
        });

        ChunkUpdated?.Invoke(this, new ChunkUpdatedEventArgs(chunk));
    }

    public Chunk GetChunkNoGenerate(int x, int y)
    {
        return Chunks.LockedAction<Chunk>((chunks) =>
        {
            ChunkAddress addr = new ChunkAddress(x, y);

            if (!chunks.ContainsKey(addr))
            {
                return null;
            }

            return chunks[addr];
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
        this.ChunkGenerated?.Invoke(this, new ChunkGeneratedEventArgs(chunk));
        AddChunk(x, y, chunk);
        return chunk;
    }

    public async Task<Chunk> GenerateChunkAsync(int x, int y)
    {
        Chunk chunk = await this.WorldGenerator.GenerateChunkAsync(x, y);
        this.ChunkGenerated?.Invoke(this, new ChunkGeneratedEventArgs(chunk));
        AddChunk(x, y, chunk);
        return chunk;
    }

    public void DiscardChunk(int x, int y)
    {
        Chunks.LockedAction((chunks) =>
        {
            chunks.Remove(new ChunkAddress(x, y));

            foreach (KeyValuePair<int, DynamicTileGrid> kvp in this.WorldLayers)
            {
                kvp.Value.RemoveChunk(x, y);
            }
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

    private void RenderChunkBorder(Chunk chunk)
    {
        Vector2 topLeft = new Vector2(chunk.X * Chunk.CHUNK_SIZE * TileGrid.TILE_SIZE, chunk.Y * Chunk.CHUNK_SIZE * TileGrid.TILE_SIZE);
        Vector2 topRight = new Vector2((chunk.X + 1) * Chunk.CHUNK_SIZE * TileGrid.TILE_SIZE, chunk.Y * Chunk.CHUNK_SIZE * TileGrid.TILE_SIZE);
        Vector2 bottomLeft = new Vector2(chunk.X * Chunk.CHUNK_SIZE * TileGrid.TILE_SIZE, (chunk.Y + 1) * Chunk.CHUNK_SIZE * TileGrid.TILE_SIZE);
        Vector2 bottomRight = new Vector2((chunk.X + 1) * Chunk.CHUNK_SIZE * TileGrid.TILE_SIZE, (chunk.Y + 1) * Chunk.CHUNK_SIZE * TileGrid.TILE_SIZE);

        Renderer.Primitive.RenderLine(topLeft, topRight, 1, ColorF.Red);
        Renderer.Primitive.RenderLine(topRight, bottomRight, 1, ColorF.Red);
        Renderer.Primitive.RenderLine(bottomRight, bottomLeft, 1, ColorF.Red);
        Renderer.Primitive.RenderLine(bottomLeft, topLeft, 1, ColorF.Red);
    }

    public int GetTileValue(int x, int y, int order)
    {
        CoordinateVector cv = new CoordinateVector(x, y);
        ChunkAddress ca = cv.ToChunkAddress();

        Chunk chunk = GetChunkNoGenerate(ca.X, ca.Y);
        if (chunk == null)
        {
            return 0;
        }

        int inChunkX = x - ca.X * Chunk.CHUNK_SIZE;
        int inChunkY = y - ca.Y * Chunk.CHUNK_SIZE;

        return chunk.GetTileValue(order, inChunkX % Chunk.CHUNK_SIZE, inChunkY % Chunk.CHUNK_SIZE);
    }

    public void Render()
    {
        List<Chunk> chunks = this.Chunks.LockedAction((chunks) => chunks.Values.ToList());

        foreach (KeyValuePair<int, DynamicTileGrid> kvp in this.WorldLayers.Reverse())
        {
            kvp.Value.Render();
        }

        // foreach (Chunk chunk in chunks)
        // {
        //     RenderChunkBorder(chunk);
        // }
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

public class ChunkGeneratedEventArgs : EventArgs
{
    public Chunk Chunk { get; set; }

    public ChunkGeneratedEventArgs(Chunk chunk)
    {
        this.Chunk = chunk;
    }
}