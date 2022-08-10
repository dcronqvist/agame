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

[ComponentNetworking(CreateTriggersNetworkUpdate = true, UpdateTriggersNetworkUpdate = true), ScriptClass(Name = "harvestable_component")]
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

    private int _breaksAfter;
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
        return Utilities.Hash(this.ToBytes());
    }

    public override void InterpolateProperties(Component from, Component to, float amt)
    {
        HarvestableComponent toComp = (HarvestableComponent)to;
        this.Yields = toComp.Yields;
        this.Tags = toComp.Tags;
        this.BreaksAfter = toComp.BreaksAfter;
        this.HarvestSound = toComp.HarvestSound;
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
        this.BreaksAfter = BitConverter.ToInt32(data, offset);
        offset += sizeof(int);

        int l2 = BitConverter.ToInt32(data, offset);
        offset += sizeof(int);
        this.HarvestSound = Encoding.UTF8.GetString(data, offset, l2);
        offset += l2;

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

        bytes.AddRange(BitConverter.GetBytes(this.BreaksAfter));
        bytes.AddRange(BitConverter.GetBytes(this.HarvestSound.Length));
        bytes.AddRange(Encoding.UTF8.GetBytes(this.HarvestSound));

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
        this.Tags = newComp.Tags;
        this.BreaksAfter = newComp.BreaksAfter;
        this.HarvestSound = newComp.HarvestSound;
    }

    public bool HasTag(string tag)
    {
        return this.Tags.Contains(tag);
    }
}