using System;
using System.Collections.Generic;
using System.Drawing;
using System.Numerics;
using AGame.Engine.Assets;
using AGame.Engine.GLFW;
using AGame.Engine.Graphics;

namespace AGame.Engine.World.Entities
{
    class Player : AnimatorEntity
    {
        public Player(Vector2 startPos) : base(startPos,
                                            new Animator(new Dictionary<string, Animation>() {
                                                { "idle", new Animation(AssetManager.GetAsset<Texture2D>("tex_player"), Vector2.One * 2f, Vector2.Zero, ColorF.White, new RectangleF(0, 0, 32, 16), 0f, 1, 2) },
                                                { "run", new Animation(AssetManager.GetAsset<Texture2D>("tex_player"), Vector2.One * 2f, Vector2.Zero, ColorF.White, new RectangleF(32, 0, 48, 16), 0f, 11, 3) }
                                            }, "idle"))
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
            {
                vel = Vector2.Normalize(vel);
                this.animator.SetAnimation("run");
            }
            else
            {
                this.animator.SetAnimation("idle");
            }
            this.Position += vel;

            base.Update();
        }
    }
}