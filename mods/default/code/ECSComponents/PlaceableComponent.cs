using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;
using AGame.Engine;
using AGame.Engine.Assets.Scripting;
using AGame.Engine.ECSys;
using AGame.Engine.Networking;
using AGame.Engine.World;

namespace DefaultMod;

[ComponentNetworking(CreateTriggersNetworkUpdate = true, UpdateTriggersNetworkUpdate = true), ScriptType(Name = "placeable_component")]
public class PlaceableComponent : Component
{
    private Vector2 _placeOffset;
    [ComponentProperty(0, typeof(Vector2Packer), typeof(Vector2Interpolator), InterpolationType.Linear)]
    public Vector2 PlaceOffset
    {
        get => _placeOffset;
        set
        {
            if (_placeOffset != value)
            {
                _placeOffset = value;
                this.NotifyPropertyChanged();
            }
        }
    }

    public override void ApplyInput(Entity parentEntity, UserCommand command, WorldContainer world, ECS ecs)
    {

    }

    public override Component Clone()
    {
        return new PlaceableComponent()
        {
            PlaceOffset = this.PlaceOffset
        };
    }

    public override ulong GetHash()
    {
        return Utilities.CombineHash(this.PlaceOffset.X.Hash(), this.PlaceOffset.Y.Hash());
    }

    public override string ToString()
    {
        return $"PlaceableComponent: {this.PlaceOffset}";
    }
}