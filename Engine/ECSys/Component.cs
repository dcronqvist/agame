using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text.Json.Serialization;
using AGame.Engine.Networking;
using AGame.Engine.World;

namespace AGame.Engine.ECSys;

public interface IComponentPropertyPacker
{
    byte[] Pack(object value);
    int Unpack(byte[] data, int offset, out object value);
}

public enum InterpolationType
{
    Linear,
    FromInstant,
    ToInstant
}
public interface IComponentPropertyInterpolator
{
    object Interpolate(InterpolationType type, object a, object b, float t);
}

public abstract class ComponentPropertyInterpolator<T> : IComponentPropertyInterpolator
{
    public abstract object Interpolate(T a, T b, float t);

    public object Interpolate(InterpolationType type, object a, object b, float t)
    {
        switch (type)
        {
            case InterpolationType.Linear:
                return Interpolate((T)a, (T)b, t);
            case InterpolationType.FromInstant:
                return a;
            case InterpolationType.ToInstant:
                return b;
            default:
                throw new ArgumentException($"Invalid interpolation type {type}");
        }
    }
}

public abstract class ComponentPropertyPacker<T> : IComponentPropertyPacker
{
    public byte[] Pack(object value)
    {
        return Pack((T)value);
    }

    public int Unpack(byte[] data, int offset, out object value)
    {
        T val;
        int size = Unpack(data, offset, out val);
        value = val;
        return size;
    }

    public abstract byte[] Pack(T value);
    public abstract int Unpack(byte[] data, int offset, out T value);
}

[AttributeUsage(AttributeTargets.Property)]
public class ComponentPropertyAttribute : Attribute
{
    public byte Index { get; set; }
    public Type PackerType { get; set; }
    public Type InterpolatorType { get; set; }
    public InterpolationType Interpolation { get; set; }

    public ComponentPropertyAttribute(byte index, Type packerType, Type interpolatorType, InterpolationType interpolation)
    {
        Index = index;
        PackerType = packerType;
        InterpolatorType = interpolatorType;
        Interpolation = interpolation;
    }

    private IComponentPropertyPacker _packer;
    public IComponentPropertyPacker GetPacker()
    {
        if (_packer == null)
        {
            _packer = (IComponentPropertyPacker)Activator.CreateInstance(PackerType);
        }

        return _packer;
    }

    private IComponentPropertyInterpolator _interpolator;
    public IComponentPropertyInterpolator GetInterpolator()
    {
        if (_interpolator == null)
        {
            _interpolator = (IComponentPropertyInterpolator)Activator.CreateInstance(InterpolatorType);
        }

        return _interpolator;
    }
}

[AttributeUsage(AttributeTargets.Property)]
public class ComponentPropertyIgnoreAttribute : Attribute
{
}

public abstract class Component : INotifyPropertyChanged
{
    private string _componentType;

    [ComponentPropertyIgnoreAttribute]
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

    // property -> queue of (time, value)
    [JsonIgnore]
    private Dictionary<string, Queue<(float, object)>> _interpolationQueues = new Dictionary<string, Queue<(float, object)>>();

    // This method is called by the Set accessor of each property.  
    // The CallerMemberName attribute that is applied to the optional propertyName  
    // parameter causes the property name of the caller to be substituted as an argument.  
    public void NotifyPropertyChanged([CallerMemberName] String propertyName = "")
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    public abstract Component Clone();
    public new abstract string ToString();
    public abstract ulong GetHash();
    public abstract void ApplyInput(Entity parentEntity, UserCommand command, WorldContainer world, ECS ecs);

    public string[] GetAllProperties()
    {
        var props = this.GetType().GetProperties().Select(p => p.Name).Where(p => !this.IsPropertyIgnored(p)).ToArray();
        return props;
    }

    public bool IsPropertyIgnored(string property)
    {
        return this.GetType().GetProperty(property).GetCustomAttribute(typeof(ComponentPropertyIgnoreAttribute)) != null;
    }

    public byte[] GetBytes(string[] includedProperties)
    {
        List<byte> bytes = new List<byte>();
        var orderedProps = includedProperties.OrderBy(p => GetPropertyAttribute(p).Item2.Index).ToArray();
        var headerBytes = GetPropertiesIncludedBytes(includedProperties);
        bytes.AddRange(headerBytes);

        foreach (var p in orderedProps)
        {
            (var prop, var attr) = GetPropertyAttribute(p);
            var propPacker = attr.GetPacker();

            var propValue = prop.GetValue(this);
            var propBytes = propPacker.Pack(propValue);
            bytes.AddRange(propBytes);
        }

        return bytes.ToArray();
    }

    public int FromBytes(byte[] data, int offset)
    {
        int startOffset = offset;
        ushort header = BitConverter.ToUInt16(data, offset);
        offset += sizeof(ushort);

        var includedProperties = GetIncludedPropertiesFromHeader(header);

        foreach (var p in includedProperties)
        {
            (var prop, var attr) = GetPropertyAttribute(p);
            var propPacker = attr.GetPacker();
            offset += propPacker.Unpack(data, offset, out object propValue);

            prop.SetValue(this, propValue);
        }

        return offset - startOffset;
    }

