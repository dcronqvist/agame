// using System;
// using System.Collections.Generic;
// using System.Drawing;
// using System.Numerics;
// using AGame.Engine.Assets;
// using AGame.Engine.GLFW;
// using AGame.Engine.Graphics;
// using AGame.Engine.Graphics.Rendering;

// namespace AGame.Engine.World.Entities
// {
//     public class Player : AnimatorEntity
//     {
//         public Player(Vector2 startPos) : base(startPos,
//                                             new Animator(new Dictionary<string, Animation>() {
//                                                 { "idle", new Animation(AssetManager.GetAsset<Texture2D>("tex_krobus"), Vector2.One * 2f, Vector2.Zero, ColorF.White, new RectangleF(0, 0, 16, 24), 0f, 1, 1, new RectangleF(2, 16, 12, 8)) },
//                                                 { "run_right", new Animation(AssetManager.GetAsset<Texture2D>("tex_krobus"), Vector2.One * 2f, Vector2.Zero, ColorF.White, new RectangleF(0, 24, 64, 24), 0f, 11, 4, new RectangleF(2, 16, 12, 8)) },
//                                                 { "run_left", new Animation(AssetManager.GetAsset<Texture2D>("tex_krobus"), Vector2.One * 2f, Vector2.Zero, ColorF.White, new RectangleF(0, 72, 64, 24), 0f, 11, 4, new RectangleF(2, 16, 12, 8)) },
//                                                 { "run_down", new Animation(AssetManager.GetAsset<Texture2D>("tex_krobus"), Vector2.One * 2f, Vector2.Zero, ColorF.White, new RectangleF(0, 0, 64, 24), 0f, 11, 4, new RectangleF(2, 16, 12, 8)) },
//                                                 { "run_up", new Animation(AssetManager.GetAsset<Texture2D>("tex_krobus"), Vector2.One * 2f, Vector2.Zero, ColorF.White, new RectangleF(0, 48, 64, 24), 0f, 11, 4, new RectangleF(2, 16, 12, 8)) }
//                                             }, "idle"), true, 0.12f)
//         {

//         }

//         public override void Update(Crater crater)
//         {
//             Vector2 vel = Vector2.Zero;

//             if (Input.IsKeyDown(Keys.A))
//             {
//                 vel += new Vector2(-1, 0);
//             }
//             if (Input.IsKeyDown(Keys.D))
//             {
//                 vel += new Vector2(1, 0);
//             }
//             if (Input.IsKeyDown(Keys.W))
//             {
//                 vel += new Vector2(0, -1);
//             }
//             if (Input.IsKeyDown(Keys.S))
//             {
//                 vel += new Vector2(0, 1);
//             }
//             if (vel.LengthSquared() > 0f)
//             {
//                 vel = Vector2.Normalize(vel);

//                 int y = Math.Sign(vel.Y);
//                 int x = Math.Sign(vel.X);

//                 if (y == 1)
//                 {
//                     // Going down
//                     if (x == 1)
//                     {
//                         this.animator.SetAnimation("run_right"); // Going down right
//                     }
//                     else if (x == -1)
//                     {
//                         this.animator.SetAnimation("run_left"); // Going down left
//                     }
//                     else
//                     {
//                         this.animator.SetAnimation("run_down");
//                     }
//                 }
//                 else if (y == -1)
//                 {
//                     // Going up
//                     if (x == 1)
//                     {
//                         this.animator.SetAnimation("run_right"); // Going up right
//                     }
//                     else if (x == -1)
//                     {
//                         this.animator.SetAnimation("run_left"); // Going up left
//                     }
//                     else
//                     {
//                         this.animator.SetAnimation("run_up");
//                     }
//                 }
//                 else if (y == 0)
//                 {
//                     // Going up
//                     if (x == 1)
//                     {
//                         this.animator.SetAnimation("run_right"); // Going up right
//                     }
//                     else if (x == -1)
//                     {
//                         this.animator.SetAnimation("run_left"); // Going up left
//                     }
//                 }
//             }
//             else
//             {
//                 this.animator.SetAnimation("idle");
//             }

//             vel = vel * 180f * GameTime.DeltaTime;
//             this.TargetVelocity = vel;

//             base.Update(crater);
//         }
//     }
// }