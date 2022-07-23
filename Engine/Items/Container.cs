using System.Drawing;
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
    public string ItemID { get; set; }
    public int ItemCount { get; set; }

    public ContainerSlotInfo()
    {

    }

    public ContainerSlotInfo(int slotID, string itemID, int itemCount)
    {
        this.SlotID = slotID;
        this.ItemID = itemID;
        this.ItemCount = itemCount;
    }

    public int Populate(byte[] data, int offset)
    {
        int start = offset;
        this.SlotID = BitConverter.ToInt32(data, offset);
        offset += sizeof(int);
        int itemIDLength = BitConverter.ToInt32(data, offset);
        offset += sizeof(int);
        this.ItemID = Encoding.UTF8.GetString(data, offset, itemIDLength);
        offset += itemIDLength;
        this.ItemCount = BitConverter.ToInt32(data, offset);
        offset += sizeof(int);
        return offset - start;
    }

    public byte[] ToBytes()
    {
        List<byte> bytes = new List<byte>();
        bytes.AddRange(BitConverter.GetBytes(this.SlotID));
        bytes.AddRange(BitConverter.GetBytes(this.ItemID.Length));
        bytes.AddRange(Encoding.UTF8.GetBytes(this.ItemID));
        bytes.AddRange(BitConverter.GetBytes(this.ItemCount));
        return bytes.ToArray();
    }
}

public class ContainerSlot
{
    public const int WIDTH = 64;
    public const int HEIGHT = 64;

    public Vector2 Position { get; set; }
    public string Item { get; set; }
    public int Count { get; set; }

    public ContainerSlot(Vector2 position)
    {
        this.Position = position;
    }

    public Item GetItem() => ItemManager.GetItem(Item);

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
                yield return new ContainerSlotInfo(slot.Key, "", 0);
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

    public IEnumerable<(string, int)> GetAllItemsInContainer()
    {
        foreach (var slot in this._slots)
        {
            if (slot.Value.Item != null)
            {
                yield return (slot.Value.Item, slot.Value.Count);
            }
        }
    }

    public bool AddItemsToContainer(string item, int amount, out int remaining)
    {
        return this.Provider.AddItems(item, amount, out remaining);
    }

    public void RemoveItem(int slot, int amount)
    {
        this.Provider.RemoveItem(slot, amount);
    }

    public void SetSlotData(int slot, string item, int count)
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

            if (slot.Value.Item != "" && slot.Value.Item != null)
            {
                var item = ItemManager.GetItem(slot.Value.Item);
                var itemSize = Vector2.One * 4f;

                Renderer.Texture.Render(item.Texture, position, itemSize, 0f, ColorF.White);

                var font = ModManager.GetAsset<Font>("default.font.rainyhearts");
                Renderer.Text.RenderText(font, slot.Value.Count.ToString(), position + new Vector2(32, 32), 1f, ColorF.White, Renderer.Camera);
            }
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