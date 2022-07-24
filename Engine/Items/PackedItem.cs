using System;
using System.Collections.Generic;
using System.Text;
using AGame.Engine.Assets;
using GameUDPProtocol;

namespace AGame.Engine.Items;

public class PackedItem : IPacketable
{
    public ItemInstance Instance { get; set; }

    public PackedItem()
    {

    }

    public PackedItem(ItemInstance instance)
    {
        this.Instance = instance;
    }

    public int Populate(byte[] data, int offset)
    {
        this.Instance = null;
        int start = offset;

        byte exists = data[offset];
        offset += sizeof(byte);

        if (exists == 0)
        {
            return offset - start;
        }

        int itemIDLength = BitConverter.ToInt32(data, offset);
        offset += sizeof(int);
        string itemID = Encoding.UTF8.GetString(data, offset, itemIDLength);
        offset += itemIDLength;

        var itemDef = ItemManager.GetItemDef(itemID);
        var item = itemDef.CreateItem();
        this.Instance = item;

        int componentCount = BitConverter.ToInt32(data, offset);
        offset += sizeof(int);
        for (int i = 0; i < componentCount; i++)
        {
            int componentTypeLen = BitConverter.ToInt32(data, offset);
            offset += sizeof(int);
            string componentType = Encoding.UTF8.GetString(data, offset, componentTypeLen);
            offset += componentTypeLen;

            if (item.TryGetComponent(componentType, out ItemComponent c))
            {
                offset += c.Populate(data, offset);
            }
        }

        return offset - start;
    }

    public byte[] ToBytes()
    {
        List<byte> bytes = new List<byte>();

        if (this.Instance == null)
        {
            bytes.Add((byte)0);
        }
        else
        {
            bytes.Add((byte)1);

            bytes.AddRange(BitConverter.GetBytes(this.Instance.ItemID.Length));
            bytes.AddRange(Encoding.UTF8.GetBytes(this.Instance.ItemID));

            bytes.AddRange(BitConverter.GetBytes(this.Instance.Components.Count));

            foreach (ItemComponent component in this.Instance.Components)
            {
                bytes.AddRange(BitConverter.GetBytes(component.GetTypeName().Length));
                bytes.AddRange(Encoding.UTF8.GetBytes(component.GetTypeName()));
                bytes.AddRange(component.ToBytes());
            }
        }
        return bytes.ToArray();
    }
}