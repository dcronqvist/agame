using System.Numerics;
using AGame.Engine.Configuration;
using AGame.Engine.Graphics;
using AGame.Engine.Networking;
using AGame.Engine.World;

namespace AGame.Engine.ECSys.Components;

[ComponentNetworking(UpdateTriggersNetworkUpdate = true, CreateTriggersNetworkUpdate = true)]
public class ColorComponent : Component
{
    private ColorF _color;
    public ColorF Color
    {
        get => _color;
        set
        {
            if (!_color.Equals(value))
            {
                _color = value;
                this.NotifyPropertyChanged();
            }
        }
    }

    public ColorComponent()
    {

    }

    public override Component Clone()
    {
        return new ColorComponent()
        {
            _color = Color
        };
    }

    public override int Populate(byte[] data, int offset)
    {
        int initialOffset = offset;
        float r = BitConverter.ToSingle(data, offset);
        offset += sizeof(float);
        float g = BitConverter.ToSingle(data, offset);
        offset += sizeof(float);
        float b = BitConverter.ToSingle(data, offset);
        offset += sizeof(float);
        float a = BitConverter.ToSingle(data, offset);
        offset += sizeof(float);
        this._color = new ColorF(r, g, b, a);
        return offset - initialOffset;
    }

    public override byte[] ToBytes()
    {
        List<byte> bytes = new List<byte>();
        bytes.AddRange(BitConverter.GetBytes(Color.R));
        bytes.AddRange(BitConverter.GetBytes(Color.G));
        bytes.AddRange(BitConverter.GetBytes(Color.B));
        bytes.AddRange(BitConverter.GetBytes(Color.A));
        return bytes.ToArray();
    }

    public override string ToString()
    {
        return $"Color=[r={Color.R}, g={Color.G}, b={Color.B}, a={Color.A}]";
    }

    public override void UpdateComponent(Component newComponent)
    {
        ColorComponent tc = newComponent as ColorComponent;

        this.Color = tc.Color;
    }

    public override void InterpolateProperties(Component from, Component to, float amt)
    {
        ColorComponent fc = from as ColorComponent;
        ColorComponent tc = to as ColorComponent;

        this.Color = ColorF.Lerp(fc.Color, tc.Color, amt);
    }

    public override int GetHashCode()
    {
        return _color.GetHashCode();
    }

    public override void ApplyInput(UserCommand command, WorldContainer world)
    {

    }
}