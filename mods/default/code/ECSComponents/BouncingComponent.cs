using System;
using System.Collections.Generic;
using System.Numerics;
using AGame.Engine;
using AGame.Engine.Assets.Scripting;
using AGame.Engine.ECSys;
using AGame.Engine.Networking;
using AGame.Engine.World;

namespace DefaultMod;

[ComponentNetworking(CreateTriggersNetworkUpdate = false, UpdateTriggersNetworkUpdate = false, NetworkUpdateRate = 50), ScriptType(Name = "bouncing_component")]
public class BouncingComponent : Component
{
    private float _gravityFactor;
    [ComponentProperty(0, typeof(FloatPacker), typeof(FloatInterpolator), InterpolationType.Linear)]
    public float GravityFactor
    {
        get => _gravityFactor;
        set
        {
            if (_gravityFactor != value)
            {
                _gravityFactor = value;
                this.NotifyPropertyChanged();
            }
        }
    }

    private float _fallOffFactor;
    [ComponentProperty(1, typeof(FloatPacker), typeof(FloatInterpolator), InterpolationType.Linear)]
    public float FallOffFactor
    {
        get => _fallOffFactor;
        set
        {
            if (_fallOffFactor != value)
            {
                _fallOffFactor = value;
                this.NotifyPropertyChanged();
            }
        }
    }

    private float _verticalVelocity;
    [ComponentProperty(2, typeof(FloatPacker), typeof(FloatInterpolator), InterpolationType.Linear)]
    public float VerticalVelocity
    {
        get => _verticalVelocity;
        set
        {
            if (_verticalVelocity != value)
            {
                _verticalVelocity = value;
                this.NotifyPropertyChanged();
            }
        }
    }

    private Vector2 _velocity;
    [ComponentProperty(3, typeof(Vector2Packer), typeof(Vector2Interpolator), InterpolationType.Linear)]
    public Vector2 Velocity
    {
        get => _velocity;
        set
        {
            if (_velocity != value)
            {
                _velocity = value;
                this.NotifyPropertyChanged();
            }
        }
    }

    private float _velocityFriction;
    [ComponentProperty(4, typeof(FloatPacker), typeof(FloatInterpolator), InterpolationType.Linear)]
    public float VelocityFriction
    {
        get => _velocityFriction;
        set
        {
            if (_velocityFriction != value)
            {
                _velocityFriction = value;
                this.NotifyPropertyChanged();
            }
        }
    }

    private float _velocityThreshold;
    [ComponentProperty(5, typeof(FloatPacker), typeof(FloatInterpolator), InterpolationType.Linear)]
    public float VelocityThreshold
    {
        get => _velocityThreshold;
        set
        {
            if (_velocityThreshold != value)
            {
                _velocityThreshold = value;
                this.NotifyPropertyChanged();
            }
        }
    }

    public override void ApplyInput(Entity parentEntity, UserCommand command, WorldContainer world, ECS ecs)
    {

    }

    public override Component Clone()
    {
        return new BouncingComponent()
        {
            GravityFactor = this.GravityFactor,
            FallOffFactor = this.FallOffFactor,
            VerticalVelocity = this.VerticalVelocity,
            Velocity = this.Velocity,
            VelocityFriction = this.VelocityFriction,
            VelocityThreshold = this.VelocityThreshold
        };
    }

    public override ulong GetHash()
    {
        return Utilities.CombineHash(this.GravityFactor.Hash(), this.FallOffFactor.Hash(), this.VelocityFriction.Hash(), this.VelocityThreshold.Hash());
    }

    public override string ToString()
    {
        return $"BouncingComponent=[gravityFactor={this.GravityFactor}, fallOffFactor={this.FallOffFactor}]";
    }
}