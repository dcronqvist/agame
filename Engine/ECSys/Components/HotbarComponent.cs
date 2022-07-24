using System;
using System.Collections.Generic;
using System.Drawing;
using System.Numerics;
using AGame.Engine.Graphics;
using AGame.Engine.Graphics.Rendering;
using AGame.Engine.Items;
using AGame.Engine.Networking;
using AGame.Engine.World;

namespace AGame.Engine.ECSys.Components;

[ComponentNetworking(CreateTriggersNetworkUpdate = true, UpdateTriggersNetworkUpdate = true)]
public class HotbarComponent : Component
{
    private int _selectedSlot;
    public int SelectedSlot
    {
        get => _selectedSlot;
        set
        {
            if (_selectedSlot != value)
            {
                _selectedSlot = value;
                this.NotifyPropertyChanged();
            }
        }
    }

    private int[] _containerSlots;
    public int[] ContainerSlots
    {
        get => _containerSlots;
        set
        {
            if (_containerSlots != value)
            {
                _containerSlots = value;
                this.NotifyPropertyChanged();
            }
        }
    }

    public override void ApplyInput(Entity parentEntity, UserCommand command, WorldContainer world, ECS ecs)
    {
        int scrollDir = command.IsInputDown(UserCommand.MOUSE_SCROLL_DOWN) ? 1 : 0;
        scrollDir = command.IsInputDown(UserCommand.MOUSE_SCROLL_UP) ? -1 : scrollDir;

        // Get inventoryComponent of parentEntity
        this.SelectedSlot = (this.SelectedSlot + scrollDir);

        if (this.SelectedSlot < 0)
        {
            this.SelectedSlot = this.ContainerSlots.Length - 1;
        }
        else if (this.SelectedSlot >= this.ContainerSlots.Length)
        {
            this.SelectedSlot = 0;
        }

        var container = parentEntity.GetComponent<ContainerComponent>();
        var state = parentEntity.GetComponent<PlayerStateComponent>();

        var slot = container.GetContainer().GetSlot(this.ContainerSlots[this.SelectedSlot]);

        if (slot.Item != null)
        {
            if (command.IsInputDown(UserCommand.USE_ITEM))
            {
                // bool working = slot.GetItem().OnHoldLeftClick(command, parentEntity, new Vector2i(state.MouseTileX, state.MouseTileY), ecs, command.DeltaTime, state.ItemUsedTime);
                // if (!working)
                // {
                //     state.ItemUsedTime = 0;
                // }

                // if (!command.HasBeenRun)
                // {
                //     if (slot.GetItem().ShouldBeConsumed)
                //     {
                //         slot.GetItem().ShouldBeConsumed = false;
                //         container.GetContainer().RemoveItem(this.ContainerSlots[this.SelectedSlot], 1);
                //     }
                // }
            }
        }
    }

    public override Component Clone()
    {
        return new HotbarComponent()
        {
            SelectedSlot = this.SelectedSlot,
            ContainerSlots = this.ContainerSlots
        };
    }

    public override int GetHashCode()
    {
        return this.SelectedSlot.GetHashCode();
    }

    public override void InterpolateProperties(Component from, Component to, float amt)
    {
        HotbarComponent toHotbar = (HotbarComponent)to;
        this.SelectedSlot = toHotbar.SelectedSlot;
        this.ContainerSlots = toHotbar.ContainerSlots;
    }

    public override int Populate(byte[] data, int offset)
    {
        int start = offset;
        this.SelectedSlot = BitConverter.ToInt32(data, offset);
        offset += sizeof(int);
        int len = BitConverter.ToInt32(data, offset);
        offset += sizeof(int);
        this.ContainerSlots = new int[len];
        for (int i = 0; i < len; i++)
        {
            this.ContainerSlots[i] = BitConverter.ToInt32(data, offset);
            offset += sizeof(int);
        }
        return offset - start;
    }

    public override byte[] ToBytes()
    {
        List<byte> bytes = new List<byte>();
        bytes.AddRange(BitConverter.GetBytes(this.SelectedSlot));
        bytes.AddRange(BitConverter.GetBytes(this.ContainerSlots.Length));
        foreach (var slot in this.ContainerSlots)
        {
            bytes.AddRange(BitConverter.GetBytes(slot));
        }
        return bytes.ToArray();
    }

    public override string ToString()
    {
        return "HotbarComponent: SelectedSlot=" + this.SelectedSlot;
    }

    public override void UpdateComponent(Component newComponent)
    {
        HotbarComponent newHotbar = (HotbarComponent)newComponent;
        this.SelectedSlot = newHotbar.SelectedSlot;
        this.ContainerSlots = newHotbar.ContainerSlots;
    }
}