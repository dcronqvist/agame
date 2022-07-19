using System.Text;
using AGame.Engine.Networking;
using AGame.Engine.World;

namespace AGame.Engine.ECSys.Components;

[ComponentNetworking(CreateTriggersNetworkUpdate = true, UpdateTriggersNetworkUpdate = true)]
public class PlayerStateComponent : Component
{
    private bool _holdingUseItem;
    public bool HoldingUseItem
    {
        get => _holdingUseItem;
        set
        {
            if (_holdingUseItem != value)
            {
                _holdingUseItem = value;
                this.NotifyPropertyChanged();
            }
        }
    }

    private string _holdingItem;
    public string HoldingItem
    {
        get => _holdingItem ?? "";
        set
        {
            if (_holdingItem != value)
            {
                _holdingItem = value;
                this.NotifyPropertyChanged();
            }
        }
    }

    private int _mouseTileX;
    public int MouseTileX
    {
        get => _mouseTileX;
        set
        {
            if (_mouseTileX != value)
            {
                _mouseTileX = value;
                this.NotifyPropertyChanged();
            }
        }
    }

    private int _mouseTileY;
    public int MouseTileY
    {
        get => _mouseTileY;
        set
        {
            if (_mouseTileY != value)
            {
                _mouseTileY = value;
                this.NotifyPropertyChanged();
            }
        }
    }

    public override void ApplyInput(Entity parentEntity, UserCommand command, WorldContainer world, ECS ecs)
    {
        this.HoldingUseItem = command.IsInputDown(UserCommand.USE_ITEM);
        this.MouseTileX = command.MouseTileX;
        this.MouseTileY = command.MouseTileY;
    }

    public override Component Clone()
    {
        return new PlayerStateComponent()
        {
            HoldingUseItem = this.HoldingUseItem,
            HoldingItem = this.HoldingItem,
            MouseTileX = this.MouseTileX,
            MouseTileY = this.MouseTileY
        };
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(this.HoldingUseItem, this.HoldingItem, this.MouseTileX, this.MouseTileY);
    }

    public override void InterpolateProperties(Component from, Component to, float amt)
    {
        var toC = (PlayerStateComponent)to;
        var fromC = (PlayerStateComponent)from;

        this.HoldingUseItem = toC.HoldingUseItem;
        this.HoldingItem = toC.HoldingItem;
        this.MouseTileX = toC.MouseTileX;
        this.MouseTileY = toC.MouseTileY;
    }

    public override int Populate(byte[] data, int offset)
    {
        int start = offset;
        this.HoldingUseItem = BitConverter.ToBoolean(data, offset);
        offset += sizeof(bool);
        int len = BitConverter.ToInt32(data, offset);
        offset += sizeof(int);
        this.HoldingItem = Encoding.UTF8.GetString(data, offset, len);
        offset += len;
        this.MouseTileX = BitConverter.ToInt32(data, offset);
        offset += sizeof(int);
        this.MouseTileY = BitConverter.ToInt32(data, offset);
        offset += sizeof(int);
        return offset - start;
    }

    public override byte[] ToBytes()
    {
        List<byte> bytes = new List<byte>();
        bytes.AddRange(BitConverter.GetBytes(this.HoldingUseItem));
        bytes.AddRange(BitConverter.GetBytes(this.HoldingItem.Length));
        bytes.AddRange(Encoding.UTF8.GetBytes(this.HoldingItem));
        bytes.AddRange(BitConverter.GetBytes(this.MouseTileX));
        bytes.AddRange(BitConverter.GetBytes(this.MouseTileY));
        return bytes.ToArray();
    }

    public override string ToString()
    {
        return $"PlayerStateComponent: HoldingUse={this.HoldingUseItem}";
    }

    public override void UpdateComponent(Component newComponent)
    {
        var newC = (PlayerStateComponent)newComponent;
        this.HoldingUseItem = newC.HoldingUseItem;
        this.HoldingItem = newC.HoldingItem;
        this.MouseTileX = newC.MouseTileX;
        this.MouseTileY = newC.MouseTileY;
    }
}