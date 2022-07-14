using System;
using System.Collections.Generic;
using System.Drawing;
using System.Numerics;
using AGame.Engine;
using AGame.Engine.Assets;
using AGame.Engine.Graphics;
using AGame.Engine.Graphics.Cameras;
using AGame.Engine.Graphics.Rendering;

namespace AGame.Engine.World;

public class StaticTileGrid : TileGrid
{
    private TileSet _tileSet;
    private int[,] grid;

    private StaticInstancedTextureRenderer sitr;
    private List<InstancingInfo> siis;

    public StaticTileGrid(TileSet tileSet, int[,] grid, Vector2 globalOffset, int[,] outerGrid) : base(grid.GetLength(0), grid.GetLength(1), globalOffset)
    {
        this._tileSet = tileSet;
        this.grid = grid;
        siis = new List<InstancingInfo>();
        for (int _y = 0; _y < grid.GetLength(1); _y++)
        {
            for (int _x = 0; _x < grid.GetLength(0); _x++)
            {
                int value = grid[_x, _y];

                if (value == 1)
                {
                    Vector2 scale = Vector2.One * TILE_SIZE;
                    RectangleF rect = tileSet.GetRandomFullTile();

                    siis.Add(InstancingInfo.Create(globalOffset + new Vector2(_x * TileGrid.TILE_SIZE, _y * TileGrid.TILE_SIZE), 0f, scale, Vector2.Zero, rect));

                    // Do bitmasking and stuff
                    this.AddSideTiles(ref siis, tileSet, scale, globalOffset, _x, _y, grid, outerGrid);
                }
            }
        }
    }

    private void AddSideTiles(ref List<InstancingInfo> infos, TileSet tileSet, Vector2 scale, Vector2 globalOffset, int tileX, int tileY, int[,] grid, int[,] outerGrid)
    {
        int ValueAt(int x, int y)
        {
            if (x < 0 || x >= grid.GetLength(0))
            {
                return outerGrid[x + 1, y + 1];
            }
            if (y < 0 || y >= grid.GetLength(1))
            {
                return outerGrid[x + 1, y + 1];
            }
            return grid[x, y];
        }

        bool topLeft = ValueAt(tileX - 1, tileY - 1) == 1;
        bool top = ValueAt(tileX, tileY - 1) == 1;
        bool topRight = ValueAt(tileX + 1, tileY - 1) == 1;
        bool left = ValueAt(tileX - 1, tileY) == 1;
        bool right = ValueAt(tileX + 1, tileY) == 1;
        bool bottomLeft = ValueAt(tileX - 1, tileY + 1) == 1;
        bool bottom = ValueAt(tileX, tileY + 1) == 1;
        bool bottomRight = ValueAt(tileX + 1, tileY + 1) == 1;

        if (!topLeft && !top && !left) // Top left outer corner
        {
            infos.Add(InstancingInfo.Create(globalOffset + new Vector2((tileX - 1) * TILE_SIZE, (tileY) * TILE_SIZE), 3f * MathF.PI / 2f, scale, new Vector2(0, tileSet.TileSize), tileSet.GetRandomOuterCornerTile()));
        }

        if (!topRight && !top && !right) // Top right outer corner
        {
            infos.Add(InstancingInfo.Create(globalOffset + new Vector2((tileX + 1) * TILE_SIZE, (tileY - 1) * TILE_SIZE), 0f, scale, new Vector2(tileSet.TileSize, 0), tileSet.GetRandomOuterCornerTile()));
        }

        if (!bottomLeft && !bottom && !left) // Bottom left outer corner
        {
            infos.Add(InstancingInfo.Create(globalOffset + new Vector2((tileX) * TILE_SIZE, (tileY + 2) * TILE_SIZE), 2f * MathF.PI / 2f, scale, new Vector2(0, tileSet.TileSize), tileSet.GetRandomOuterCornerTile()));
        }

        if (!bottomRight && !bottom && !right) // Bottom right outer corner
        {
            infos.Add(InstancingInfo.Create(globalOffset + new Vector2((tileX + 2) * TILE_SIZE, (tileY + 1) * TILE_SIZE), MathF.PI / 2f, scale, new Vector2(tileSet.TileSize, 0), tileSet.GetRandomOuterCornerTile()));
        }

        if (!top)
        {
            infos.Add(InstancingInfo.Create(globalOffset + new Vector2((tileX) * TILE_SIZE, (tileY - 1) * TILE_SIZE), 0f, scale, new Vector2(0, tileSet.TileSize), tileSet.GetRandomSideTile()));
        }

        if (!left)
        {
            infos.Add(InstancingInfo.Create(globalOffset + new Vector2((tileX - 1) * TILE_SIZE, (tileY + 1) * TILE_SIZE), 3f * MathF.PI / 2f, scale, new Vector2(0, tileSet.TileSize), tileSet.GetRandomSideTile()));
        }

        if (!right)
        {
            infos.Add(InstancingInfo.Create(globalOffset + new Vector2((tileX + 2) * TILE_SIZE, (tileY) * TILE_SIZE), MathF.PI / 2f, scale, new Vector2(tileSet.TileSize, 0), tileSet.GetRandomSideTile()));
        }

        if (!bottom)
        {
            infos.Add(InstancingInfo.Create(globalOffset + new Vector2((tileX + 1) * TILE_SIZE, (tileY + 2) * TILE_SIZE), MathF.PI, scale, new Vector2(tileSet.TileSize, 0), tileSet.GetRandomSideTile()));
        }
    }

    public int[,] GetGrid()
    {
        int[,] grid = new int[Width, Height];
        for (int y = 0; y < Height; y++)
        {
            for (int x = 0; x < Width; x++)
            {
                grid[x, y] = GetTileIDAtPos(x, y);
            }
        }

        return grid;
    }

    public override string GetTileNameAtPos(int x, int y)
    {
        return TileManager.GetTileNameFromID(this.GetTileIDAtPos(x, y));
    }

    public override int GetTileIDAtPos(int x, int y)
    {
        return -1;
    }

    public override void Render()
    {
        if (this.sitr == null)
        {
            this.sitr = new StaticInstancedTextureRenderer(ModManager.GetAsset<Shader>("default.shader.instanced_texture"), this._tileSet.GetTexture(), siis);
        }

        this.sitr.Render(ColorF.White);
    }
}
