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

[ComponentNetworking(CreateTriggersNetworkUpdate = true, UpdateTriggersNetworkUpdate = false), ScriptType(Name = "container_component")]
public class ContainerComponent : Component
{
    private string _containerProvider;
    [ComponentProperty(0, typeof(StringPacker), typeof(StringInterpolator), InterpolationType.ToInstant)]
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
            _container = new Container(ScriptingManager.CreateInstance<IContainerProvider>(this.ContainerProvider));
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
        return this.ContainerProvider.Hash();
    }

    public override string ToString()
    {
        return $"ContainerComponent: {this.ContainerProvider}";
    }
}