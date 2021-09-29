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
        Camera2D camera2D;
        Crater crater;

        public TestScreen() : base("testscreen")
        {
            camera2D = new Camera2D(Vector2.Zero, 1f);
        }

        public override Screen Initialize()
        {
            crater = new Crater(new TestingGenerator());

            Input.OnScroll += (sender, scroll) =>
            {
                if (scroll > 0)
                {
                    this.camera2D.Zoom *= 1.1f;
                }
                else
                {
                    this.camera2D.Zoom *= 1f / 1.1f;
                }
            };

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
            Renderer.SetRenderTarget(null, null);
            Renderer.Clear(ColorF.Orange);
            RenderTexture crt = crater.Render(this.camera2D);

            Renderer.RenderRenderTexture(crt);

            // Renderer.SetRenderTarget(this.world, camera2D);
            // Renderer.Clear(ColorF.Black);

            // tg.Render();
            // tg2.Render();


            // Renderer.SetRenderTarget(this.lights, camera2D);
            // Renderer.Clear(ColorF.Transparent);
            // Vector2 topLeft = this.camera2D.TopLeft;
            // Renderer.Primitive.RenderRectangle(new RectangleF(topLeft.X, topLeft.Y, 1280, 720), ColorF.Black * 0.9f);

            // Renderer.Primitive.RenderCircle(Input.GetMousePosition(camera2D), 700.0f, ColorF.White, true);

            // Renderer.Primitive.RenderCircle(new Vector2(20, 30), 600.0f + MathF.Sin(GameTime.TotalElapsedSeconds * 0.9f) * 80f, ColorF.White, true);
            // Renderer.Primitive.RenderCircle(new Vector2(220, 123), 600.0f + MathF.Sin(GameTime.TotalElapsedSeconds * 0.7f) * 60f, ColorF.White, true);
            // Renderer.Primitive.RenderCircle(new Vector2(721, 500), 600.0f + MathF.Sin(GameTime.TotalElapsedSeconds * 1.2f) * 50f, ColorF.White, true);
            // Renderer.Primitive.RenderCircle(new Vector2(799, 250), 600.0f + MathF.Sin(GameTime.TotalElapsedSeconds) * 100f, ColorF.White, true);

            // Renderer.SetRenderTarget(combined, camera2D);
            // Renderer.Clear(ColorF.Transparent);

            // //Renderer.RenderRenderTexture(this.world);


            // Renderer.RenderRenderTexture(this.world, this.lights, camera2D.TopLeft, Vector2.Zero, Vector2.One, 0f, ColorF.White, AssetManager.GetAsset<Shader>("shader_rend_tex_mult"));

            // Renderer.SetRenderTarget(null, null);
            // Renderer.Clear(ColorF.Black);
            // Renderer.RenderRenderTexture(this.combined);
        }
    }
}