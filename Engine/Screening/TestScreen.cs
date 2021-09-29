using System;
using System.Drawing;
using System.Numerics;
using AGame.Engine;
using AGame.Engine.Assets;
using AGame.Engine.DebugTools;
using AGame.Engine.GLFW;
using AGame.Engine.Graphics;
using AGame.Engine.Graphics.Cameras;
using AGame.Engine.Graphics.Rendering;
using AGame.Engine.Screening;
using AGame.Engine.World;
using AGame.World;

namespace AGame.Engine.Screening
{
    class TestScreen : Screen
    {
        TileGrid tg;
        TileGrid tg2;

        Camera2D camera2D;

        RenderTexture world;
        RenderTexture lights;

        RenderTexture combined;

        public TestScreen() : base("testscreen")
        {
            camera2D = new Camera2D(Vector2.Zero, 1f);
        }

        public override Screen Initialize()
        {
            this.world = new RenderTexture(DisplayManager.GetWindowSizeInPixels());
            this.combined = new RenderTexture(DisplayManager.GetWindowSizeInPixels());
            this.lights = new RenderTexture(DisplayManager.GetWindowSizeInPixels());

            tg = new TileGrid(1000, 1000);

            Tile[,] tiles = new Tile[10, 10];

            for (int i = 0; i < 10; i++)
            {
                for (int j = 0; j < 10; j++)
                {
                    if (j == i)
                        tiles[j, i] = new Tile("tex_dirt", false);
                }
            }

            tg2 = new TileGrid(0, 0, grid: tiles);

            return this;
        }

        public override void OnEnter()
        {

        }

        public override void OnLeave()
        {

        }

        public override void Update()
        {
            float xSpeed = 0f, ySpeed = 0f;

            if (Input.IsKeyDown(Keys.A))
            {
                xSpeed -= 2f;
            }
            if (Input.IsKeyDown(Keys.D))
            {
                xSpeed += 2f;
            }
            if (Input.IsKeyDown(Keys.W))
            {
                ySpeed -= 2f;
            }
            if (Input.IsKeyDown(Keys.S))
            {
                ySpeed += 2f;
            }

            camera2D.FocusPosition += new Vector2(xSpeed, ySpeed);
        }

        public override void Render()
        {
            Renderer.SetRenderTarget(this.world, camera2D);
            Renderer.Clear(ColorF.Black);

            tg.Render();
            tg2.Render();


            Renderer.SetRenderTarget(this.lights, camera2D);
            Renderer.Clear(ColorF.Transparent);
            Vector2 topLeft = this.camera2D.TopLeft;
            Renderer.Primitive.RenderRectangle(new RectangleF(topLeft.X, topLeft.Y, 1280, 720), ColorF.Black * 0.9f);

            Renderer.Primitive.RenderCircle(Input.GetMousePosition(camera2D), 700.0f, ColorF.White, true);

            Renderer.Primitive.RenderCircle(new Vector2(20, 30), 600.0f + MathF.Sin(GameTime.TotalElapsedSeconds * 0.9f) * 80f, ColorF.White, true);
            Renderer.Primitive.RenderCircle(new Vector2(220, 123), 600.0f + MathF.Sin(GameTime.TotalElapsedSeconds * 0.7f) * 60f, ColorF.White, true);
            Renderer.Primitive.RenderCircle(new Vector2(721, 500), 600.0f + MathF.Sin(GameTime.TotalElapsedSeconds * 1.2f) * 50f, ColorF.White, true);
            Renderer.Primitive.RenderCircle(new Vector2(799, 250), 600.0f + MathF.Sin(GameTime.TotalElapsedSeconds) * 100f, ColorF.White, true);

            Renderer.SetRenderTarget(combined, camera2D);
            Renderer.Clear(ColorF.Transparent);

            //Renderer.RenderRenderTexture(this.world);


            Renderer.RenderRenderTexture(this.world, this.lights, camera2D.TopLeft, Vector2.Zero, Vector2.One, 0f, ColorF.White, AssetManager.GetAsset<Shader>("shader_rend_tex_mult"));

            Renderer.SetRenderTarget(null, null);
            Renderer.Clear(ColorF.Black);
            Renderer.RenderRenderTexture(this.combined);
        }
    }
}