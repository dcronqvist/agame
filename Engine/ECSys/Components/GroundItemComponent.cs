using System.Text;
using AGame.Engine.Networking;
using AGame.Engine.World;

namespace AGame.Engine.ECSys.Components;

[ComponentNetworking(CreateTriggersNetworkUpdate = true, UpdateTriggersNetworkUpdate = true)]
public class GroundItemComponent : Component
{
    private string _item;
    public string Item
    {
        get => _item;
        set
        {
            if (_item != value)
            {
                _item = value;
                this.NotifyPropertyChanged();
            }
        }
    }

    public override void ApplyInput(Entity parentEntity, UserCommand command, WorldContainer world, ECS ecs)
    {
        // Do nothing
    }

    public override Component Clone()
    {
        return new GroundItemComponent()
        {
            Item = this.Item
        };
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(this.Item);
    }

    public override void InterpolateProperties(Component from, Component to, float amt)
    {
        GroundItemComponent toComp = (GroundItemComponent)to;
        this.Item = toComp.Item;
    }

    public override int Populate(byte[] data, int offset)
    {
        int start = offset;
        int len = BitConverter.ToInt32(data, offset);
        offset += sizeof(int);
        this.Item = Encoding.UTF8.GetString(data, offset, len);
        offset += len;
        return offset - start;
    }

    public override byte[] ToBytes()
    {
        List<byte> bytes = new List<byte>();
        bytes.AddRange(BitConverter.GetBytes(this.Item.Length));
        bytes.AddRange(Encoding.UTF8.GetBytes(this.Item));
        return bytes.ToArray();
    }

    public override string ToString()
    {
        return $"GroundItemComponent: {this.Item}";
    }

    public override void UpdateComponent(Component newComponent)
    {
        GroundItemComponent newComp = (GroundItemComponent)newComponent;
        this.Item = newComp.Item;
    }
}