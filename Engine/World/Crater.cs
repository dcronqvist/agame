using System;
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
    class Crater
    {
        public string Name { get; set; }
        public TileGrid BackgroundLayer { get; set; }
        public TileGrid ResourceLayer { get; set; }
        public TileGrid BuildingLayer { get; set; }
        public TileGrid ForegroundLayer { get; set; }
        public float SlopeAngle { get; set; }

        private RenderTexture _renderTexture;

        public Crater(int seed, ICraterGenerator generator)
        {
            // Initially only one 
            this.BackgroundLayer = generator.GenerateBackgroundLayer(seed);
            this.ResourceLayer = generator.GenerateResourceLayer(seed);
            Utilities.InitRNG(seed);
            this.SlopeAngle = Utilities.GetRandomFloat(0f, 2f * MathF.PI);

            _renderTexture = new RenderTexture(DisplayManager.GetWindowSizeInPixels());
        }

        public void Update()
        {

        }

        public Vector2 AbsoluteMiddle()
        {
            float a = (BackgroundLayer.Height * TileGrid.TILE_SIZE) / 2f;
            return new Vector2(a, a);
        }

        public RenderTexture Render(Camera2D camera)
        {
            Renderer.SetRenderTarget(this._renderTexture, camera);
            Renderer.Clear(new ColorF(1.0f, 0f, 0f, 1.0f));

            BackgroundLayer.Render();
            ResourceLayer.Render();
            //BuildingLayer.Render();
            //ForegroundLayer.Render();

            int x = ResourceLayer.GetTileXFromPosition(Input.GetMousePosition(camera));
            int y = ResourceLayer.GetTileYFromPosition(Input.GetMousePosition(camera));

            Renderer.Primitive.RenderRectangle(new RectangleF(x * TileGrid.TILE_SIZE, y * TileGrid.TILE_SIZE, TileGrid.TILE_SIZE, TileGrid.TILE_SIZE), ColorF.BlueGray * 0.5f);

            Renderer.SetRenderTarget(null, null);
            return this._renderTexture;
        }
    }
}