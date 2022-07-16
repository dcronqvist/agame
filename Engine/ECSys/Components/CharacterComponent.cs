using System.Numerics;
using System.Text;
using AGame.Engine.Networking;
using AGame.Engine.World;

namespace AGame.Engine.ECSys.Components;

[ComponentNetworking(CreateTriggersNetworkUpdate = true, UpdateTriggersNetworkUpdate = true)]
public class CharacterComponent : Component
{
    private string _name;
    public string Name
    {
        get => _name;
        set
        {
            if (_name != value)
            {
                _name = value;
                this.NotifyPropertyChanged();
            }
        }
    }

    private Vector2 _nameRenderOffset;
    public Vector2 NameRenderOffset
    {
        get => _nameRenderOffset;
        set
        {
            if (_nameRenderOffset != value)
            {
                _nameRenderOffset = value;
                this.NotifyPropertyChanged();
            }
        }
    }

    public override void ApplyInput(Entity parentEntity, UserCommand command, WorldContainer world, ECS ecs)
    {

    }

    public override Component Clone()
    {
        return new CharacterComponent()
        {
            Name = this.Name,
            NameRenderOffset = this.NameRenderOffset
        };
    }

    public override int GetHashCode()
    {
        return this.Name.GetHashCode();
    }

    public override void InterpolateProperties(Component from, Component to, float amt)
    {
        var fromC = (CharacterComponent)from;
        var toC = (CharacterComponent)to;

        this.Name = toC.Name;
        this.NameRenderOffset = Vector2.Lerp(fromC.NameRenderOffset, toC.NameRenderOffset, amt);
    }

    public override int Populate(byte[] data, int offset)
    {
        int sOffset = offset;
        var nameLength = BitConverter.ToInt32(data, offset);
        offset += sizeof(int);
        var name = Encoding.UTF8.GetString(data, offset, nameLength);
        offset += nameLength;
        this.Name = name;
        var x = BitConverter.ToSingle(data, offset);
        offset += sizeof(float);
        var y = BitConverter.ToSingle(data, offset);
        offset += sizeof(float);
        this.NameRenderOffset = new Vector2(x, y);
        return offset - sOffset;
    }

    public override byte[] ToBytes()
    {
        List<byte> bytes = new List<byte>();
        bytes.AddRange(BitConverter.GetBytes(this.Name.Length));
        bytes.AddRange(Encoding.UTF8.GetBytes(this.Name));
        bytes.AddRange(BitConverter.GetBytes(this.NameRenderOffset.X));
        bytes.AddRange(BitConverter.GetBytes(this.NameRenderOffset.Y));
        return bytes.ToArray();
    }

    public override string ToString()
    {
        return $"CharacterComponent: {this.Name}";
    }

    public override void UpdateComponent(Component newComponent)
    {
        var newC = (CharacterComponent)newComponent;
        this.Name = newC.Name;
        this.NameRenderOffset = newC.NameRenderOffset;
    }
}