using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Numerics;
using AGame.Engine;
using AGame.Engine.Assets.Scripting;
using AGame.Engine.ECSys;
using AGame.Engine.Graphics;
using AGame.Engine.Graphics.Rendering;
using AGame.Engine.Items;
using AGame.Engine.Networking;
using AGame.Engine.World;

namespace DefaultMod;

[ComponentNetworking(CreateTriggersNetworkUpdate = true, UpdateTriggersNetworkUpdate = true), ScriptClass(Name = "hotbar_component")]
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

        if (command.HotbarButtons != 0)
        {
            int pressedSlot = command.HotbarButtons - 1;
            this.SelectedSlot = pressedSlot;
        }

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
                if (slot.Item.CanBeUsed(parentEntity, command, slot.Item, ecs))
                {
                    if (!command.HasBeenRun)
                        state.ItemUsedTime += command.DeltaTime;

                    bool done = slot.Item.OnUse(parentEntity, command, slot.Item, ecs, command.DeltaTime, state.ItemUsedTime, out bool resetUseTime);
                    if (done)
                    {
                        // Perform done thing
                        if (slot.Item.ShouldItemBeConsumed())
                        {
                            slot.Item.OnConsumed(parentEntity, slot.Item, ecs);
                            container.GetContainer().RemoveItem(this.ContainerSlots[this.SelectedSlot]);
                        }
                    }

                    if (resetUseTime)
                    {
                        state.ItemUsedTime = 0;
                    }
                }
            }
            else
            {
                state.ItemUsedTime = 0;
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

    public override ulong GetHash()
    {
        return Utilities.Hash(this.ToBytes());
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