using System.ComponentModel;
using System.Reflection;
using System.Runtime.CompilerServices;
using AGame.Engine.Networking;
using GameUDPProtocol;

namespace AGame.Engine.ECSys;

public abstract class Component : IPacketable, INotifyPropertyChanged
{
    private string _componentType;

    [PacketPropIgnore]
    public string ComponentType
    {
        get => _componentType;

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

    // This method is called by the Set accessor of each property.  
    // The CallerMemberName attribute that is applied to the optional propertyName  
    // parameter causes the property name of the caller to be substituted as an argument.  
    protected void NotifyPropertyChanged([CallerMemberName] String propertyName = "")
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    public abstract Component Clone();
    public abstract int Populate(byte[] data, int offset);
    public abstract byte[] ToBytes();
    public new abstract string ToString();
    public abstract void UpdateComponent(Component newComponent);
    public abstract void InterpolateProperties();

    public ComponentNetworkingAttribute GetCNAttrib()
    {
        return this.GetType().GetCustomAttribute<ComponentNetworkingAttribute>(true);
    }

    public bool HasCNType(CNType type)
    {
        return this.GetType().GetCustomAttribute<ComponentNetworkingAttribute>()?.Type == type;
    }

    public bool HasCNType(CNType type, NDirection direction)
    {
        return this.GetType().GetCustomAttribute<ComponentNetworkingAttribute>()?.Has(type, direction) ?? false;
    }

    public int GetPacketSize()
    {
        return this.ToBytes().Length;
    }

    private EventInfo[] GetEvents()
    {
        return this.GetType().GetEvents().OrderBy(e => e.Name).ToArray();
    }

    public Type GetEventArgsType(int id)
    {
        return this.GetEvents()[id].EventHandlerType.GetGenericArguments()[0];
    }

    public void TriggerComponentEvent<T>(int id, T eventArgs) where T : EventArgs
    {
        // Find all
        EventInfo[] events = this.GetEvents();
        events[id].GetRaiseMethod().Invoke(this, new object[] { eventArgs });
    }

    public void TriggerComponentEvent(Type eventArgsType, int id, object eventArgs)
    {
        // Find all
        EventInfo[] events = this.GetEvents();
        events[id].GetRaiseMethod().Invoke(this, new object[] { Convert.ChangeType(eventArgs, eventArgsType) });
    }
}