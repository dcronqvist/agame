using System;
using System.Collections.Generic;
using System.Drawing;
using System.Numerics;
using System.Text;
using AGame.Engine.Assets;
using AGame.Engine.Graphics;
using AGame.Engine.Networking;
using AGame.Engine.World;

namespace AGame.Engine.ECSys.Components;

[ComponentNetworking(CreateTriggersNetworkUpdate = true, UpdateTriggersNetworkUpdate = true)]
public class SpriteComponent : Component
{
    private string _texture;
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
        return Utilities.Hash(this.ToBytes());
    }

    public override void InterpolateProperties(Component from, Component to, float amt)
    {
        var fromC = (SpriteComponent)from;
        var toC = (SpriteComponent)to;

        this.Texture = toC.Texture;
        this.RenderScale = Vector2.Lerp(fromC.RenderScale, toC.RenderScale, amt);
        this.Origin = Vector2.Lerp(fromC.Origin, toC.Origin, amt);
        this.ColorTint = ColorF.Lerp(fromC.ColorTint, toC.ColorTint, amt);
        this.SourceRectangle = fromC.SourceRectangle.Lerp(toC.SourceRectangle, amt);
        this.Rotation = Utilities.Lerp(fromC.Rotation, toC.Rotation, amt);
        this._sprite = null;
    }

    public override int Populate(byte[] data, int offset)
    {
        int startOffset = offset;
        int len = BitConverter.ToInt32(data, offset);
        offset += sizeof(int);
        this.Texture = Encoding.UTF8.GetString(data, offset, len);
        offset += len;
        this.RenderScale = new Vector2(BitConverter.ToSingle(data, offset), BitConverter.ToSingle(data, offset + sizeof(float)));
        offset += sizeof(float) * 2;
        this.Origin = new Vector2(BitConverter.ToSingle(data, offset), BitConverter.ToSingle(data, offset + sizeof(float)));
        offset += sizeof(float) * 2;
        this.ColorTint = new ColorF(BitConverter.ToSingle(data, offset), BitConverter.ToSingle(data, offset + sizeof(float)), BitConverter.ToSingle(data, offset + sizeof(float) * 2), BitConverter.ToSingle(data, offset + sizeof(float) * 3));
        offset += sizeof(float) * 4;
        this.SourceRectangle = new RectangleF(BitConverter.ToSingle(data, offset), BitConverter.ToSingle(data, offset + sizeof(float)), BitConverter.ToSingle(data, offset + sizeof(float) * 2), BitConverter.ToSingle(data, offset + sizeof(float) * 3));
        offset += sizeof(float) * 4;
        this.Rotation = BitConverter.ToSingle(data, offset);
        offset += sizeof(float);
        return offset - startOffset;
    }

    public override byte[] ToBytes()
    {
        List<byte> bytes = new List<byte>();
        bytes.AddRange(BitConverter.GetBytes(this.Texture.Length));
        bytes.AddRange(Encoding.UTF8.GetBytes(this.Texture));
        bytes.AddRange(BitConverter.GetBytes(this.RenderScale.X));
        bytes.AddRange(BitConverter.GetBytes(this.RenderScale.Y));
        bytes.AddRange(BitConverter.GetBytes(this.Origin.X));
        bytes.AddRange(BitConverter.GetBytes(this.Origin.Y));
        bytes.AddRange(BitConverter.GetBytes(this.ColorTint.R));
        bytes.AddRange(BitConverter.GetBytes(this.ColorTint.G));
        bytes.AddRange(BitConverter.GetBytes(this.ColorTint.B));
        bytes.AddRange(BitConverter.GetBytes(this.ColorTint.A));
        bytes.AddRange(BitConverter.GetBytes(this.SourceRectangle.X));
        bytes.AddRange(BitConverter.GetBytes(this.SourceRectangle.Y));
        bytes.AddRange(BitConverter.GetBytes(this.SourceRectangle.Width));
        bytes.AddRange(BitConverter.GetBytes(this.SourceRectangle.Height));
        bytes.AddRange(BitConverter.GetBytes(this.Rotation));
        return bytes.ToArray();
    }

    public override string ToString()
    {
        return $"SpriteComponent: {this.Texture}";
    }

    public override void UpdateComponent(Component newComponent)
    {
        var newC = (SpriteComponent)newComponent;
        this.Texture = newC.Texture;
        this.RenderScale = newC.RenderScale;
        this.Origin = newC.Origin;
        this.ColorTint = newC.ColorTint;
        this.SourceRectangle = newC.SourceRectangle;
        this.Rotation = newC.Rotation;
        this._sprite = null;
    }
}