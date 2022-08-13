using System;
using System.Collections.Generic;
using System.Drawing;
using System.Numerics;
using System.Text;
using AGame.Engine;
using AGame.Engine.Assets;
using AGame.Engine.Assets.Scripting;
using AGame.Engine.ECSys;
using AGame.Engine.Graphics;
using AGame.Engine.Networking;
using AGame.Engine.World;

namespace DefaultMod;

[ComponentNetworking(CreateTriggersNetworkUpdate = true, UpdateTriggersNetworkUpdate = true), ScriptType(Name = "sprite_component")]
public class SpriteComponent : Component
{
    private string _texture;
    [ComponentProperty(0, typeof(StringPacker), typeof(StringInterpolator), InterpolationType.ToInstant)]
    public string Texture
    {
        get => _texture;
        set
        {
            if (_texture != value)
            {
                _texture = value;
                this.NotifyPropertyChanged();
            }
        }
    }

    private Vector2 _renderScale;
    [ComponentProperty(1, typeof(Vector2Packer), typeof(Vector2Interpolator), InterpolationType.Linear)]
    public Vector2 RenderScale
    {
        get => _renderScale;
        set
        {
            if (_renderScale != value)
            {
                _renderScale = value;
                this.NotifyPropertyChanged();
            }
        }
    }

    private Vector2 _origin;
    [ComponentProperty(2, typeof(Vector2Packer), typeof(Vector2Interpolator), InterpolationType.Linear)]
    public Vector2 Origin
    {
        get => _origin;
        set
        {
            if (_origin != value)
            {
                _origin = value;
                this.NotifyPropertyChanged();
            }
        }
    }

    private ColorF _colorTint;
    [ComponentProperty(3, typeof(ColorFPacker), typeof(ColorFInterpolator), InterpolationType.Linear)]
    public ColorF ColorTint
    {
        get => _colorTint;
        set
        {
            if (!_colorTint.Equals(value))
            {
                _colorTint = value;
                this.NotifyPropertyChanged();
            }
        }
    }

    private RectangleF _SourceRectangle;
    [ComponentProperty(4, typeof(RectangleFPacker), typeof(RectangleFInterpolator), InterpolationType.Linear)]
    public RectangleF SourceRectangle
    {
        get => _SourceRectangle;
        set
        {
            if (_SourceRectangle != value)
            {
                _SourceRectangle = value;
                this.NotifyPropertyChanged();
            }
        }
    }

    private float _rotation;
    [ComponentProperty(5, typeof(FloatPacker), typeof(FloatInterpolator), InterpolationType.Linear)]
    public float Rotation
    {
        get => _rotation;
        set
        {
            if (_rotation != value)
            {
                _rotation = value;
                this.NotifyPropertyChanged();
            }
        }
    }

    private Sprite _sprite;
    public Sprite GetSprite()
    {
        if (_sprite == null)
        {
            _sprite = new Sprite(ModManager.GetAsset<Texture2D>(_texture),
                                _renderScale,
                                _origin,
                                _colorTint,
                                _SourceRectangle,
                                _rotation);
        }

        return _sprite;
    }

    public override void ApplyInput(Entity parentEntity, UserCommand command, WorldContainer world, ECS ecs)
    {
        // Do nothing
    }

    public override Component Clone()
    {
        return new SpriteComponent()
        {
            Texture = this.Texture,
            RenderScale = this.RenderScale,
            Origin = this.Origin,
            ColorTint = this.ColorTint,
            SourceRectangle = this.SourceRectangle,
            Rotation = this.Rotation
        };
    }

    public override ulong GetHash()
    {
        return Utilities.CombineHash(this.Texture.Hash(), this.RenderScale.X.Hash(), this.RenderScale.Y.Hash(), this.Origin.X.Hash(), this.Origin.Y.Hash(), this.ColorTint.R.Hash(), this.ColorTint.G.Hash(), this.ColorTint.B.Hash(), this.ColorTint.A.Hash(), this.SourceRectangle.X.Hash(), this.SourceRectangle.Y.Hash(), this.SourceRectangle.Width.Hash(), this.SourceRectangle.Height.Hash(), this.Rotation.Hash());
    }

    public override string ToString()
    {
        return $"SpriteComponent: {this.Texture}";
    }
}