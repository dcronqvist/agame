using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Numerics;
using System.Text;
using AGame.Engine.Assets;
using AGame.Engine.Graphics;
using AGame.Engine.Graphics.Rendering;
using AGame.Engine.Networking;
using GameUDPProtocol;

namespace AGame.Engine.Items;

public class ContainerSlotInfo : IPacketable
{
    public int SlotID { get; set; }
    public PackedItem Item { get; set; }
    public int ItemCount { get; set; }

    public ContainerSlotInfo()
    {

    }

    public ContainerSlotInfo(int slotID, ItemInstance item, int itemCount)
    {
        this.SlotID = slotID;
        this.Item = new PackedItem(item);
        this.ItemCount = itemCount;
    }

    public int Populate(byte[] data, int offset)
    {
        int start = offset;
        this.SlotID = BitConverter.ToInt32(data, offset);
        offset += sizeof(int);
        this.ItemCount = BitConverter.ToInt32(data, offset);
        offset += sizeof(int);
        this.Item = new PackedItem();
        offset += this.Item.Populate(data, offset);
        return offset - start;
    }

    public byte[] ToBytes()
    {
        List<byte> bytes = new List<byte>();
        bytes.AddRange(BitConverter.GetBytes(this.SlotID));
        bytes.AddRange(BitConverter.GetBytes(this.ItemCount));
        bytes.AddRange(this.Item.ToBytes());
        return bytes.ToArray();
    }

    public ulong GetHash()
    {
        return Utilities.CombineHash(this.SlotID.Hash(), this.ItemCount.Hash(), this.Item.GetHash());
    }
}

public class ContainerSlot
{
    public const int WIDTH = 64;
    public const int HEIGHT = 64;

    public Vector2 Position { get; set; }
    public ItemInstance Item { get; set; }
    public int Count { get; set; }

    public ContainerSlot(Vector2 position)
    {
        this.Position = position;
    }

    public Vector2 GetSize() => new Vector2(WIDTH, HEIGHT);

    public ContainerSlotInfo ToSlotInfo(int index) => new ContainerSlotInfo(index, this.Item, this.Count);
}

public class Container
{
    public IContainerProvider Provider { get; set; }

    private Dictionary<int, ContainerSlot> _slots;

    public Container(IContainerProvider provider)
    {
        this.Provider = provider;
        this._slots = new Dictionary<int, ContainerSlot>();

        this.InitializeSlots();
    }

    public IEnumerable<ContainerSlotInfo> GetSlotInfos()
    {
        foreach (var slot in this._slots)
        {
            if (slot.Value.Item != null)
            {
                yield return slot.Value.ToSlotInfo(slot.Key);
            }
            else
            {
                yield return new ContainerSlotInfo(slot.Key, null, 0);
            }
        }
    }

    private void InitializeSlots()
    {
        foreach (var slot in this.Provider.GetSlots())
        {
            this._slots.Add(this._slots.Count, slot);
        }
    }

    private int FindSlotWithSameItem(ItemInstance item)
    {
        List<int> slots = this.Provider.GetSlotSeekOrder().ToList();

        foreach (int slot in slots)
        {
            var cslot = this._slots[slot];

            if (cslot.Item is not null && cslot.Item.Definition.ItemID == item.Definition.ItemID && (cslot.Count + 1 <= cslot.Item.Definition.MaxStack))
            {
                return slot;
            }
        }

        return -1;
    }

    private int FindNextEmptySlot()
    {
        List<int> slots = this.Provider.GetSlotSeekOrder().ToList();

        foreach (int slot in slots)
        {
            var cslot = this._slots[slot];

            if (cslot.Item is null)
            {
                return slot;
            }
        }

        return -1;
    }

    public bool AddItem(ItemInstance item)
    {
        int sameItemSlot = this.FindSlotWithSameItem(item);

        if (sameItemSlot != -1)
        {
            var foundSlot = this._slots[sameItemSlot];
            foundSlot.Count += 1;
            return true;
        }
        else
        {
            int emptySlot = this.FindNextEmptySlot();

            if (emptySlot != -1)
            {
                var foundSlot = this._slots[emptySlot];
                foundSlot.Item = item;
                foundSlot.Count = 1;
                return true;
            }
            else
            {
                return false;
            }
        }
    }

    public void SetItemInSlot(int slot, ItemInstance item, int amount)
    {
        var foundSlot = this._slots[slot];
        foundSlot.Item = item;
        foundSlot.Count = amount;
    }

