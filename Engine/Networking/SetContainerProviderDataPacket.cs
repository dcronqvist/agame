using GameUDPProtocol;

namespace AGame.Engine.Networking;

public class SetContainerProviderDataPacket : Packet
{
    public int EntityID { get; set; }
    public byte[] Data { get; set; }

    public static SetContainerProviderDataPacket GetDefault(int entityID)
    {
        return new SetContainerProviderDataPacket()
        {
            EntityID = entityID,
            Data = new byte[0]
        };
    }
}