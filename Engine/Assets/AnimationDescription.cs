using System.Drawing;
using System.Numerics;
using AGame.Engine.Graphics;

namespace AGame.Engine.Assets;

public class AnimationDescription : Asset
{
    public string Texture { get; set; }
    public Vector2 RenderScale { get; set; }
    public Vector2 Origin { get; set; }
    public ColorF ColorTint { get; set; }
    public RectangleF[] Frames { get; set; }
    public float Rotation { get; set; }

    public override bool InitOpenGL()
    {
        // Do nothing.
        return true;
    }

    public Animation GetAnimation()
    {
        return new Animation(
            this.Texture,
            RenderScale,
            Origin,
            ColorTint,
            Frames,
            Rotation);
    }
}