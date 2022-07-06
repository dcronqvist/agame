using System.Drawing;
using System.Numerics;
using System.Text;
using System.Text.Json.Serialization;
using AGame.Engine.Assets;
using AGame.Engine.Graphics;
using AGame.Engine.Networking;

namespace AGame.Engine.ECSys.Components;

[ComponentNetworking(CNType.Update, NDirection.ServerToClient)]
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

    private RectangleF _collisionRect;
    public RectangleF CollisionRect
    {
        get => _collisionRect;
        set
        {
            if (_collisionRect != value)
            {
                _collisionRect = value;
                this.NotifyPropertyChanged();
            }
        }
    }

    [JsonIgnore]
    private Sprite _sprite;

    [JsonIgnore, GameUDPProtocol.PacketPropIgnore]
    public Sprite Sprite
    {
        get
        {
            if (_sprite == null || _sprite.Texture.Name != this.Texture)
            {
                _sprite = new Sprite(ModManager.GetAsset<Texture2D>(this.Texture),
                                     this.RenderScale,
                                     this.Origin,
                                     this.ColorTint,
                                     this.SourceRectangle,
                                     this.Rotation,
                                     this.CollisionRect);
            }

            return _sprite;
        }
    }

    [JsonConstructor]
    public SpriteComponent()
    {

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
            CollisionRect = this.CollisionRect
        };
    }

    public override string ToString()
    {
        return $"SpriteComponent[Texture={this.Texture}, RenderScale={this.RenderScale}, Origin={this.Origin}, ColorTint={this.ColorTint}, SourceRectangle={this.SourceRectangle}, Rotation={this.Rotation}, CollisionRect={this.CollisionRect}]";
    }

    public override int Populate(byte[] data, int offset)
    {
        int startOffset = offset;
        int length = BitConverter.ToInt32(data, offset);
        offset += sizeof(int);
        this.Texture = Encoding.UTF8.GetString(data, offset, length);
        offset += length;

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

        this.CollisionRect = new RectangleF(BitConverter.ToSingle(data, offset), BitConverter.ToSingle(data, offset + sizeof(float)), BitConverter.ToSingle(data, offset + sizeof(float) * 2), BitConverter.ToSingle(data, offset + sizeof(float) * 3));
        offset += sizeof(float) * 4;

        this._sprite = null;
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

        bytes.AddRange(BitConverter.GetBytes(this.CollisionRect.X));
        bytes.AddRange(BitConverter.GetBytes(this.CollisionRect.Y));
        bytes.AddRange(BitConverter.GetBytes(this.CollisionRect.Width));
        bytes.AddRange(BitConverter.GetBytes(this.CollisionRect.Height));
        return bytes.ToArray();
    }

    public override void UpdateComponent(Component newComponent)
    {
        SpriteComponent newSpriteComponent = (SpriteComponent)newComponent;
        this.Texture = newSpriteComponent.Texture;
        this.RenderScale = newSpriteComponent.RenderScale;
        this.Origin = newSpriteComponent.Origin;
        this.ColorTint = newSpriteComponent.ColorTint;
        this.SourceRectangle = newSpriteComponent.SourceRectangle;
        this.Rotation = newSpriteComponent.Rotation;
        this.CollisionRect = newSpriteComponent.CollisionRect;
        this._sprite = null;
    }

    public override void InterpolateProperties(Component from, Component to, float amt)
    {
        // No interpolation really needed
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(
            this.Texture,
            this.RenderScale,
            this.Origin,
            this.ColorTint,
            this.SourceRectangle,
            this.Rotation,
            this.CollisionRect
        );
    }

    public override void ApplyInput(UserCommand command)
    {

    }
}