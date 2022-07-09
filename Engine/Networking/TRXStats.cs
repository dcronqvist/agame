using AGame.Engine.ECSys;
using GameUDPProtocol;

namespace AGame.Engine.Networking;

public class TRXStats
{
    public int PacketsSent { get; set; }
    public int PacketsReceived { get; set; }

    public int PacketsSentBytes { get; set; }
    public int PacketsReceivedBytes { get; set; }

    public int PacketsSentAverageSize { get; set; }
    public int PacketsReceivedAverageSize { get; set; }

    public List<(Type, int)> PacketsSentByType { get; set; }
    public List<(Type, int)> PacketsReceivedByType { get; set; }

    public List<(Type, int)> PacketsSentBytesByType { get; set; }
    public List<(Type, int)> PacketsReceivedBytesByType { get; set; }

    public List<(Type, int)> ComponentUpdatesReceivedBytesByType { get; set; }

    public TRXStats(List<Packet> receivedPackets, List<Packet> sentPackets)
    {
        PacketsSent = sentPackets.Count;
        PacketsReceived = receivedPackets.Count;
        PacketsSentBytes = sentPackets.Sum(x => x.ToBytes().Length);
        PacketsReceivedBytes = receivedPackets.Sum(x => x.ToBytes().Length);
        PacketsSentAverageSize = PacketsSentBytes / Math.Max(PacketsSent, 1);
        PacketsReceivedAverageSize = PacketsReceivedBytes / Math.Max(PacketsReceived, 1);
        PacketsSentByType = sentPackets.GroupBy(x => x.GetType()).Select(x => (x.Key, x.Count())).ToList();
        PacketsReceivedByType = receivedPackets.GroupBy(x => x.GetType()).Select(x => (x.Key, x.Count())).ToList();
        PacketsSentBytesByType = sentPackets.GroupBy(x => x.GetType()).Select(x => (x.Key, x.Sum(y => y.ToBytes().Length))).ToList();
        PacketsReceivedBytesByType = receivedPackets.GroupBy(x => x.GetType()).Select(x => (x.Key, x.Sum(y => y.ToBytes().Length))).ToList();

        ComponentUpdatesReceivedBytesByType = new List<(Type, int)>();

        foreach (Packet packet in receivedPackets)
        {
            if (packet is UpdateEntitiesPacket)
            {
                UpdateEntitiesPacket updateEntitiesPacket = (UpdateEntitiesPacket)packet;
                foreach (EntityUpdate eu in updateEntitiesPacket.Updates)
                {
                    foreach (Component c in eu.Components)
                    {
                        if (!ComponentUpdatesReceivedBytesByType.Any(x => x.Item1 == c.GetType()))
                        {
                            ComponentUpdatesReceivedBytesByType.Add((c.GetType(), 0));
                        }
                        else
                        {
                            int index = ComponentUpdatesReceivedBytesByType.FindIndex(x => x.Item1 == c.GetType());
                            ComponentUpdatesReceivedBytesByType[index] = (c.GetType(), ComponentUpdatesReceivedBytesByType[index].Item2 + c.ToBytes().Length);
                        }
                    }
                }
            }
        }
    }

    public string GetRXBytesString()
    {
        return Utilities.GetBytesPerSecondAsString(PacketsReceivedBytes);
    }

    public string GetTXBytesString()
    {
        return Utilities.GetBytesPerSecondAsString(PacketsSentBytes);
    }
}