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
}