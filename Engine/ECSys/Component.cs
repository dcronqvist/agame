using System.Reflection;
using AGame.Engine.Networking;
using GameUDPProtocol;

namespace AGame.Engine.ECSys;

public abstract class Component : IPacketable
{
    public string ComponentType { get; set; }

    public abstract Component Clone();
    public abstract int Populate(byte[] data, int offset);
    public abstract byte[] ToBytes();
    public new abstract string ToString();
    public abstract void UpdateComponent(Component newComponent);

    public void InterpolateProperties()
    {
        PropertyInfo[] infos = this.GetType().GetProperties().Where(x => Utilities.IsSubclassOfRawGeneric(typeof(Interpolated<>), x.PropertyType)).ToArray();

        foreach (PropertyInfo pi in infos)
        {
            pi.GetValue(this).GetType().GetMethod("Update").Invoke(pi.GetValue(this), new object[] { GameTime.DeltaTime });
        }
    }
}