using System.Text;
using AGame.Engine.Assets;
using AGame.Engine.Items;
using GameUDPProtocol;

namespace AGame.Engine.Networking;

public class SetInventoryContentPacket : Packet
{
    public int EntityID { get; set; }
    public int Width { get; set; }
    public int Height { get; set; }
    public InventorySlot[,] Slots { get; set; }

    public SetInventoryContentPacket()
    {

    }

    public SetInventoryContentPacket(int entityID, Inventory inventory)
    {
        this.EntityID = entityID;
        this.Slots = inventory.GetSlots();
        this.Width = inventory.GetSlots().GetLength(0);
        this.Height = inventory.GetSlots().GetLength(1);
    }

    private byte[] BytifySlot(InventorySlot slot)
    {
        string item = slot.Item;
        int count = slot.Count;

        List<byte> bytes = new List<byte>();
        bytes.AddRange(BitConverter.GetBytes(item.Length));
        bytes.AddRange(Encoding.UTF8.GetBytes(item));
        bytes.AddRange(BitConverter.GetBytes(count));
        return bytes.ToArray();
    }

    private int ParseSlot(byte[] data, int offset, out InventorySlot slot)
    {
        int start = offset;
        int itemLength = BitConverter.ToInt32(data, offset);
        offset += sizeof(int);
        string item = Encoding.UTF8.GetString(data, offset, itemLength);
        offset += itemLength;
        int count = BitConverter.ToInt32(data, offset);
        offset += sizeof(int);

        slot = new InventorySlot(item, count);
        return offset - start;
    }

    public override byte[] ToBytes()
    {
        List<byte> bytes = new List<byte>();
        bytes.AddRange(this.Header.ToBytes());

        bytes.AddRange(BitConverter.GetBytes(this.EntityID));
        bytes.AddRange(BitConverter.GetBytes(this.Width));
        bytes.AddRange(BitConverter.GetBytes(this.Height));

        for (int i = 0; i < this.Slots.GetLength(0); i++)
        {
            for (int j = 0; j < this.Slots.GetLength(1); j++)
            {
                InventorySlot slot = this.Slots[i, j];
                if (slot != null)
                {
                    bytes.AddRange(this.BytifySlot(slot));
                }
                else
                {
                    bytes.Add(0);
                }
            }
        }

        return bytes.ToArray();
    }

    public override void Populate(byte[] data, int offset)
    {
        // Header is already fixed here
        this.EntityID = BitConverter.ToInt32(data, offset);
        offset += sizeof(int);
        this.Width = BitConverter.ToInt32(data, offset);
        offset += sizeof(int);
        this.Height = BitConverter.ToInt32(data, offset);
        offset += sizeof(int);

        this.Slots = new InventorySlot[this.Width, this.Height];
        for (int i = 0; i < this.Width; i++)
        {
            for (int j = 0; j < this.Height; j++)
            {
                if (data[offset] == 0)
                {
                    //Empty slot, continue
                    offset++;
                    continue;
                }
                else
                {
                    // Non-empty slot
                    InventorySlot slot;
                    offset += this.ParseSlot(data, offset, out slot);
                    this.Slots[i, j] = slot;
                }
            }
        }
    }
}