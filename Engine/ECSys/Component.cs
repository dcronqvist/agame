using System.ComponentModel;
using System.Reflection;
using AGame.Engine.Networking;
using GameUDPProtocol;

namespace AGame.Engine.ECSys;

public abstract class Component : IPacketable, INotifyPropertyChanged
{
    public string ComponentType { get; set; }

    public event PropertyChangedEventHandler PropertyChanged;

    public abstract Component Clone();
    public abstract int Populate(byte[] data, int offset);
    public abstract byte[] ToBytes();
    public new abstract string ToString();
    public abstract void UpdateComponent(Component newComponent);

    public abstract void InterpolateProperties();

    public bool ShouldSnapshot()
    {
        NetworkingBehaviourAttribute nba = this.GetType().GetCustomAttribute(typeof(NetworkingBehaviourAttribute), false) as NetworkingBehaviourAttribute;

        if (nba is not null)
        {
            return nba.Type == NBType.Snapshot;
        }

        return true;
    }
}