    public ItemInstance RemoveItem(int slot)
    {
        var item = this._slots[slot].Item;
        this._slots[slot].Count -= 1;
        if (this._slots[slot].Count <= 0)
        {
            this._slots[slot].Item = null;
        }

        return item;
    }

    public void SetSlotData(int slot, ItemInstance item, int count)
    {
        this._slots[slot].Item = item;
        this._slots[slot].Count = count;
    }

    public ContainerSlot GetSlot(int index)
    {
        return this._slots[index];
    }

    public bool UpdateLogic(float deltaTime)
    {
        return this.Provider.Update(deltaTime);
    }

    public void Render(Vector2 topLeft)
    {
        ContainerSlot hoveredSlot = null;
        var font = ModManager.GetAsset<Font>("default.font.rainyhearts");

        foreach (KeyValuePair<int, ContainerSlot> slot in this._slots)
        {
            // Render slot
            var position = slot.Value.Position + topLeft;
            var size = slot.Value.GetSize();
            var rec = new RectangleF(position.X, position.Y, size.X, size.Y);
            var color = ColorF.Black;

            if (rec.Contains(Input.GetMousePositionInWindow()))
            {
                color = ColorF.Gray;
            }

            Renderer.Primitive.RenderRectangle(rec, color * 0.5f);
            slot.Value.Item?.RenderInSlot(position);

            // Render count
            if (slot.Value.Item is not null)
            {
                float scale = 1f;
                var text = slot.Value.Count.ToString();
                var textSize = font.MeasureString(text, scale);
                var textPosition = position + new Vector2(size.X - textSize.X, size.Y - textSize.Y);
                Renderer.Text.RenderText(font, text, textPosition, scale, ColorF.White, Renderer.Camera);

                // If has tool component, render durability
                if (slot.Value.Item.TryGetComponent<DefaultMod.Tool>(out DefaultMod.Tool t))
                {
                    var durability = t.Definition.Durability;
                    var currDur = t.CurrentDurability;
                    var perc = ((float)currDur / durability).ToString("0.00");

                    var durabilitySize = font.MeasureString(perc, scale);
                    var durabilityPosition = position + new Vector2(size.X - durabilitySize.X, size.Y - durabilitySize.Y - textSize.Y);
                    Renderer.Text.RenderText(font, perc, durabilityPosition, scale, ColorF.White, Renderer.Camera);
                }
            }
        }

        if (hoveredSlot is not null && hoveredSlot.Item is not null)
        {
            Renderer.Text.RenderText(font, hoveredSlot.Item.Definition.Name, Input.GetMousePositionInWindow(), 2f, ColorF.White, Renderer.Camera);
        }
    }

    public void ClickSlot(int slot, ref ContainerSlot mouseSlot)
    {
        var containerSlot = this._slots[slot];

        if (mouseSlot.Item == null)
        {
            mouseSlot.Item = containerSlot.Item;
            mouseSlot.Count = containerSlot.Count;
            containerSlot.Item = null;
            containerSlot.Count = 0;
        }
        else
        {
            // Swap items
            var temp = mouseSlot.Item;
            var tempCount = mouseSlot.Count;
            mouseSlot.Item = containerSlot.Item;
            mouseSlot.Count = containerSlot.Count;
            containerSlot.Item = temp;
            containerSlot.Count = tempCount;
        }
    }

    public IEnumerable<ContainerSlot> GetSlots(params int[] slots)
    {
        foreach (var slot in slots)
        {
            yield return this._slots[slot];
        }
    }

    public void UpdateInteract(ref ContainerSlot mouseSlot, int localParentEntity, GameClient client, Vector2 topLeft, float deltaTime)
    {
        // Allow for interaction with the container.
        // This is where you would handle mouse clicks to move items around etc.
        var remoteParentID = client.GetRemoteIDForEntity(localParentEntity);

        foreach (KeyValuePair<int, ContainerSlot> slot in this._slots)
        {
            // Render slot
            var position = slot.Value.Position + topLeft;
            var size = slot.Value.GetSize();
            var rec = new RectangleF(position.X, position.Y, size.X, size.Y);

            if (rec.Contains(Input.GetMousePositionInWindow()))
            {
                if (Input.IsMouseButtonPressed(GLFW.MouseButton.Left))
                {
                    this.ClickSlot(slot.Key, ref mouseSlot);
                    client.EnqueuePacket(new ClickContainerSlotPacket() { SlotID = slot.Key, EntityID = remoteParentID }, true, false);
                }
            }
        }
    }
}