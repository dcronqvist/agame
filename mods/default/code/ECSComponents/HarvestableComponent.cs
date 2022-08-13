using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AGame.Engine;
using AGame.Engine.Assets.Scripting;
using AGame.Engine.ECSys;
using AGame.Engine.Networking;
using AGame.Engine.World;
using GameUDPProtocol;

namespace DefaultMod;

public class HarvestDefinitionPacker : ComponentPropertyPacker<HarvestDefinition>
{
    public override byte[] Pack(HarvestDefinition value)
    {
        return value.ToBytes();
    }

    public override int Unpack(byte[] data, int offset, out HarvestDefinition value)
    {
        value = new HarvestDefinition();
        return value.Populate(data, offset);
    }
}

public class HarvestDefinitionInterpolator : ComponentPropertyInterpolator<HarvestDefinition>
{
    public override HarvestDefinition Interpolate(HarvestDefinition a, HarvestDefinition b, float t)
    {
        return a;
    }
}

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

[ComponentNetworking(CreateTriggersNetworkUpdate = true, UpdateTriggersNetworkUpdate = true), ScriptType(Name = "harvestable_component")]
public class HarvestableComponent : Component
{
    private string[] _tags;
    [ComponentProperty(0, typeof(ArrayPacker<string, StringPacker>), typeof(ArrayInterpolator), InterpolationType.ToInstant)]
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
    [ComponentProperty(1, typeof(ArrayPacker<HarvestDefinition, HarvestDefinitionPacker>), typeof(ArrayInterpolator), InterpolationType.ToInstant)]
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

    private int _breaksAfter;
    [ComponentProperty(2, typeof(IntPacker), typeof(IntInterpolator), InterpolationType.ToInstant)]
    public int BreaksAfter
    {
        get => _breaksAfter;
        set
        {
            if (_breaksAfter != value)
            {
                _breaksAfter = value;
                this.NotifyPropertyChanged();
            }
        }
    }

    private string _harvestSound;
    [ComponentProperty(3, typeof(StringPacker), typeof(StringInterpolator), InterpolationType.ToInstant)]
    public string HarvestSound
    {
        get => _harvestSound;
        set
        {
            if (_harvestSound != value)
            {
                _harvestSound = value;
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
            Tags = this.Tags,
            BreaksAfter = this.BreaksAfter,
            HarvestSound = this.HarvestSound
        };
    }

    public override ulong GetHash()
    {
        return Utilities.CombineHash(this.Tags.Length.Hash(), this.Yields.Length.Hash(), this.BreaksAfter.Hash(), this.HarvestSound.Hash());
    }

    public override string ToString()
    {
        return $"{this.Yields.Length} yields";
    }

    public bool HasTag(string tag)
    {
        return this.Tags.Contains(tag);
    }
}