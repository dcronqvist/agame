using System.Collections.Generic;
using System.Drawing;
using System.Numerics;
using System.Text;
using System.Text.Json.Serialization;
using AGame.Engine.Assets;
using AGame.Engine.Graphics;
using AGame.Engine.Graphics.Rendering;
using GameUDPProtocol;

namespace AGame.Engine.Graphics;

public class Animation
{
    private Texture2D _texture;
    public string Texture { get; set; }
    public Vector2 RenderScale { get; set; }
    public Vector2 Origin { get; set; }
    public ColorF ColorTint { get; set; }
    public RectangleF[] Frames { get; set; }
    public float Rotation { get; set; }
    public int FramesPerSecond { get; set; }
    private int _currentFrame;

    private float _currentFrameTime;

    public Animation(string texture, Vector2 renderScale, Vector2 origin, ColorF colorTint, RectangleF[] frames, float rotation, int framesPerSecond)
    {
        Texture = texture;
        RenderScale = renderScale;
        Origin = origin;
        ColorTint = colorTint;
        Frames = frames;
        Rotation = rotation;
        FramesPerSecond = framesPerSecond;
        this._currentFrame = 0;
    }

    public RectangleF GetFrame(int frame)
    {
        return this.Frames[frame];
    }

    private float GetFrameTime(int fps)
    {
        return 1f / fps;
    }

    public void Reset()
    {
        this._currentFrame = 0;
    }

    private Texture2D GetTexture()
    {
        if (_texture == null)
        {
            _texture = ModManager.GetAsset<Texture2D>(Texture);
        }
        return _texture;
    }

    /// <summary>
    /// Updates the animation with the supplied time delta.
    /// if the animation is finished, it will return true, otherwise false.
    /// </summary>
    public bool Update(float deltaTime)
    {
        _currentFrameTime += deltaTime;
        if (_currentFrameTime >= GetFrameTime(FramesPerSecond))
        {
            _currentFrameTime = 0;
            _currentFrame++;
            if (_currentFrame >= Frames.Length)
            {
                _currentFrame = 0;
                return true;
            }
        }
        return false;
    }

    public void Render(Vector2 position)
    {
        Renderer.Texture.Render(this.GetTexture(), position, this.RenderScale, this.Rotation, this.ColorTint, this.Origin, this.GetFrame(_currentFrame));
    }

    public Animation Clone()
    {
        return new Animation(this.Texture, this.RenderScale, this.Origin, this.ColorTint, this.Frames, this.Rotation, this.FramesPerSecond);
    }
}
