using System.Text;
using AGame.Engine.ECSys;
using GameUDPProtocol;

namespace AGame.Engine.Networking;

public class UpdateEntitiesPacket : Packet
{
    public EntityUpdate[] Updates { get; set; }

    public UpdateEntitiesPacket()
    {

    }

    public UpdateEntitiesPacket(params EntityUpdate[] updates)
    {
        this.Updates = updates;
    }
}

public class EntityUpdate : IPacketable
{
    public int EntityID { get; set; }
    public ushort[] ComponentTypes { get; set; }
    public Component[] Components { get; set; }

    public EntityUpdate()
    {

    }

    public EntityUpdate(int entityId, params Component[] components)
    {
        this.EntityID = entityId;
        this.ComponentTypes = components.Select(x => (ushort)ECS.Instance.Value.GetComponentID(x.GetType())).ToArray();
        this.Components = components;
    }

    public byte[] ToBytes()
    {
        List<byte> bytes = new List<byte>();
        bytes.AddRange(BitConverter.GetBytes(EntityID));
        bytes.AddRange(BitConverter.GetBytes(ComponentTypes.Length));

        foreach (ushort componentType in ComponentTypes)
        {
            bytes.AddRange(BitConverter.GetBytes(componentType));
        }

        foreach (Component component in Components)
        {
            bytes.AddRange(component.ToBytes());
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

        this.ComponentTypes = new ushort[length];
        this.Components = new Component[length];

        for (int i = 0; i < length; i++)
        {
            ushort componentType = BitConverter.ToUInt16(data, offset);
            this.ComponentTypes[i] = componentType;
            offset += sizeof(ushort);
        }

        for (int i = 0; i < this.ComponentTypes.Length; i++)
        {
            Component comp = Activator.CreateInstance(ECS.Instance.Value.GetComponentType(this.ComponentTypes[i])) as Component;
            offset += comp.Populate(data, offset);
            this.Components[i] = comp;
        }

        return offset - startOffset;
    }
}