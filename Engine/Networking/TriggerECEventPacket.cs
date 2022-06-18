using AGame.Engine.ECSys;
using GameUDPProtocol;

namespace AGame.Engine.Networking;

public abstract class PacketableEventArgs : EventArgs, IPacketable
{
    public abstract int Populate(byte[] data, int offset);
    public abstract byte[] ToBytes();

    public new static TECEventArgsEmpty Empty { get; } = new TECEventArgsEmpty();
}

public class TECEventArgsEmpty : PacketableEventArgs
{
    public override int Populate(byte[] data, int offset)
    {
        return 0;
    }

    public override byte[] ToBytes()
    {
        return new byte[0];
    }
}

public class TriggerECEventPacket : Packet
{
    public int EntityID { get; set; }
    public int ComponentTypeID { get; set; }
    public int EventID { get; set; }
    public PacketableEventArgs EventArgs { get; set; }

    public TriggerECEventPacket()
    {

    }

    public TriggerECEventPacket(int entityId, Component comp, int eventId, PacketableEventArgs eventArgs)
    {
        this.EntityID = entityId;
        this.ComponentTypeID = ECS.Instance.Value.GetComponentID(comp.GetType());
        this.EventID = eventId;
        this.EventArgs = eventArgs;
    }

    public T GetEventArgsAs<T>() where T : PacketableEventArgs
    {
        return (T)this.EventArgs;
    }

    public override byte[] ToBytes()
    {
        List<byte> bytes = new List<byte>();

        // Must include header
        bytes.AddRange(this.Header.ToBytes());

        bytes.AddRange(BitConverter.GetBytes(this.EntityID));
        bytes.AddRange(BitConverter.GetBytes(this.ComponentTypeID));
        bytes.AddRange(BitConverter.GetBytes(this.EventID));

        // This typing lookup should be cached so it doesn't have to be done every time
        Type[] types = Utilities.FindDerivedTypes(typeof(PacketableEventArgs)).OrderBy(x => x.Name).ToArray();
        int index = Array.IndexOf(types, this.EventArgs.GetType());

        bytes.AddRange(BitConverter.GetBytes(index));
        bytes.AddRange(this.EventArgs.ToBytes());

        return bytes.ToArray();
    }

    public override void Populate(byte[] data, int offset)
    {
        int startOffset = offset;
        this.EntityID = BitConverter.ToInt32(data, offset);
        offset += sizeof(int);

        this.ComponentTypeID = BitConverter.ToInt32(data, offset);
        offset += sizeof(int);

        this.EventID = BitConverter.ToInt32(data, offset);
        offset += sizeof(int);

        int index = BitConverter.ToInt32(data, offset);
        offset += sizeof(int);

        // This typing lookup should be cached so it doesn't have to be done every time
        Type[] types = Utilities.FindDerivedTypes(typeof(PacketableEventArgs)).OrderBy(x => x.Name).ToArray();
        this.EventArgs = (PacketableEventArgs)Activator.CreateInstance(types[index]);

        this.EventArgs.Populate(data, offset);
    }
}