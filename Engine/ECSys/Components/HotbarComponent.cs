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

    public override void ApplyInput(Entity parentEntity, UserCommand command, WorldContainer world, ECS ecs)
    {
        int scrollDir = command.IsInputDown(UserCommand.MOUSE_SCROLL_DOWN) ? 1 : 0;
        scrollDir = command.IsInputDown(UserCommand.MOUSE_SCROLL_UP) ? -1 : scrollDir;

        // Get inventoryComponent of parentEntity
        InventoryComponent inventoryComponent = parentEntity.GetComponent<InventoryComponent>();

        this.SelectedSlot = (this.SelectedSlot + scrollDir);

        if (this.SelectedSlot < 0)
        {
            this.SelectedSlot = inventoryComponent.Width - 1;
        }
        else if (this.SelectedSlot >= inventoryComponent.Width)
        {
            this.SelectedSlot = 0;
        }
    }

    public override Component Clone()
    {
        return new HotbarComponent()
        {
            SelectedSlot = this.SelectedSlot
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
    }

    public override int Populate(byte[] data, int offset)
    {
        this.SelectedSlot = BitConverter.ToInt32(data, offset);
        return sizeof(int);
    }

    public override byte[] ToBytes()
    {
        List<byte> bytes = new List<byte>();
        bytes.AddRange(BitConverter.GetBytes(this.SelectedSlot));
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
    }

    public void Render(Vector2 topLeftStart, Inventory inventory)
    {
        InventorySlot[] bottomRow = inventory.GetRow(2);

        int slotSize = 64;
        int slotSpacing = 10;

        for (int i = 0; i < bottomRow.Length; i++)
        {
            InventorySlot slot = bottomRow[i];
            ColorF color = this.SelectedSlot == i ? ColorF.LightGray : ColorF.DarkGray;
            Renderer.Primitive.RenderRectangle(new RectangleF(topLeftStart.X + i * (slotSize + slotSpacing), topLeftStart.Y, slotSize, slotSize), color * 0.8f);

            if (slot != null)
            {
                Renderer.Texture.Render(slot.GetItem().Texture, new Vector2(topLeftStart.X + i * (slotSize + slotSpacing), topLeftStart.Y), new Vector2(slotSize / slot.GetItem().Texture.Width, slotSize / slot.GetItem().Texture.Height), 0f, ColorF.White);
            }
        }
    }
}