    public string GetPropertyNameFromIndex(byte index)
    {
        var props = this.GetAllProperties();
        foreach (var p in props)
        {
            (var prop, var attr) = GetPropertyAttribute(p);
            if (attr.Index == index)
            {
                return prop.Name;
            }
        }
        return null;
    }

    public string[] GetIncludedPropertiesFromHeader(ushort header)
    {
        var includedProperties = new List<string>();
        for (int i = 0; i < 16; i++)
        {
            if ((header & (1 << i)) != 0)
            {
                includedProperties.Add(GetPropertyNameFromIndex((byte)i));
            }
        }
        return includedProperties.OrderBy(p => GetPropertyAttribute(p).Item2.Index).ToArray();
    }

    public byte[] GetPropertiesIncludedBytes(string[] includeProperties)
    {
        var indexes = includeProperties.OrderBy(p => GetPropertyAttribute(p).Item2.Index).Select(p => GetPropertyAttribute(p).Item2.Index).ToArray();

        ushort packed = 0;

        for (int i = 0; i < indexes.Length; i++)
        {
            packed |= (ushort)(1 << indexes[i]);
        }

        return BitConverter.GetBytes(packed);
    }

    public byte[] GetPropertyAsBytes(string propertyName)
    {
        (var prop, var attr) = this.GetPropertyAttribute(propertyName);
        return attr.GetPacker().Pack(prop.GetValue(this));
    }

    public void SetPropertyFromBytes(string propertyName, byte[] data, int offset)
    {
        (var prop, var attr) = this.GetPropertyAttribute(propertyName);
        var val = attr.GetPacker().Unpack(data, offset, out object value);
        prop.SetValue(this, value);
    }

    private Dictionary<string, (PropertyInfo, ComponentPropertyAttribute)> _propertyAttributes = new Dictionary<string, (PropertyInfo, ComponentPropertyAttribute)>();

    public (PropertyInfo, ComponentPropertyAttribute) GetPropertyAttribute(string propertyName)
    {
        if (!this._propertyAttributes.ContainsKey(propertyName))
        {
            var prop = this.GetType().GetProperty(propertyName);
            var attr = prop.GetCustomAttribute(typeof(ComponentPropertyAttribute)) as ComponentPropertyAttribute;
            this._propertyAttributes.Add(propertyName, (prop, attr));
        }

        return this._propertyAttributes[propertyName];
    }

    public ComponentNetworkingAttribute GetCNAttrib()
    {
        return this.GetType().GetCustomAttribute<ComponentNetworkingAttribute>(true);
    }

    public void PushPropertyInterpolationUpdate(byte[] data, int offset, float currentTime = -1f)
    {
        int startOffset = offset;
        ushort header = BitConverter.ToUInt16(data, offset);
        offset += sizeof(ushort);

        var includedProperties = GetIncludedPropertiesFromHeader(header);

        foreach (var p in includedProperties)
        {
            (var prop, var attr) = GetPropertyAttribute(p);
            var propPacker = attr.GetPacker();
            offset += propPacker.Unpack(data, offset, out object propValue);

            if (!_interpolationQueues.ContainsKey(p))
            {
                _interpolationQueues.Add(p, new Queue<(float, object)>());
            }

            float time = currentTime == -1f ? GameTime.TotalElapsedSeconds : currentTime;
            this._interpolationQueues[p].Enqueue((time, propValue));
        }
    }

    private void InterpolateProperty(string property, object from, object to, float amt)
    {
        (var prop, var attr) = GetPropertyAttribute(property);
        var propInterpolator = attr.GetInterpolator();

        object newValue = propInterpolator.Interpolate(attr.Interpolation, from, to, amt);

        this.GetType().GetProperty(property).SetValue(this, newValue);
    }

    public void InterpolateComponent(float interpolationTime)
    {
        float now = GameTime.TotalElapsedSeconds;
        float renderTimestamp = now - interpolationTime;

        foreach (string prop in this.GetAllProperties())
        {
            if (!this._interpolationQueues.ContainsKey(prop))
            {
                this._interpolationQueues.Add(prop, new Queue<(float, object)>());
            }

            Queue<(float, object)> queue = this._interpolationQueues[prop];

            while (queue.Count > 2 && queue.ElementAt(1).Item1 <= renderTimestamp)
            {
                queue.Dequeue();
            }

            if (queue.Count >= 2 && queue.ElementAt(0).Item1 <= renderTimestamp && queue.ElementAt(1).Item1 >= renderTimestamp)
            {
                (float, object) first = queue.ElementAt(0);
                (float, object) second = queue.ElementAt(1);

                float amt = (renderTimestamp - first.Item1) / (second.Item1 - first.Item1);

                this.InterpolateProperty(prop, first.Item2, second.Item2, amt);
            }
        }
    }

    private int _lastSentUpdateTick = -1;

    public bool ShouldSendUpdate(int tick)
    {
        if (tick >= this._lastSentUpdateTick + this.GetCNAttrib().NetworkUpdateRate)
        {
            this._lastSentUpdateTick = tick;
            return true;
        }
        else
        {
            return false;
        }
    }
}