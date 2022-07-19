using System.Drawing;
using System.Numerics;
using AGame.Engine.Assets;
using AGame.Engine.Graphics;
using AGame.Engine.Graphics.Rendering;

namespace AGame.Engine.Items;

public class InventorySlot
{
    public string Item { get; set; }
    public int Count { get; set; }

    public InventorySlot(string item, int count)
    {
        Item = item;
        Count = count;
    }

    public Item GetItem()
    {
        return ItemManager.GetItem(Item);
    }
}

public class Inventory
{
    private InventorySlot[,] Items { get; set; }

    public event EventHandler<InventorySlot> InventoryChanged;

    public Inventory(int width, int height)
    {
        Items = new InventorySlot[width, height];
    }

    private void AddItem(string itemID)
    {
        Item item = ItemManager.GetItem(itemID);

        for (int x = 0; x < Items.GetLength(0); x++)
        {
            for (int y = 0; y < Items.GetLength(1); y++)
            {
                if (Items[x, y] != null && Items[x, y].Item == itemID && Items[x, y].Count < item.MaxStack)
                {
                    Items[x, y].Count += 1;
                    this.InventoryChanged?.Invoke(this, Items[x, y]);
                    return;
                }
            }
        }

        for (int x = 0; x < Items.GetLength(0); x++)
        {
            for (int y = 0; y < Items.GetLength(1); y++)
            {
                if (Items[x, y] == null)
                {
                    Items[x, y] = new InventorySlot(itemID, 1);
                    this.InventoryChanged?.Invoke(this, Items[x, y]);
                    return;
                }
            }
        }
    }

    public void AddItem(string item, int amount)
    {
        for (int i = 0; i < amount; i++)
        {
            this.AddItem(item);
        }
    }

    public InventorySlot GetSlot(int x, int y)
    {
        return Items[x, y];
    }

    public InventorySlot[,] GetSlots()
    {
        return Items;
    }

    public InventorySlot TakeItemsFromSlot(int x, int y)
    {
        return TakeItemsFromSlot(x, y, this.GetSlot(x, y).Count);
    }

    public InventorySlot TakeItemsFromSlot(int x, int y, int amount)
    {
        InventorySlot curr = this.GetSlot(x, y);
        InventorySlot slot = new InventorySlot(curr.Item, amount);
        curr.Count -= amount;
        if (curr.Count <= 0)
        {
            Items[x, y] = null;
        }
        this.InventoryChanged?.Invoke(this, curr);
        return slot;
    }

    public void SetItem(int x, int y, InventorySlot slot)
    {
        Items[x, y] = slot;
        this.InventoryChanged?.Invoke(this, slot);
    }

    public InventorySlot[] GetRow(int y)
    {
        InventorySlot[] row = new InventorySlot[Items.GetLength(0)];
        for (int x = 0; x < Items.GetLength(0); x++)
        {
            row[x] = Items[x, y];
        }
        return row;
    }

    public void Render(Vector2 topLeftStart)
    {
        int slotSize = 64;
        int slotSpacing = 10;

        for (int x = 0; x < Items.GetLength(0); x++)
        {
            for (int y = 0; y < Items.GetLength(1); y++)
            {
                Renderer.Primitive.RenderRectangle(new RectangleF(topLeftStart.X + x * (slotSize + slotSpacing), topLeftStart.Y + y * (slotSize + slotSpacing), slotSize, slotSize), ColorF.DarkGray * 0.8f);
                if (Items[x, y] != null)
                {
                    InventorySlot slot = Items[x, y];
                    Item item = ItemManager.GetItem(slot.Item);

                    Renderer.Texture.Render(item.Texture, new Vector2(topLeftStart.X + x * (slotSize + slotSpacing), topLeftStart.Y + y * (slotSize + slotSpacing)), new Vector2(slotSize / item.Texture.Width, slotSize / item.Texture.Height), 0f, ColorF.White);
                }
            }
        }
    }

    public Vector2 GetRenderSize()
    {
        int slotSize = 64;
        int slotSpacing = 10;

        return new Vector2(Items.GetLength(0) * (slotSize + slotSpacing), Items.GetLength(1) * (slotSize + slotSpacing));
    }
}