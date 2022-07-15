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
    public RectangleF[] Frames { get; set; }
    public float Rotation { get; set; }
    private int _currentFrame;

    private float _currentFrameTime;

    public Animation(string texture, Vector2 renderScale, Vector2 origin, RectangleF[] frames, float rotation)
    {
        Texture = texture;
        RenderScale = renderScale;
        Origin = origin;
        Frames = frames;
        Rotation = rotation;
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

    public Vector2 GetMiddleOfCurrentFrameScaled()
    {
        RectangleF frame = this.Frames[this._currentFrame];
        return new Vector2(frame.Width / 2, frame.Height / 2) * this.RenderScale;
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
    public bool Update(float frameTime, float deltaTime)
    {
        _currentFrameTime += deltaTime;
        if (_currentFrameTime >= frameTime)
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

    public void Render(Vector2 position, ColorF tint, TextureRenderEffects effects)
    {
        Renderer.Texture.Render(this.GetTexture(), position, this.RenderScale, this.Rotation, tint, this.Origin, this.GetFrame(this._currentFrame), effects);
    }

    public Animation Clone()
    {
        return new Animation(this.Texture, this.RenderScale, this.Origin, this.Frames, this.Rotation);
    }
}
