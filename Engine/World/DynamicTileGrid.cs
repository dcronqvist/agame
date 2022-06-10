using System.Collections.Generic;
using System.Drawing;
using System.Numerics;
using AGame.Engine.Assets;
using AGame.Engine.Graphics;
using AGame.Engine.Graphics.Rendering;

namespace AGame.Engine.World
{
    public class DynamicTileGrid : TileGrid
    {
        private Dictionary<int, List<Vector2>> TileIDAndPositions { get; set; }
        private Dictionary<int, DynamicInstancedTextureRenderer> TileIDToRenderer { get; set; }

        public DynamicTileGrid(int[,] grid, Vector2 globalOffset) : base(grid.GetLength(0), grid.GetLength(1), globalOffset)
        {
            this.TileIDAndPositions = new Dictionary<int, List<Vector2>>();
            this.TileIDToRenderer = new Dictionary<int, DynamicInstancedTextureRenderer>();

            for (int y = 0; y < Height; y++)
            {
                for (int x = 0; x < Width; x++)
                {
                    // Render each tile
                    if (grid[x, y] != -1)
                    {
                        Vector2 tilePos = new Vector2(TILE_SIZE * x, TILE_SIZE * y) + this.GlobalOffset;
                        if (!TileIDAndPositions.ContainsKey(grid[x, y]))
                        {
                            TileIDAndPositions.Add(grid[x, y], new List<Vector2>());
                        }

                        TileIDAndPositions[grid[x, y]].Add(tilePos);
                    }
                }
            }

            foreach (KeyValuePair<int, List<Vector2>> kvp in TileIDAndPositions)
            {
                Tile t = TileManager.GetTileFromID(kvp.Key);

                List<Vector2> positions = kvp.Value;
                List<Matrix4x4> matrices = new List<Matrix4x4>();
                for (int i = 0; i < positions.Count; i++)
                {
                    // Matrix4x4 transPos = Matrix4x4.CreateTranslation(new Vector3(positions[i], 0.0f));
                    // Matrix4x4 rot = Matrix4x4.CreateRotationZ(0f);
                    // RectangleF sourceRect = new RectangleF(0, 0, 16, 16);
                    // Vector2 scale = Vector2.One * (TILE_SIZE / t.Texture.Width);
                    // Matrix4x4 mscale = Matrix4x4.CreateScale(new Vector3(new Vector2(sourceRect.Width, sourceRect.Height) * scale, 1.0f));
                    Matrix4x4 matrix = Utilities.CreateModelMatrixFromPosition(positions[i], new Vector2(TileGrid.TILE_SIZE));
                    matrices.Add(matrix);
                }

                TileIDToRenderer.Add(kvp.Key, new DynamicInstancedTextureRenderer(AssetManager.GetAsset<Shader>("shader_texture"), TileManager.GetTileFromID(kvp.Key).GetTexture(), new RectangleF(0, 0, 16, 16), matrices.ToArray()));
            }
        }

        public int GetTileIDAtPosition(int x, int y)
        {
            Vector2 pos = new Vector2(x, y) * TileGrid.TILE_SIZE + GlobalOffset;

            foreach (KeyValuePair<int, List<Vector2>> kvp in this.TileIDAndPositions)
            {
                for (int i = 0; i < kvp.Value.Count; i++)
                {
                    if (kvp.Value[i] == pos)
                    {
                        return kvp.Key;
                    }
                }
            }

            return -1;
        }

        public bool RemoveTile(int x, int y)
        {
            int tileId = this.GetTileIDAtPosition(x, y);

            if (tileId != -1)
            {
                Vector2 pos = new Vector2(x, y) * TileGrid.TILE_SIZE + GlobalOffset;
                Matrix4x4 mat = Utilities.CreateModelMatrixFromPosition(pos, new Vector2(TileGrid.TILE_SIZE));

                this.TileIDToRenderer[tileId].RemoveMatrix(mat);

                this.TileIDAndPositions[tileId].Remove(pos);
                return true;
            }
            else
            {
                return false;
            }
        }

        public void SetTile(int x, int y, int tileID)
        {
            if (this.GetTileIDAtPosition(x, y) == tileID)
            {
                return;
            }

            RemoveTile(x, y);
            Vector2 worldPosition = new Vector2(x, y) * TileGrid.TILE_SIZE + GlobalOffset;

            if (!this.TileIDAndPositions.ContainsKey(tileID))
            {
                this.TileIDAndPositions.Add(tileID, new List<Vector2>());
            }

            this.TileIDAndPositions[tileID].Add(worldPosition);

            Matrix4x4 modelMatrix = Utilities.CreateModelMatrixFromPosition(worldPosition, new Vector2(TileGrid.TILE_SIZE));

            if (!this.TileIDToRenderer.ContainsKey(tileID))
            {
                this.TileIDToRenderer.Add(tileID, new DynamicInstancedTextureRenderer(AssetManager.GetAsset<Shader>("shader_texture"), TileManager.GetTileFromID(tileID).GetTexture(), new RectangleF(0, 0, 16, 16), new Matrix4x4[] { modelMatrix }));
            }
            else
            {
                DynamicInstancedTextureRenderer ditr = this.TileIDToRenderer[tileID];
                ditr.AddMatrix(modelMatrix);
            }
        }

        public override void Render()
        {
            foreach (KeyValuePair<int, DynamicInstancedTextureRenderer> kvp in TileIDToRenderer)
            {
                kvp.Value.Render(ColorF.White);
            }
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
    }
}