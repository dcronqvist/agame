using System.Drawing;
using System.Numerics;
using System.Text;
using AGame.Engine.Assets;
using AGame.Engine.Graphics;
using AGame.Engine.Networking;
using AGame.Engine.World;

namespace AGame.Engine.ECSys.Components;

[ComponentNetworking(UpdateTriggersNetworkUpdate = true, CreateTriggersNetworkUpdate = true)]
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

    private RectangleF _sourceRectangle;
    public RectangleF SourceRectangle
    {
        get => _sourceRectangle;
        set
        {
            if (_sourceRectangle != value)
            {
                _sourceRectangle = value;
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

    private RectangleF _collisionBox;
    public RectangleF CollisionBox
    {
        get => _collisionBox;
        set
        {
            if (_collisionBox != value)
            {
                _collisionBox = value;
                this.NotifyPropertyChanged();
            }
        }
    }

    private Sprite _sprite;

    public override void ApplyInput(UserCommand command, WorldContainer world)
    {
        // Do nothing, we don't care about input
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
            Rotation = this.Rotation,
            CollisionBox = this.CollisionBox
        };
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(this.Texture, this.RenderScale, this.Origin, this.ColorTint, this.SourceRectangle, this.Rotation, this.CollisionBox);
    }

    public override void InterpolateProperties(Component from, Component to, float amt)
    {
        SpriteComponent fromSprite = (SpriteComponent)from;
        SpriteComponent toSprite = (SpriteComponent)to;

        if (amt > 0.5f)
        {
            this.Texture = toSprite.Texture;
        }

        this.RenderScale = Vector2.Lerp(fromSprite.RenderScale, toSprite.RenderScale, amt);
        this.Origin = Vector2.Lerp(fromSprite.Origin, toSprite.Origin, amt);
        this.ColorTint = ColorF.Lerp(fromSprite.ColorTint, toSprite.ColorTint, amt);
        this.SourceRectangle = toSprite.SourceRectangle;
        this.Rotation = Utilities.Lerp(fromSprite.Rotation, toSprite.Rotation, amt);
        this.CollisionBox = toSprite.CollisionBox;
    }

    public override int Populate(byte[] data, int offset)
    {
        int startOffset = offset;
        int texNameLength = BitConverter.ToInt32(data, offset);
        offset += sizeof(int);
        this.Texture = Encoding.UTF8.GetString(data, offset, texNameLength);
        offset += texNameLength;
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
        this.CollisionBox = new RectangleF(BitConverter.ToSingle(data, offset), BitConverter.ToSingle(data, offset + sizeof(float)), BitConverter.ToSingle(data, offset + sizeof(float) * 2), BitConverter.ToSingle(data, offset + sizeof(float) * 3));
        offset += sizeof(float) * 4;
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
        bytes.AddRange(BitConverter.GetBytes(this.CollisionBox.X));
        bytes.AddRange(BitConverter.GetBytes(this.CollisionBox.Y));
        bytes.AddRange(BitConverter.GetBytes(this.CollisionBox.Width));
        bytes.AddRange(BitConverter.GetBytes(this.CollisionBox.Height));
        return bytes.ToArray();
    }

    public override string ToString()
    {
        return $"Sprite=[{this._texture}]";
    }

    public override void UpdateComponent(Component newComponent)
    {
        SpriteComponent newSprite = (SpriteComponent)newComponent;
        this.Texture = newSprite.Texture;
        this.RenderScale = newSprite.RenderScale;
        this.Origin = newSprite.Origin;
        this.ColorTint = newSprite.ColorTint;
        this.SourceRectangle = newSprite.SourceRectangle;
        this.Rotation = newSprite.Rotation;
        this.CollisionBox = newSprite.CollisionBox;
    }

    public Sprite GetSprite()
    {
        if (this._sprite == null)
        {
            Texture2D texture = ModManager.GetAsset<Texture2D>(this.Texture);
            this._sprite = new Sprite(texture, this._renderScale, this._origin, this._colorTint, this._sourceRectangle, this._rotation, this._collisionBox);
        }

        return this._sprite;
    }
}