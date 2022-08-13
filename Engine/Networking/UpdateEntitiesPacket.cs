using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AGame.Engine.ECSys;
using GameUDPProtocol;

namespace AGame.Engine.Networking;

public class UpdateEntitiesPacket : Packet
{
    public int LastProcessedCommand { get; set; }
    public EntityUpdate[] Updates { get; set; }
    public int[] DeleteEntities { get; set; }
    public int ServerTick { get; set; }

    public UpdateEntitiesPacket()
    {

    }

    public UpdateEntitiesPacket(int lastProcessedCommand, int serverTick, int[] deleteEntities, params EntityUpdate[] updates)
    {
        this.Updates = updates;
        this.LastProcessedCommand = lastProcessedCommand;
        this.DeleteEntities = deleteEntities;
        this.ServerTick = serverTick;
    }
}

public class EntityUpdate : IPacketable
{
    public int EntityID { get; set; }
    public Dictionary<ushort, byte[]> ComponentData { get; set; }

    public EntityUpdate()
    {

    }

    public EntityUpdate(int entityId, params (Component, string[])[] components)
    {
        this.EntityID = entityId;
        this.ComponentData = new Dictionary<ushort, byte[]>();

        foreach ((var comp, var props) in components)
        {
            var compBytes = comp.GetBytes(props);
            var compType = ECS.Instance.Value.GetComponentID(comp.GetType());
            this.ComponentData.Add((ushort)compType, compBytes);
        }
    }

    public byte[] ToBytes()
    {
        List<byte> bytes = new List<byte>();
        bytes.AddRange(BitConverter.GetBytes(EntityID));
        bytes.AddRange(BitConverter.GetBytes(ComponentData.Count));

        foreach (var kvp in this.ComponentData)
        {
            bytes.AddRange(BitConverter.GetBytes(kvp.Key));
            bytes.AddRange(BitConverter.GetBytes(kvp.Value.Length));
            bytes.AddRange(kvp.Value);
        }

        return bytes.ToArray();
    }

    public int Populate(byte[] data, int offset)
    {
        int startOffset = offset;
        this.EntityID = BitConverter.ToInt32(data, offset);
        offset += sizeof(int);

        int length = BitConverter.ToInt32(data, offset);
        offset += sizeof(int);

        this.ComponentData = new Dictionary<ushort, byte[]>();

        for (int i = 0; i < length; i++)
        {
            ushort compType = BitConverter.ToUInt16(data, offset);
            offset += sizeof(ushort);

            int compLength = BitConverter.ToInt32(data, offset);
            offset += sizeof(int);

            byte[] compBytes = new byte[compLength];
            Array.Copy(data, offset, compBytes, 0, compLength);

            offset += compLength;

            this.ComponentData.Add(compType, compBytes);
        }

        // this.ComponentTypes = new ushort[length];
        // this.Components = new Component[length];

        // for (int i = 0; i < length; i++)
        // {
        //     ushort componentType = BitConverter.ToUInt16(data, offset);
        //     this.ComponentTypes[i] = componentType;
        //     offset += sizeof(ushort);
        // }

        // for (int i = 0; i < this.ComponentTypes.Length; i++)
        // {
        //     Component comp = Activator.CreateInstance(ECS.Instance.Value.GetComponentType(this.ComponentTypes[i])) as Component;
        //     offset += comp.Populate(data, offset);
        //     this.Components[i] = comp;
        // }

        return offset - startOffset;
    }
}