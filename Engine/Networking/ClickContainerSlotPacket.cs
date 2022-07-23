using GameUDPProtocol;

namespace AGame.Engine.Networking;

public class ClickContainerSlotPacket : Packet
{
    public int EntityID { get; set; }
    public int SlotID { get; set; }
}