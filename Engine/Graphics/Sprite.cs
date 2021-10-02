using System.Drawing;
using System.Numerics;
using AGame.Engine.Assets;
using AGame.Engine.Graphics.Rendering;

namespace AGame.Engine.Graphics
{
    class Sprite
    {
        public Texture2D Texture { get; set; }
        public Vector2 RenderScale { get; set; }
        public Vector2 Origin { get; set; }
        public ColorF ColorTint { get; set; }
        public RectangleF SourceRectangle { get; set; }
        public float Rotation { get; set; }

        public Sprite(Texture2D texture, Vector2 renderScale, Vector2 origin, ColorF colorTint, RectangleF sourceRectangle, float rotation)
        {
            Texture = texture;
            RenderScale = renderScale;
            Origin = origin;
            ColorTint = colorTint;
            SourceRectangle = sourceRectangle;
            Rotation = rotation;
        }

        public virtual void Update()
        {

        }

        public RectangleF GetRectangle(Vector2 position)
        {
            return new RectangleF(position.X, position.Y, SourceRectangle.Width * RenderScale.X, SourceRectangle.Height * RenderScale.Y);
        }

        public virtual void Render(Vector2 position)
        {
            Renderer.Texture.Render(this.Texture, position, this.RenderScale, this.Rotation, this.ColorTint, this.Origin, this.SourceRectangle);
        }
    }
}