using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AGame.Engine;
using AGame.Engine.Assets.Scripting;
using AGame.Engine.ECSys;
using AGame.Engine.Items;
using AGame.Engine.Networking;
using AGame.Engine.World;

namespace DefaultMod;

public class ContainerSlotInfoPacker : ComponentPropertyPacker<ContainerSlotInfo>
{
    public override byte[] Pack(ContainerSlotInfo value)
    {
        return value.ToBytes();
    }

    public override int Unpack(byte[] data, int offset, out ContainerSlotInfo value)
    {
        value = new ContainerSlotInfo();
        return value.Populate(data, offset);
    }
}

public class ContainerSlotInfoInterpolator : ComponentPropertyInterpolator<ContainerSlotInfo>
{
    public override ContainerSlotInfo Interpolate(ContainerSlotInfo a, ContainerSlotInfo b, float t)
    {
        return a;
    }
}

[ComponentNetworking(CreateTriggersNetworkUpdate = true, UpdateTriggersNetworkUpdate = true), ScriptType(Name = "player_state_component")]
public class PlayerStateComponent : Component
{
    private bool _holdingUseItem;
    [ComponentProperty(0, typeof(BoolPacker), typeof(BoolInterpolator), InterpolationType.ToInstant)]
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
    [ComponentProperty(1, typeof(StringPacker), typeof(StringInterpolator), InterpolationType.ToInstant)]
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
    [ComponentProperty(2, typeof(IntPacker), typeof(IntInterpolator), InterpolationType.ToInstant)]
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
    [ComponentProperty(3, typeof(IntPacker), typeof(IntInterpolator), InterpolationType.ToInstant)]
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

    private float _itemUsedTime;
    [ComponentProperty(4, typeof(FloatPacker), typeof(FloatInterpolator), InterpolationType.Linear)]
    public float ItemUsedTime
    {
        get => _itemUsedTime;
        set
        {
            if (_itemUsedTime != value)
            {
                _itemUsedTime = value;
                this.NotifyPropertyChanged();
            }
        }
    }

    private string _itemOnMouse;
    [ComponentProperty(5, typeof(StringPacker), typeof(StringInterpolator), InterpolationType.ToInstant)]
    public string ItemOnMouse
    {
        get => _itemOnMouse ?? "";
        set
        {
            if (_itemOnMouse != value)
            {
                _itemOnMouse = value;
                this.NotifyPropertyChanged();
            }
        }
    }

    private int _itemOnMouseCount;
    [ComponentProperty(6, typeof(IntPacker), typeof(IntInterpolator), InterpolationType.ToInstant)]
    public int ItemOnMouseCount
    {
        get => _itemOnMouseCount;
        set
        {
            if (_itemOnMouseCount != value)
            {
                _itemOnMouseCount = value;
                this.NotifyPropertyChanged();
            }
        }
    }

    private ContainerSlotInfo _mouseSlot;
    [ComponentProperty(7, typeof(ContainerSlotInfoPacker), typeof(ContainerSlotInfoInterpolator), InterpolationType.ToInstant)]
    public ContainerSlotInfo MouseSlot
    {
        get => _mouseSlot ?? (_mouseSlot = new ContainerSlotInfo(0, null, 0));
        set
        {
            if (_mouseSlot != value)
            {
                _mouseSlot = value;
                this.NotifyPropertyChanged();
            }
        }
    }

    public override void ApplyInput(Entity parentEntity, UserCommand command, WorldContainer world, ECS ecs)
    {
        this.HoldingUseItem = command.IsInputDown(UserCommand.USE_ITEM);
        this.MouseTileX = command.MouseTileX;
        this.MouseTileY = command.MouseTileY;

        if (command.IsInputDown(UserCommand.INTERACT_ENTITY) && ecs.IsRunner(SystemRunner.Server))
        {
            // Get entity at mouse tile position
            var playerCollider = parentEntity.GetComponent<ColliderComponent>();
            var mouseX = command.MouseTileX * TileGrid.TILE_SIZE;
            var mouseY = command.MouseTileY * TileGrid.TILE_SIZE;
            var interactWith = ecs.GetAllEntities(e => e.TryGetComponent<InteractableComponent>(out var i) && e.TryGetComponent<ColliderComponent>(out var c) && c.Box.Contains(mouseX + 16, mouseY + 16)).FirstOrDefault();

            if (interactWith is not null)
            {
                // Interacting with something
                var interactable = interactWith.GetComponent<InteractableComponent>();

                var interactableCollider = interactWith.GetComponent<ColliderComponent>();

                var interactBox = interactableCollider.Box.Inflate(interactable.InteractDistance * TileGrid.TILE_SIZE);

                if (playerCollider.Box.IntersectsWith(interactBox))
                {
                    interactable.GetOnInteract().OnInteract(parentEntity, interactWith, command, ecs);
                }
            }
        }
    }

    public override Component Clone()
    {
        return new PlayerStateComponent()
        {
            HoldingUseItem = this.HoldingUseItem,
            HoldingItem = this.HoldingItem,
            MouseTileX = this.MouseTileX,
            MouseTileY = this.MouseTileY,
            ItemUsedTime = this.ItemUsedTime,
            ItemOnMouse = this.ItemOnMouse,
            ItemOnMouseCount = this.ItemOnMouseCount,
            MouseSlot = this.MouseSlot
        };
    }

    public override ulong GetHash()
    {
        return Utilities.CombineHash(this.HoldingUseItem.Hash(), this.HoldingItem.Hash(), this.MouseTileX.Hash(), this.MouseTileY.Hash(), this.ItemUsedTime.Hash(), this.ItemOnMouse.Hash(), this.ItemOnMouseCount.Hash());
    }

    public override string ToString()
    {
        return $"PlayerStateComponent: HoldingUse={this.HoldingUseItem}";
    }
}