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
        public string Name { get; set; }
        public StaticTileGrid BackgroundLayer { get; set; }
        public DynamicTileGrid ResourceLayer { get; set; }
        public DynamicTileGrid BuildingLayer { get; set; }
        public DynamicTileGrid ForegroundLayer { get; set; }
        public float SlopeAngle { get; set; }

        private RenderTexture _renderTexture;

        public Crater(int seed, ICraterGenerator generator)
        {
            this.BackgroundLayer = generator.GenerateBackgroundLayer(seed);
            this.ResourceLayer = generator.GenerateResourceLayer(seed);
            this.BuildingLayer = new DynamicTileGrid(new int[50, 50]);
            Utilities.InitRNG(seed);
            this.SlopeAngle = Utilities.GetRandomFloat(0f, 2f * MathF.PI);

            _renderTexture = new RenderTexture(DisplayManager.GetWindowSizeInPixels());
        }

        public bool CheckCollisionWithCrater(RectangleF rectInCrater, bool backgroundCheck = true, bool resourceCheck = false, bool buildingCheck = false)
        {
            bool background = backgroundCheck && CheckCollisionWithGrid(rectInCrater, BackgroundLayer);
            bool resource = resourceCheck && CheckCollisionWithGrid(rectInCrater, ResourceLayer);
            //bool building = buildingCheck && CheckCollisionWithGrid(rectInCrater, BuildingLayer);

            return background || resource;// || building;
        }

        public bool CheckCollisionWithGrid(RectangleF rectInCrater, TileGrid grid)
        {
            return GetCollisionsWithGrid(rectInCrater, grid).Length > 0;
        }

        public RectangleF[] GetCollisionsWithGrid(RectangleF rectInCrater, TileGrid grid)
        {
            int margin = 2;

            int minX = (int)(rectInCrater.X / StaticTileGrid.TILE_SIZE) - margin + 1;
            int maxX = (int)((rectInCrater.X + rectInCrater.Width) / StaticTileGrid.TILE_SIZE) + margin;
            int minY = (int)(rectInCrater.Y / StaticTileGrid.TILE_SIZE) - margin + 1;
            int maxY = (int)((rectInCrater.Y + rectInCrater.Height) / StaticTileGrid.TILE_SIZE) + margin;

            List<RectangleF> recs = new List<RectangleF>();

            for (int y = minY; y < maxY; y++)
            {
                for (int x = minX; x < maxX; x++)
                {
                    Tile t = TileManager.GetTileFromID(grid.GridOfIDs[x, y]);
                    if (t.Solid)
                    {
                        RectangleF r = new RectangleF(x * StaticTileGrid.TILE_SIZE, y * StaticTileGrid.TILE_SIZE, StaticTileGrid.TILE_SIZE, StaticTileGrid.TILE_SIZE);
                        if (rectInCrater.IntersectsWith(r))
                        {
                            recs.Add(r);
                        }
                    }
                }
            }

            return recs.ToArray();
        }

        public void Update()
        {
            if (Input.IsMouseButtonPressed(GLFW.MouseButton.Left))
            {
                int x = ResourceLayer.GetTileXFromPosition(Input.GetMousePosition(Renderer.Camera));
                int y = ResourceLayer.GetTileYFromPosition(Input.GetMousePosition(Renderer.Camera));

                this.BuildingLayer.SetTile(x, y, 2);
            }
            if (Input.IsMouseButtonPressed(GLFW.MouseButton.Right))
            {
                int x = ResourceLayer.GetTileXFromPosition(Input.GetMousePosition(Renderer.Camera));
                int y = ResourceLayer.GetTileYFromPosition(Input.GetMousePosition(Renderer.Camera));

                this.BuildingLayer.RemoveTile(x, y);
            }
        }

        public Vector2 AbsoluteMiddle()
        {
            float a = (BackgroundLayer.Height * StaticTileGrid.TILE_SIZE) / 2f;
            return new Vector2(a, a);
        }

        public RenderTexture Render(Camera2D camera)
        {
            Renderer.SetRenderTarget(this._renderTexture, camera);
            Renderer.Clear(ColorF.Orange);

            BackgroundLayer.Render();
            //ResourceLayer.Render();
            BuildingLayer.Render();
            //ForegroundLayer.Render();

            int x = ResourceLayer.GetTileXFromPosition(Input.GetMousePosition(camera));
            int y = ResourceLayer.GetTileYFromPosition(Input.GetMousePosition(camera));

            Renderer.Primitive.RenderRectangle(new RectangleF(x * StaticTileGrid.TILE_SIZE, y * StaticTileGrid.TILE_SIZE, StaticTileGrid.TILE_SIZE, StaticTileGrid.TILE_SIZE), ColorF.BlueGray * 0.5f);

            Renderer.SetRenderTarget(null, null);
            return this._renderTexture;
        }
    }
}