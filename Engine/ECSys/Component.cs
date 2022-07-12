using System.ComponentModel;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text.Json;
using System.Text.Json.Serialization;
using AGame.Engine.Networking;
using AGame.Engine.World;
using GameUDPProtocol;

namespace AGame.Engine.ECSys;

public abstract class Component : IPacketable, INotifyPropertyChanged
{
    private string _componentType;

    [PacketPropIgnore]
    public string ComponentType
    {
        get
        {
            if (_componentType == null)
            {
                _componentType = this.GetType().Name.Replace("Component", "");
            }

            return _componentType;
        }

        set
        {
            if (value != this._componentType)
            {
                this._componentType = value;
                this.NotifyPropertyChanged();
            }
        }
    }

    public event PropertyChangedEventHandler PropertyChanged;

    [JsonIgnore]
    [PacketPropIgnore]
    private Queue<(float, Component)> _interpolationQueue = new Queue<(float, Component)>();

    // This method is called by the Set accessor of each property.  
    // The CallerMemberName attribute that is applied to the optional propertyName  
    // parameter causes the property name of the caller to be substituted as an argument.  
    public void NotifyPropertyChanged([CallerMemberName] String propertyName = "")
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    public abstract Component Clone();
    public abstract int Populate(byte[] data, int offset);
    public abstract byte[] ToBytes();
    public new abstract string ToString();
    public abstract void UpdateComponent(Component newComponent);
    public abstract void InterpolateProperties(Component from, Component to, float amt);
    public new abstract int GetHashCode();
    public abstract void ApplyInput(UserCommand command, WorldContainer world);

    public ComponentNetworkingAttribute GetCNAttrib()
    {
        return this.GetType().GetCustomAttribute<ComponentNetworkingAttribute>(true);
    }

    public int GetPacketSize()
    {
        return this.ToBytes().Length;
    }

    public void PushComponentUpdate(Component component)
    {
        float currentTime = GameTime.TotalElapsedSeconds;
        this._interpolationQueue.Enqueue((currentTime, component));
    }

    public void InterpolateComponent(float interpolationTime)
    {
        float now = GameTime.TotalElapsedSeconds;
        float renderTimestamp = now - interpolationTime;

        Queue<(float, Component)> queue = this._interpolationQueue;

        while (queue.Count > 2 && queue.ElementAt(1).Item1 <= renderTimestamp)
        {
            queue.Dequeue();
        }

        if (queue.Count >= 2 && queue.ElementAt(0).Item1 <= renderTimestamp && queue.ElementAt(1).Item1 >= renderTimestamp)
        {
            (float, Component) first = queue.ElementAt(0);
            (float, Component) second = queue.ElementAt(1);

            float amt = (renderTimestamp - first.Item1) / (second.Item1 - first.Item1);

            this.InterpolateProperties(first.Item2, second.Item2, Math.Max(amt, 0));
        }
    }
}