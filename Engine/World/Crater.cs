using System;
using System.Collections.Generic;
using System.Drawing;
using System.Numerics;
using AGame.Engine.Assets;
using AGame.Engine.Graphics;
using AGame.Engine.Graphics.Cameras;
using AGame.Engine.Graphics.Rendering;
using AGame.Engine.World.Entities;
using AGame.World;

namespace AGame.Engine.World
{
    public class Crater
    {
        public StaticTileGrid GroundLayer { get; set; }
        public TileGrid grid;

        public Crater()
        {

            int[,] g = new int[100, 100];

            for (int y = 0; y < 100; y++)
            {
                for (int x = 0; x < 100; x++)
                {
                    g[x, y] = 1;
                }
            }

            GroundLayer = new StaticTileGrid(g);
            this.grid = new TileGrid(new int[100, 100]);
        }

        public bool CheckCollisionWithCrater(RectangleF rectInCrater, bool backgroundCheck = true, bool buildingCheck = false)
        {
            bool background = backgroundCheck && CheckCollisionWithGrid(rectInCrater, this.GroundLayer);
            bool building = buildingCheck && CheckCollisionWithGrid(rectInCrater, this.grid);
            //bool building = buildingCheck && CheckCollisionWithGrid(rectInCrater, BuildingLayer);

            return background || building;// || building;
        }

        public bool CheckCollisionWithGrid(RectangleF rectInCrater, TileGrid grid)
        {
            return GetCollisionsWithGrid(rectInCrater, grid).Length > 0;
        }

        public RectangleF[] GetCollisionsWithGrid(RectangleF rectInCrater, TileGrid grid)
        {
            int margin = 2;

            int minX = (int)(rectInCrater.X / TileGrid.TILE_SIZE) - margin + 1;
            int maxX = (int)((rectInCrater.X + rectInCrater.Width) / TileGrid.TILE_SIZE) + margin;
            int minY = (int)(rectInCrater.Y / TileGrid.TILE_SIZE) - margin + 1;
            int maxY = (int)((rectInCrater.Y + rectInCrater.Height) / TileGrid.TILE_SIZE) + margin;

            List<RectangleF> recs = new List<RectangleF>();

            for (int y = minY; y < maxY; y++)
            {
                for (int x = minX; x < maxX; x++)
                {
                    if (grid.GridOfIDs[x, y] != 0)
                    {
                        Tile t = TileManager.GetTileFromID(grid.GridOfIDs[x, y]);
                        if (t.Solid)
                        {
                            RectangleF r = new RectangleF(x * TileGrid.TILE_SIZE, y * TileGrid.TILE_SIZE, TileGrid.TILE_SIZE, TileGrid.TILE_SIZE);
                            if (rectInCrater.IntersectsWith(r))
                            {
                                recs.Add(r);
                            }
                        }
                    }
                }
            }

            return recs.ToArray();
        }

        public void Render()
        {
            this.GroundLayer.Render();
        }

        public IRenderable[] GetRenderables()
        {
            RectangleF visibleArea = Renderer.Camera.VisibleArea;
            IRenderable[] tileRenderables = this.grid.GetTileRenderablesInRect(visibleArea);
            return tileRenderables;
        }
    }
}