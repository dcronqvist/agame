using System;
using System.Collections.Generic;
using System.Text;
using AGame.Engine;
using AGame.Engine.Assets.Scripting;
using AGame.Engine.ECSys;
using AGame.Engine.Items;
using AGame.Engine.Networking;
using AGame.Engine.World;

namespace DefaultMod;

[ComponentNetworking(CreateTriggersNetworkUpdate = true, UpdateTriggersNetworkUpdate = true), ScriptType(Name = "ground_item_component")]
public class GroundItemComponent : Component
{
    private ItemInstance _item;
    [ComponentProperty(0, typeof(ItemInstancePacker), typeof(ItemInstanceInterpolator), InterpolationType.ToInstant)]
    public ItemInstance Item
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

    private int _pickedUpBy;
    [ComponentProperty(1, typeof(IntPacker), typeof(IntInterpolator), InterpolationType.ToInstant)]
    public int PickedUpBy
    {
        get => _pickedUpBy;
        set
        {
            if (_pickedUpBy != value)
            {
                _pickedUpBy = value;
                this.NotifyPropertyChanged();
            }
        }
    }

    public GroundItemComponent()
    {
        _pickedUpBy = -1;
    }

    public override void ApplyInput(Entity parentEntity, UserCommand command, WorldContainer world, ECS ecs)
    {
        // Do nothing
    }

    public override Component Clone()
    {
        return new GroundItemComponent()
        {
            Item = this.Item,
            PickedUpBy = this.PickedUpBy
        };
    }

    public override ulong GetHash()
    {
        return this.Item.Definition.ItemID.Hash();
    }

    public override string ToString()
    {
        return $"GroundItemComponent: {this.Item}";
    }
}