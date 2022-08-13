using System;
using System.Collections.Generic;
using AGame.Engine;
using AGame.Engine.Assets.Scripting;
using AGame.Engine.ECSys;
using AGame.Engine.Networking;
using AGame.Engine.World;

namespace DefaultMod;

[ComponentNetworking(CreateTriggersNetworkUpdate = true, UpdateTriggersNetworkUpdate = true, NetworkUpdateRate = 5), ScriptType(Name = "shadow_component")]
public class ShadowComponent : Component
{
    private float _radius;
    [ComponentProperty(0, typeof(FloatPacker), typeof(FloatInterpolator), InterpolationType.Linear)]
    public float Radius
    {
        get => _radius;
        set
        {
            if (_radius != value)
            {
                _radius = value;
                this.NotifyPropertyChanged();
            }
        }
    }

    private float _opacity;
    [ComponentProperty(1, typeof(FloatPacker), typeof(FloatInterpolator), InterpolationType.Linear)]
    public float Opacity
    {
        get => _opacity;
        set
        {
            if (_opacity != value)
            {
                _opacity = value;
                this.NotifyPropertyChanged();
            }
        }
    }

    public override void ApplyInput(Entity parentEntity, UserCommand command, WorldContainer world, ECS ecs)
    {

    }

    public override Component Clone()
    {
        return new ShadowComponent()
        {
            Radius = this.Radius,
            Opacity = this.Opacity
        };
    }

    public override ulong GetHash()
    {
        return Utilities.CombineHash(this.Radius.Hash(), this.Opacity.Hash());
    }

    public override string ToString()
    {
        return $"ShadowComponent: {this.Radius}, {this.Opacity}";
    }
}