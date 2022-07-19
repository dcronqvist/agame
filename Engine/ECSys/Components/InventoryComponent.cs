using AGame.Engine.Items;
using AGame.Engine.Networking;
using AGame.Engine.World;

namespace AGame.Engine.ECSys.Components;

[ComponentNetworking(CreateTriggersNetworkUpdate = true, UpdateTriggersNetworkUpdate = true)]
public class InventoryComponent : Component
{
    private int _width;
    public int Width
    {
        get => _width;
        set
        {
            if (_width != value)
            {
                _width = value;
                this.NotifyPropertyChanged();
            }
        }
    }

    private int _height;
    public int Height
    {
        get => _height;
        set
        {
            if (_height != value)
            {
                _height = value;
                this.NotifyPropertyChanged();
            }
        }
    }

    private Inventory _inventory;
    public Inventory GetInventory()
    {
        if (_inventory == null)
        {
            _inventory = new Inventory(Width, Height);
        }

        return _inventory;
    }

    public override void ApplyInput(Entity parentEntity, UserCommand command, WorldContainer world, ECS ecs)
    {

    }

    public override Component Clone()
    {
        return new InventoryComponent()
        {
            Width = this.Width,
            Height = this.Height
        };
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(this.Width, this.Height);
    }

    public override void InterpolateProperties(Component from, Component to, float amt)
    {
        var toInv = (InventoryComponent)to;
        this.Width = toInv.Width;
        this.Height = toInv.Height;
        this._inventory = toInv._inventory;
    }

    public override int Populate(byte[] data, int offset)
    {
        int start = offset;
        this.Width = BitConverter.ToInt32(data, offset);
        offset += sizeof(int);
        this.Height = BitConverter.ToInt32(data, offset);
        offset += sizeof(int);
        return offset - start;
    }

    public override byte[] ToBytes()
    {
        List<byte> bytes = new List<byte>();
        bytes.AddRange(BitConverter.GetBytes(this.Width));
        bytes.AddRange(BitConverter.GetBytes(this.Height));
        return bytes.ToArray();
    }

    public override string ToString()
    {
        return $"InventoryComponent: {this.Width}x{this.Height}";
    }

    public override void UpdateComponent(Component newComponent)
    {
        var newInv = (InventoryComponent)newComponent;
        this.Width = newInv.Width;
        this.Height = newInv.Height;
    }
}