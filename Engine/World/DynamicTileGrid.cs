using System.Collections.Generic;
using System.Drawing;
using System.Numerics;
using AGame.Engine.Assets;
using AGame.Engine.Graphics;
using AGame.Engine.Graphics.Rendering;

namespace AGame.Engine.World;

public class DynamicTileGrid
{
    private Vector2 _globalOffset;
    private TileSet _tileSet;

    private DynamicInstancedTextureRenderer _ditr;
    private Dictionary<ChunkAddress, List<InstancingInfo>> _loadedChunks;
    private WorldContainer _parentWorld;
    private int _order;

    public DynamicTileGrid(TileSet tileSet, int order, Vector2 globalOffset, WorldContainer container)
    {
        this._parentWorld = container;
        this._order = order;
        this._tileSet = tileSet;
        this._globalOffset = globalOffset;
        this._ditr = new DynamicInstancedTextureRenderer(ModManager.GetAsset<Shader>("default.shader.dynamic_instanced_texture"), this._tileSet.GetTexture(), null);
        this._loadedChunks = new Dictionary<ChunkAddress, List<InstancingInfo>>();
    }

    public void RemoveChunk(int x, int y)
    {
        ChunkAddress ca = new ChunkAddress(x, y);
        if (this._loadedChunks.ContainsKey(ca))
        {
            this._ditr.RemoveInstances(this._loadedChunks[ca].ToArray());
            this._loadedChunks.Remove(ca);
        }
    }

    public void UpdateChunk(int x, int y, ChunkLayer chunkLayer)
    {
        ChunkAddress address = new ChunkAddress(x, y);

        List<InstancingInfo> instances = new List<InstancingInfo>();

        for (int _y = 0; _y < chunkLayer.Grid.GetLength(1); _y++)
        {
            for (int _x = 0; _x < chunkLayer.Grid.GetLength(0); _x++)
            {
                int value = chunkLayer.Grid[_x, _y];

                if (value == 1)
                {
                    Vector2 scale = Vector2.One * TileGrid.TILE_SIZE;
                    RectangleF rect = this._tileSet.GetRandomFullTile();

                    instances.Add(InstancingInfo.Create(this._globalOffset + new Vector2((x * Chunk.CHUNK_SIZE + _x) * TileGrid.TILE_SIZE, (y * Chunk.CHUNK_SIZE + _y) * TileGrid.TILE_SIZE), 0f, scale, Vector2.Zero, rect));

                    if (chunkLayer.UseSideTiles)
                    {
                        //this.AddSideTiles(ref instances, this._tileSet, scale, this._globalOffset, x, y, _x, _y, chunkLayer);
                    }
                }
            }
        }

        List<InstancingInfo> oldInfos = this._loadedChunks.ContainsKey(address) ? this._loadedChunks[address] : new List<InstancingInfo>();
        this._loadedChunks[address] = instances;

        foreach (InstancingInfo info in oldInfos)
        {
            this._ditr.RemoveInstances(info);
        }

        this._ditr.AddInstances(instances.ToArray());
    }

    private void AddSideTiles(ref List<InstancingInfo> infos, TileSet tileSet, Vector2 scale, Vector2 globalOffset, int chunkX, int chunkY, int tileX, int tileY, ChunkLayer chunkLayer)
    {
        int realX = chunkX * Chunk.CHUNK_SIZE + tileX;
        int realY = chunkY * Chunk.CHUNK_SIZE + tileY;

        bool topLeft = this._parentWorld.GetTileValue(realX - 1, realY - 1, chunkLayer.Order) == 1;
        bool top = this._parentWorld.GetTileValue(realX, realY - 1, chunkLayer.Order) == 1;
        bool topRight = this._parentWorld.GetTileValue(realX + 1, realY - 1, chunkLayer.Order) == 1;
        bool left = this._parentWorld.GetTileValue(realX - 1, realY, chunkLayer.Order) == 1;
        bool right = this._parentWorld.GetTileValue(realX + 1, realY, chunkLayer.Order) == 1;
        bool bottomLeft = this._parentWorld.GetTileValue(realX - 1, realY + 1, chunkLayer.Order) == 1;
        bool bottom = this._parentWorld.GetTileValue(realX, realY + 1, chunkLayer.Order) == 1;
        bool bottomRight = this._parentWorld.GetTileValue(realX + 1, realY + 1, chunkLayer.Order) == 1;

        if (!topLeft && !top && !left) // Top left outer corner
        {
            infos.Add(InstancingInfo.Create(globalOffset + new Vector2((realX - 1) * TileGrid.TILE_SIZE, (realY) * TileGrid.TILE_SIZE), 3f * MathF.PI / 2f, scale, new Vector2(0, tileSet.TileSize), tileSet.GetRandomOuterCornerTile()));
        }

        if (!topRight && !top && !right) // Top right outer corner
        {
            infos.Add(InstancingInfo.Create(globalOffset + new Vector2((realX + 1) * TileGrid.TILE_SIZE, (realY - 1) * TileGrid.TILE_SIZE), 0f, scale, new Vector2(tileSet.TileSize, 0), tileSet.GetRandomOuterCornerTile()));
        }

        if (!bottomLeft && !bottom && !left) // Bottom left outer corner
        {
            infos.Add(InstancingInfo.Create(globalOffset + new Vector2((realX) * TileGrid.TILE_SIZE, (realY + 2) * TileGrid.TILE_SIZE), 2f * MathF.PI / 2f, scale, new Vector2(0, tileSet.TileSize), tileSet.GetRandomOuterCornerTile()));
        }

        if (!bottomRight && !bottom && !right) // Bottom right outer corner
        {
            infos.Add(InstancingInfo.Create(globalOffset + new Vector2((realX + 2) * TileGrid.TILE_SIZE, (realY + 1) * TileGrid.TILE_SIZE), MathF.PI / 2f, scale, new Vector2(tileSet.TileSize, 0), tileSet.GetRandomOuterCornerTile()));
        }

        if (!top)
        {
            infos.Add(InstancingInfo.Create(globalOffset + new Vector2((realX) * TileGrid.TILE_SIZE, (realY - 1) * TileGrid.TILE_SIZE), 0f, scale, new Vector2(0, tileSet.TileSize), tileSet.GetRandomSideTile()));
        }

        if (!left)
        {
            infos.Add(InstancingInfo.Create(globalOffset + new Vector2((realX - 1) * TileGrid.TILE_SIZE, (realY + 1) * TileGrid.TILE_SIZE), 3f * MathF.PI / 2f, scale, new Vector2(0, tileSet.TileSize), tileSet.GetRandomSideTile()));
        }

        if (!right)
        {
            infos.Add(InstancingInfo.Create(globalOffset + new Vector2((realX + 2) * TileGrid.TILE_SIZE, (realY) * TileGrid.TILE_SIZE), MathF.PI / 2f, scale, new Vector2(tileSet.TileSize, 0), tileSet.GetRandomSideTile()));
        }

        if (!bottom)
        {
            infos.Add(InstancingInfo.Create(globalOffset + new Vector2((realX + 1) * TileGrid.TILE_SIZE, (realY + 2) * TileGrid.TILE_SIZE), MathF.PI, scale, new Vector2(tileSet.TileSize, 0), tileSet.GetRandomSideTile()));
        }
    }

    public void Render()
    {
        this._ditr.Render(ColorF.White);
    }
}
