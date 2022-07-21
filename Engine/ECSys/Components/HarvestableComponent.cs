using System.Text;
using AGame.Engine.Networking;
using AGame.Engine.World;
using GameUDPProtocol;

namespace AGame.Engine.ECSys.Components;

public class HarvestDefinition : IPacketable
{
    public HarvestDefinition()
    {

    }

    public HarvestDefinition(int minAmount, int maxAmount, string itemID)
    {
        MinAmount = minAmount;
        MaxAmount = maxAmount;
        Item = itemID;
    }

    public int MinAmount { get; set; }
    public int MaxAmount { get; set; }
    public string Item { get; set; }

    public int Populate(byte[] data, int offset)
    {
        int start = offset;
        this.MinAmount = BitConverter.ToInt32(data, offset);
        offset += sizeof(int);
        this.MaxAmount = BitConverter.ToInt32(data, offset);
        offset += sizeof(int);
        int len = BitConverter.ToInt32(data, offset);
        offset += sizeof(int);
        this.Item = Encoding.UTF8.GetString(data, offset, len);
        offset += len;
        return offset - start;
    }

    public byte[] ToBytes()
    {
        List<byte> bytes = new List<byte>();
        bytes.AddRange(BitConverter.GetBytes(MinAmount));
        bytes.AddRange(BitConverter.GetBytes(MaxAmount));
        bytes.AddRange(BitConverter.GetBytes(Item.Length));
        bytes.AddRange(Encoding.ASCII.GetBytes(Item));
        return bytes.ToArray();
    }
}

[ComponentNetworking(CreateTriggersNetworkUpdate = true, UpdateTriggersNetworkUpdate = true)]
public class HarvestableComponent : Component
{
    private string[] _tags;
    public string[] Tags
    {
        get => _tags;
        set
        {
            if (_tags != value)
            {
                _tags = value;
                this.NotifyPropertyChanged();
            }
        }
    }

    private HarvestDefinition[] _yields;
    public HarvestDefinition[] Yields
    {
        get => _yields;
        set
        {
            if (_yields != value)
            {
                _yields = value;
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
        return new HarvestableComponent()
        {
            Yields = this.Yields,
            Tags = this.Tags
        };
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(this.Yields, this.Tags);
    }

    public override void InterpolateProperties(Component from, Component to, float amt)
    {
        HarvestableComponent toComp = (HarvestableComponent)to;
        this.Yields = toComp.Yields;
        this.Tags = toComp.Tags;
    }

    public override int Populate(byte[] data, int offset)
    {
        int start = offset;
        int tagLen = BitConverter.ToInt32(data, offset);
        offset += sizeof(int);

        this.Tags = new string[tagLen];
        for (int i = 0; i < tagLen; i++)
        {
            int l = BitConverter.ToInt32(data, offset);
            offset += sizeof(int);
            this.Tags[i] = Encoding.UTF8.GetString(data, offset, l);
            offset += l;
        }

        int len = BitConverter.ToInt32(data, offset);
        offset += sizeof(int);
        this.Yields = new HarvestDefinition[len];
        for (int i = 0; i < len; i++)
        {
            HarvestDefinition def = new HarvestDefinition();
            offset += def.Populate(data, offset);
            this.Yields[i] = def;
        }
        return offset - start;
    }

    public override byte[] ToBytes()
    {
        List<byte> bytes = new List<byte>();
        bytes.AddRange(BitConverter.GetBytes(this.Tags.Length));
        foreach (string tag in this.Tags)
        {
            bytes.AddRange(BitConverter.GetBytes(tag.Length));
            bytes.AddRange(Encoding.UTF8.GetBytes(tag));
        }

        bytes.AddRange(BitConverter.GetBytes(this.Yields.Length));
        foreach (HarvestDefinition yield in this.Yields)
        {
            bytes.AddRange(yield.ToBytes());
        }
        return bytes.ToArray();
    }

    public override string ToString()
    {
        return $"{this.Yields.Length} yields";
    }

    public override void UpdateComponent(Component newComponent)
    {
        HarvestableComponent newComp = (HarvestableComponent)newComponent;
        this.Yields = newComp.Yields;
    }

    public bool HasTag(string tag)
    {
        return this.Tags.Contains(tag);
    }
}