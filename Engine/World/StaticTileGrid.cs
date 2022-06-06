using System;
using System.Collections.Generic;
using System.Drawing;
using System.Numerics;
using AGame.Engine;
using AGame.Engine.Assets;
using AGame.Engine.Graphics;
using AGame.Engine.Graphics.Cameras;
using AGame.Engine.Graphics.Rendering;
using AGame.Engine.World;

namespace AGame.World
{
    public class StaticTileGrid : TileGrid
    {
        private Dictionary<int, List<Vector2>> TileIDAndPositions { get; set; }
        private Dictionary<int, float[]> TileIDAndModelMatrices { get; set; }
        private Dictionary<int, StaticInstancedTextureRenderer> TileIDToRenderer { get; set; }

        public StaticTileGrid(int[,] grid) : base(grid)
        {
            TileIDAndPositions = new Dictionary<int, List<Vector2>>();
            TileIDToRenderer = new Dictionary<int, StaticInstancedTextureRenderer>();
            // render all tiles
            for (int y = 0; y < Height; y++)
            {
                for (int x = 0; x < Width; x++)
                {
                    // Render each tile
                    if (GridOfIDs[x, y] != 0)
                    {
                        Vector2 tilePos = new Vector2(TILE_SIZE * x, TILE_SIZE * y);
                        if (!TileIDAndPositions.ContainsKey(GridOfIDs[x, y]))
                        {
                            TileIDAndPositions.Add(GridOfIDs[x, y], new List<Vector2>());
                        }

                        TileIDAndPositions[GridOfIDs[x, y]].Add(tilePos);
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
                    Vector2 scale = Vector2.One * (TILE_SIZE / t.Texture.Width);
                    Matrix4x4 mscale = Matrix4x4.CreateScale(new Vector3(new Vector2(sourceRect.Width, sourceRect.Height) * scale, 1.0f));

                    matrices.Add(mscale * rot * transPos);
                }

                TileIDToRenderer.Add(kvp.Key, new StaticInstancedTextureRenderer(AssetManager.GetAsset<Shader>("shader_texture"), TileManager.GetTileFromID(kvp.Key).Texture, new RectangleF(0, 0, 16, 16), matrices.ToArray()));
            }
        }

        public void Render()
        {
            RectangleF visibleArea = Renderer.Camera.VisibleArea;

            // int minX = (int)(visibleArea.X / tileSize);
            // int maxX = (int)((visibleArea.X + visibleArea.Width) / tileSize);

            // int minY = (int)(visibleArea.Y / tileSize);
            // int maxY = (int)((visibleArea.Y + visibleArea.Height) / tileSize);

            // Dictionary<int, List<Vector2>> tileIDAndPositions = new Dictionary<int, List<Vector2>>();

            // // render all tiles
            // for (int y = Math.Max(minY, 0); y < Math.Min(this.Height, maxY + 1); y++)
            // {
            //     for (int x = Math.Max(minX, 0); x < Math.Min(this.Width, maxX + 1); x++)
            //     {
            //         // Render each tile
            //         if (GridOfIDs[x, y] != 0)
            //         {
            //             Vector2 tilePos = new Vector2(tileSize * x, tileSize * y);
            //             if (!tileIDAndPositions.ContainsKey(GridOfIDs[x, y]))
            //             {
            //                 tileIDAndPositions.Add(GridOfIDs[x, y], new List<Vector2>());
            //             }

            //             tileIDAndPositions[GridOfIDs[x, y]].Add(tilePos);
            //         }
            //     }
            // }

            foreach (KeyValuePair<int, StaticInstancedTextureRenderer> kvp in TileIDToRenderer)
            {
                Tile t = TileManager.GetTileFromID(kvp.Key);
                kvp.Value.Render(t.Texture, ColorF.White);
                //Renderer.Texture.RenderInstanced(t.Texture, kvp.Value, ColorF.White, new RectangleF(0, 0, 16, 16));
            }
        }
    }
}