using System.Linq;
using System.Text;
using AGame.Engine.Assets;
using AGame.Engine.Items;
using GameUDPProtocol;

namespace AGame.Engine.Networking;

public class SetContainerContentPacket : Packet
{
    public bool OpenInteract { get; set; }
    public int EntityID { get; set; }
    public ContainerSlotInfo[] Slots { get; set; }

    public SetContainerContentPacket()
    {

    }

    public SetContainerContentPacket(int entityID, Container container, bool openInteract)
    {
        this.EntityID = entityID;
        this.Slots = container.GetSlotInfos().ToArray();
        this.OpenInteract = openInteract;
    }
}