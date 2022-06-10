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
    private Dictionary<int, List<Vector2>> TileIDAndPositions { get; set; }
    private Dictionary<int, float[]> TileIDAndModelMatrices { get; set; }
    private Dictionary<int, StaticInstancedTextureRenderer> TileIDToRenderer { get; set; }

    public StaticTileGrid(int[,] grid, Vector2 globalOffset) : base(grid.GetLength(0), grid.GetLength(1), globalOffset)
    {
        TileIDAndPositions = new Dictionary<int, List<Vector2>>();
        TileIDToRenderer = new Dictionary<int, StaticInstancedTextureRenderer>();
        // render all tiles
        for (int y = 0; y < Height; y++)
        {
            for (int x = 0; x < Width; x++)
            {
                // Render each tile
                if (grid[x, y] != -1)
                {
                    Vector2 tilePos = new Vector2(TILE_SIZE * x, TILE_SIZE * y) + GlobalOffset;
                    if (!TileIDAndPositions.ContainsKey(grid[x, y]))
                    {
                        TileIDAndPositions.Add(grid[x, y], new List<Vector2>());
                    }

                    TileIDAndPositions[grid[x, y]].Add(tilePos);
                }
            }
        }

        TileIDAndModelMatrices = new Dictionary<int, float[]>();
        foreach (KeyValuePair<int, List<Vector2>> kvp in TileIDAndPositions)
        {
            Tile t = TileManager.GetTileFromID(kvp.Key);

            List<Vector2> positions = kvp.Value;
            List<Matrix4x4> matrices = new List<Matrix4x4>();
            for (int i = 0; i < positions.Count; i++)
            {
                Matrix4x4 transPos = Matrix4x4.CreateTranslation(new Vector3(positions[i], 0.0f));
                Matrix4x4 rot = Matrix4x4.CreateRotationZ(0f);
                RectangleF sourceRect = new RectangleF(0, 0, 16, 16);
                Vector2 scale = Vector2.One * (TILE_SIZE / t.GetTexture().Width);
                Matrix4x4 mscale = Matrix4x4.CreateScale(new Vector3(new Vector2(sourceRect.Width, sourceRect.Height) * scale, 1.0f));

                matrices.Add(mscale * rot * transPos);
            }

            TileIDToRenderer.Add(kvp.Key, new StaticInstancedTextureRenderer(AssetManager.GetAsset<Shader>("shader_texture"), TileManager.GetTileFromID(kvp.Key).GetTexture(), new RectangleF(0, 0, 16, 16), matrices.ToArray()));
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
        Vector2 position = TILE_SIZE * new Vector2(x, y) + GlobalOffset;
        foreach (KeyValuePair<int, List<Vector2>> kvp in TileIDAndPositions)
        {
            foreach (Vector2 pos in kvp.Value)
            {
                if (pos == position)
                {
                    return kvp.Key;
                }
            }
        }
        return -1;
    }

    public override void Render()
    {
        foreach (KeyValuePair<int, StaticInstancedTextureRenderer> kvp in TileIDToRenderer)
        {
            Tile t = TileManager.GetTileFromID(kvp.Key);
            kvp.Value.Render(t.GetTexture(), ColorF.White);
        }
    }
}
