using System;
using System.Collections.Generic;
using System.Text;
using AGame.Engine;
using AGame.Engine.Assets.Scripting;
using AGame.Engine.ECSys;
using AGame.Engine.Items;
using AGame.Engine.Networking;
using AGame.Engine.World;

namespace DefaultMod;

[ComponentNetworking(CreateTriggersNetworkUpdate = true, UpdateTriggersNetworkUpdate = false), ScriptClass(Name = "container_component")]
public class ContainerComponent : Component
{
    private string _containerProvider;
    public string ContainerProvider
    {
        get => _containerProvider;
        set
        {
            if (_containerProvider != value)
            {
                _containerProvider = value;
                this.NotifyPropertyChanged();
            }
        }
    }

    private Container _container;
    public Container GetContainer()
    {
        if (_container == null)
        {
            _container = new Container((IContainerProvider)ScriptingManager.CreateInstance(this.ContainerProvider));
        }

        return _container;
    }

    public override void ApplyInput(Entity parentEntity, UserCommand command, WorldContainer world, ECS ecs)
    {

    }

    public override Component Clone()
    {
        return new ContainerComponent()
        {
            ContainerProvider = this.ContainerProvider
        };
    }

    public override ulong GetHash()
    {
        return Utilities.Hash(this.ToBytes());
    }

    public override void InterpolateProperties(Component from, Component to, float amt)
    {
        var toC = (ContainerComponent)to;
        this.ContainerProvider = toC.ContainerProvider;
        this._container = null;
    }

    public override int Populate(byte[] data, int offset)
    {
        int start = offset;
        int len = BitConverter.ToInt32(data, offset);
        offset += sizeof(int);
        this.ContainerProvider = Encoding.UTF8.GetString(data, offset, len);
        offset += len;
        return offset - start;
    }

    public override byte[] ToBytes()
    {
        List<byte> bytes = new List<byte>();
        bytes.AddRange(BitConverter.GetBytes(this.ContainerProvider.Length));
        bytes.AddRange(Encoding.UTF8.GetBytes(this.ContainerProvider));
        return bytes.ToArray();
    }

    public override string ToString()
    {
        return $"ContainerComponent: {this.ContainerProvider}";
    }

    public override void UpdateComponent(Component newComponent)
    {
        var newC = (ContainerComponent)newComponent;
        this.ContainerProvider = newC.ContainerProvider;
        this._container = null;
    }
}