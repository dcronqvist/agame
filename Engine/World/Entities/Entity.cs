using System.Drawing;
using System.Numerics;
using AGame.Engine.Graphics;

namespace AGame.Engine.World.Entities
{
    class Entity
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
        public Sprite Sprite { get; set; }
        public RectangleF CollisionBox
        {
            get
            {
                return Sprite.GetRectangle(this.Position);
            }
        }

        public Entity(Vector2 startPos, Sprite sprite)
        {
            this.Position = startPos;
            this.Sprite = sprite;
        }

        public virtual void Update()
        {
            this.Sprite.Update();
        }

        public virtual void Render()
        {
            this.Sprite.Render(this.Position);
        }
    }
}