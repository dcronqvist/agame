using System;
using System.Drawing;
using System.Numerics;
using AGame.Engine.Graphics;
using AGame.Engine.Graphics.Rendering;

namespace AGame.Engine.World.Entities
{
    public class Entity
    {
        public Vector2 Position { get; set; }
        public Vector2 MiddleOfSpritePosition
        {
            get
            {
                return Position + new Vector2(this.Sprite.SourceRectangle.Width * this.Sprite.RenderScale.X, this.Sprite.SourceRectangle.Height * this.Sprite.RenderScale.Y) / 2f;
            }
        }
        public Vector2 Velocity { get; set; }
        public Vector2 TargetVelocity { get; set; }
        public float MovementTweenFactor { get; set; }
        public Sprite Sprite { get; set; }
        public RectangleF CollisionBox
        {
            get
            {
                return Sprite.GetRectangle(this.Position);
            }
        }
        public bool CollidesWithSolidTiles { get; set; }

        public Entity(Vector2 startPos, Sprite sprite, bool collidesSolids, float movementTweenFactor = 1f)
        {
            this.Position = startPos;
            this.Sprite = sprite;
            this.CollidesWithSolidTiles = collidesSolids;
            this.MovementTweenFactor = movementTweenFactor;
        }

        public virtual void Update(Crater crater)
        {
            this.Velocity += (this.TargetVelocity - this.Velocity) * MovementTweenFactor;

            if (this.CollidesWithSolidTiles)
            {
                // X collisions
                if (crater.CheckCollisionWithCrater(this.CollisionBox.Offset(new Vector2(this.Velocity.X, 0f)), true, false, true))
                {
                    //this.Position = this.Position.Round();
                    while (!crater.CheckCollisionWithCrater(this.CollisionBox.Offset(new Vector2(Math.Sign(this.Velocity.X), 0)), true, false, true))
                    {
                        this.Position += new Vector2(Math.Sign(this.Velocity.X), 0);
                    }

                    this.TargetVelocity = new Vector2(0f, this.TargetVelocity.Y);
                    this.Velocity = new Vector2(0f, this.Velocity.Y);
                }

                // Y collisions
                if (crater.CheckCollisionWithCrater(this.CollisionBox.Offset(new Vector2(0f, this.Velocity.Y)), true, false, true))
                {
                    //this.Position = this.Position.Round();
                    while (!crater.CheckCollisionWithCrater(this.CollisionBox.Offset(new Vector2(0f, Math.Sign(this.Velocity.Y))), true, false, true))
                    {
                        this.Position += new Vector2(0f, Math.Sign(this.Velocity.Y));
                    }

                    this.TargetVelocity = new Vector2(this.TargetVelocity.X, 0f);
                    this.Velocity = new Vector2(this.Velocity.X, 0f);
                }
            }

            this.Sprite.Update();

            if (this.Velocity.AbsLength() < 0.01f)
            {
                this.Velocity = Vector2.Zero;
            }

            this.Position += this.Velocity;
        }

        public virtual void Render(Crater crater)
        {
            this.Sprite.Render(this.Position);
            if (Debug.DrawEntityCollisionBoxes)
            {
                Renderer.Primitive.RenderRectangle(CollisionBox, ColorF.Blue * 0.3f);
            }
        }
    }
}