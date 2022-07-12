using System.Drawing;
using System.Numerics;
using System.Text.Json.Serialization;
using AGame.Engine.Assets;
using AGame.Engine.Graphics.Rendering;

namespace AGame.Engine.Graphics
{
    public class Sprite
    {
        public Texture2D Texture { get; set; }
        public Vector2 RenderScale { get; set; }
        public Vector2 Origin { get; set; }
        public ColorF ColorTint { get; set; }
        public RectangleF SourceRectangle { get; set; }
        public Vector2 MiddleOfSourceRectangle
        {
            get
            {
                return new Vector2(SourceRectangle.Width / 2f, SourceRectangle.Height / 2f);
            }
        }
        public float Rotation { get; set; }
        public RectangleF CollisionBox { get; set; }

        [JsonConstructor]
        public Sprite()
        {

        }

        public Sprite(Texture2D texture, Vector2 renderScale, Vector2 origin, ColorF colorTint, RectangleF sourceRectangle, float rotation, RectangleF collisionBox)
        {
            Texture = texture;
            RenderScale = renderScale;
            Origin = origin;
            ColorTint = colorTint;
            SourceRectangle = sourceRectangle;
            Rotation = rotation;
            CollisionBox = collisionBox;
        }

        public virtual int GetWidth()
        {
            return (int)(this.Texture.Width * this.RenderScale.X);
        }

        public virtual int GetHeight()
        {
            return (int)(this.Texture.Height * this.RenderScale.Y);
        }

        public virtual void Update()
        {

        }

        public RectangleF GetCollisionBox(Vector2 position)
        {
            return new RectangleF(position.X + this.CollisionBox.X * RenderScale.X, position.Y + this.CollisionBox.Y * RenderScale.Y, this.CollisionBox.Width * RenderScale.X, this.CollisionBox.Height * RenderScale.Y);
        }

        public virtual void Render(Vector2 position)
        {
            Renderer.Texture.Render(this.Texture, position, this.RenderScale, this.Rotation, this.ColorTint, this.Origin, this.SourceRectangle);
        }
    }
}