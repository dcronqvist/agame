using System.Numerics;
using AGame.Engine.Assets;
using AGame.Engine.GLFW;
using AGame.Engine.Graphics;

namespace AGame.Engine.World.Entities
{
    class Player : Entity
    {
        public Player(Vector2 startPos) : base(startPos, new Sprite(AssetManager.GetAsset<Texture2D>("tex_player"), Vector2.One * 2f, Vector2.Zero, ColorF.White, new System.Drawing.RectangleF(0, 0, 16, 16), 0f))
        {
        }

        public override void Render()
        {
            base.Render();
        }

        public override void Update()
        {
            Vector2 vel = Vector2.Zero;

            if (Input.IsKeyDown(Keys.A))
            {
                vel += new Vector2(-1, 0);
            }
            if (Input.IsKeyDown(Keys.D))
            {
                vel += new Vector2(1, 0);
            }
            if (Input.IsKeyDown(Keys.W))
            {
                vel += new Vector2(0, -1);
            }
            if (Input.IsKeyDown(Keys.S))
            {
                vel += new Vector2(0, 1);
            }
            if (vel.LengthSquared() > 0f)
                vel = Vector2.Normalize(vel);
            this.Position += vel;

            base.Update();
        }
    }
}