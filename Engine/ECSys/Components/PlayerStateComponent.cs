using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AGame.Engine.Items;
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

    private float _itemUsedTime;
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

        if (this.HoldingUseItem)
        {
            if (!command.HasBeenRun)
                this.ItemUsedTime += command.DeltaTime;
        }
        else
        {
            this.ItemUsedTime = 0;
        }

        if (command.IsInputDown(UserCommand.INTERACT_ENTITY) && ecs.IsRunner(SystemRunner.Server))
        {
            // Get entity at mouse tile position
            var entityAtMouse = ecs.GetAllEntities(e => e.HasComponent<InteractableComponent>() && e.TryGetComponent<TransformComponent>(out var t) && t.Position.Equals(new CoordinateVector(this.MouseTileX, this.MouseTileY))).FirstOrDefault();

            if (entityAtMouse is not null)
            {
                // Interacting with something
                var interactable = entityAtMouse.GetComponent<InteractableComponent>();

                var playerCollider = parentEntity.GetComponent<ColliderComponent>();
                var interactableCollider = entityAtMouse.GetComponent<ColliderComponent>();

                var interactBox = interactableCollider.Box.Inflate(interactable.InteractDistance * TileGrid.TILE_SIZE);

                if (playerCollider.Box.IntersectsWith(interactBox))
                {
                    interactable.GetOnInteract().OnInteract(parentEntity, entityAtMouse, command, ecs);
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
        return Utilities.Hash(this.ToBytes());
    }

    public override void InterpolateProperties(Component from, Component to, float amt)
    {
        var toC = (PlayerStateComponent)to;
        var fromC = (PlayerStateComponent)from;

        this.HoldingUseItem = toC.HoldingUseItem;
        this.HoldingItem = toC.HoldingItem;
        this.MouseTileX = (int)Math.Round(Utilities.Lerp(fromC.MouseTileX, toC.MouseTileX, amt));
        this.MouseTileY = (int)Math.Round(Utilities.Lerp(fromC.MouseTileY, toC.MouseTileY, amt));
        this.ItemUsedTime = Utilities.Lerp(fromC.ItemUsedTime, toC.ItemUsedTime, amt);
        this.ItemOnMouse = toC.ItemOnMouse;
        this.ItemOnMouseCount = toC.ItemOnMouseCount;
        this.MouseSlot = toC.MouseSlot;
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
        this.ItemUsedTime = BitConverter.ToSingle(data, offset);
        offset += sizeof(float);
        len = BitConverter.ToInt32(data, offset);
        offset += sizeof(int);
        this.ItemOnMouse = Encoding.UTF8.GetString(data, offset, len);
        offset += len;
        this.ItemOnMouseCount = BitConverter.ToInt32(data, offset);
        offset += sizeof(int);
        this.MouseSlot = new ContainerSlotInfo();
        offset += this.MouseSlot.Populate(data, offset);
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
        bytes.AddRange(BitConverter.GetBytes(this.ItemUsedTime));
        bytes.AddRange(BitConverter.GetBytes(this.ItemOnMouse.Length));
        bytes.AddRange(Encoding.UTF8.GetBytes(this.ItemOnMouse));
        bytes.AddRange(BitConverter.GetBytes(this.ItemOnMouseCount));
        bytes.AddRange(this.MouseSlot.ToBytes());
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
        this.ItemUsedTime = newC.ItemUsedTime;
        this.ItemOnMouse = newC.ItemOnMouse;
        this.ItemOnMouseCount = newC.ItemOnMouseCount;
        this.MouseSlot = newC.MouseSlot;
    }
}