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

    [JsonIgnore]
    private Sprite _sprite;

    [JsonIgnore, GameUDPProtocol.PacketPropIgnore]
    public Sprite Sprite
    {
        get
        {
            if (_sprite == null || _sprite.Texture.Name != this.Texture)
            {
                _sprite = new Sprite(AssetManager.GetAsset<Texture2D>(this.Texture), Vector2.One, Vector2.Zero, ColorF.White, new RectangleF(0, 0, 16, 16), 0f, new RectangleF(0, 0, 16, 16));
            }

            return _sprite;
        }
    }

    [JsonConstructor]
    public SpriteComponent()
    {

    }

    public SpriteComponent(string texture)
    {
        this.Texture = texture;
    }

    public override Component Clone()
    {
        return new SpriteComponent(this.Texture);
    }

    public override string ToString()
    {
        return $"Texture={this.Texture}";
    }

    public override int Populate(byte[] data, int offset)
    {
        int length = BitConverter.ToInt32(data, offset);
        this.Texture = Encoding.UTF8.GetString(data, offset + 4, length);
        return length + 4;
    }

    public override byte[] ToBytes()
    {
        List<byte> bytes = new List<byte>();
        bytes.AddRange(BitConverter.GetBytes(this.Texture.Length));
        bytes.AddRange(Encoding.UTF8.GetBytes(this.Texture));
        return bytes.ToArray();
    }

    public override void UpdateComponent(Component newComponent)
    {
        this.Texture = ((SpriteComponent)newComponent).Texture;
    }

    public override void InterpolateProperties()
    {

    }
}