using System.Text;
using AGame.Engine.ECSys;
using GameUDPProtocol;

namespace AGame.Engine.Networking;

public class UpdateEntityComponentPacket : Packet
{
    public int EntityID { get; set; }
    public string ComponentType { get; set; }
    public Component Component { get; set; }

    public UpdateEntityComponentPacket()
    {

    }

    public UpdateEntityComponentPacket(int entityId, Component component)
    {
        this.EntityID = entityId;
        this.ComponentType = component.GetType().Name.Replace("Component", "");
        this.Component = component;
    }

    public UpdateEntityComponentPacket(int entityId, string type, Component component)
    {
        EntityID = entityId;
        ComponentType = type;
        Component = component;
    }

    public override byte[] ToBytes()
    {
        List<byte> bytes = new List<byte>();
        bytes.AddRange(this.Header.ToBytes());
        bytes.AddRange(BitConverter.GetBytes(EntityID));
        bytes.AddRange(BitConverter.GetBytes(ComponentType.Length));
        bytes.AddRange(Encoding.UTF8.GetBytes(ComponentType));
        bytes.AddRange(Component.ToBytes());
        return bytes.ToArray();
    }

    public override void Populate(byte[] data, int offset)
    {
        this.Header = new PacketHeader();
        offset = Header.Populate(data, offset);

        this.EntityID = BitConverter.ToInt32(data, offset);
        int length = BitConverter.ToInt32(data, offset + 4);
        this.ComponentType = Encoding.UTF8.GetString(data, offset + 8, length);

        Component comp = Activator.CreateInstance(ECS.Instance.Value.GetComponentType(ComponentType)) as Component;
        comp.Populate(data, offset + 8 + length);

        this.Component = comp;
    }
}