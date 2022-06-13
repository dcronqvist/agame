using AGame.Engine.ECSys;
using GameUDPProtocol;

namespace AGame.Engine.Networking;

public abstract class PacketableEventArgs : EventArgs, IPacketable
{
    public abstract int Populate(byte[] data, int offset);
    public abstract byte[] ToBytes();
}

public class TriggerECEventPacket : Packet
{
    public int EntityID { get; set; }
    public string ComponentType { get; set; }
    public int EventID { get; set; }
    public PacketableEventArgs EventArgs { get; set; }

    public TriggerECEventPacket()
    {

    }

    public TriggerECEventPacket(int entityId, Component comp, int eventId, PacketableEventArgs eventArgs)
    {
        this.EntityID = entityId;
        this.ComponentType = comp.GetType().Name.Replace("Component", "");
        this.EventID = eventId;
        this.EventArgs = eventArgs;
    }

    public T GetEventArgsAs<T>() where T : PacketableEventArgs
    {
        return (T)this.EventArgs;
    }
}