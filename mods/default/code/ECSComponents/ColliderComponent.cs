using System;
using System.Collections.Generic;
using System.Drawing;
using System.Numerics;
using AGame.Engine;
using AGame.Engine.Assets.Scripting;
using AGame.Engine.ECSys;
using AGame.Engine.Networking;
using AGame.Engine.World;

namespace DefaultMod;

[ComponentNetworking(CreateTriggersNetworkUpdate = true, UpdateTriggersNetworkUpdate = true), ScriptType(Name = "collider_component")]
public class ColliderComponent : Component
{
    private RectangleF _box;
    [ComponentProperty(0, typeof(RectangleFPacker), typeof(RectangleFInterpolator), InterpolationType.Linear)]
    public RectangleF Box
    {
        get => _box;
        set
        {
            if (_box != value)
            {
                _box = value;
                this.NotifyPropertyChanged();
            }
        }
    }

    private Vector2 _transformOffset;
    [ComponentProperty(1, typeof(Vector2Packer), typeof(Vector2Interpolator), InterpolationType.Linear)]
    public Vector2 TransformOffset
    {
        get => _transformOffset;
        set
        {
            if (_transformOffset != value)
            {
                _transformOffset = value;
                this.NotifyPropertyChanged();
            }
        }
    }

    private bool _solid;
    [ComponentProperty(2, typeof(BoolPacker), typeof(BoolInterpolator), InterpolationType.ToInstant)]
    public bool Solid
    {
        get => _solid;
        set
        {
            if (_solid != value)
            {
                _solid = value;
                this.NotifyPropertyChanged();
            }
        }
    }

    public override void ApplyInput(Entity parentEntity, UserCommand command, WorldContainer world, ECS ecs)
    {
        // Assume that this is a player, so it should have a transform component.
        var transform = parentEntity.GetComponent<TransformComponent>();
        WorldVector playerWorld = transform.Position.ToWorldVector();
        this.UpdateBox(playerWorld);
    }

    public override Component Clone()
    {
        return new ColliderComponent()
        {
            Box = this.Box,
            TransformOffset = this.TransformOffset,
            Solid = this.Solid
        };
    }

    public override ulong GetHash()
    {
        return Utilities.CombineHash(this.Solid.Hash(), this.TransformOffset.X.Hash(), this.TransformOffset.Y.Hash(), this.Box.X.Hash(), this.Box.Y.Hash(), this.Box.Width.Hash(), this.Box.Height.Hash());
    }

    public override string ToString()
    {
        return $"ColliderComponent=[solid={this.Solid}, x={this.Box.X}, y={this.Box.Y}, w={this.Box.Width}, h={this.Box.Height}, offsetX={this.TransformOffset.X}, offsetY={this.TransformOffset.Y}]";
    }

    public void UpdateBox(WorldVector position)
    {
        this.Box = new RectangleF(
            position.X + this.TransformOffset.X,
            position.Y + this.TransformOffset.Y,
            this.Box.Width,
            this.Box.Height
        );
    }
}