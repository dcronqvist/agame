using System;
using System.Collections.Generic;
using System.Numerics;
using AGame.Engine;
using AGame.Engine.Assets.Scripting;
using AGame.Engine.ECSys;
using AGame.Engine.Networking;
using AGame.Engine.World;

namespace DefaultMod;

[ComponentNetworking(CreateTriggersNetworkUpdate = true, UpdateTriggersNetworkUpdate = true), ScriptType(Name = "render_component")]
public class RenderComponent : Component
{
    private bool _sortByY;
    [ComponentProperty(0, typeof(BoolPacker), typeof(BoolInterpolator), InterpolationType.ToInstant)]
    public bool SortByY
    {
        get => _sortByY;
        set
        {
            if (_sortByY != value)
            {
                _sortByY = value;
                this.NotifyPropertyChanged();
            }
        }
    }

    private byte _renderLayer;
    [ComponentProperty(1, typeof(BytePacker), typeof(ByteInterpolator), InterpolationType.ToInstant)]
    public byte RenderLayer
    {
        get => _renderLayer;
        set
        {
            if (_renderLayer != value)
            {
                _renderLayer = value;
                this.NotifyPropertyChanged();
            }
        }
    }

    private Vector2 _feetOffset;
    [ComponentProperty(2, typeof(Vector2Packer), typeof(Vector2Interpolator), InterpolationType.ToInstant)]
    public Vector2 FeetOffset
    {
        get => _feetOffset;
        set
        {
            if (_feetOffset != value)
            {
                _feetOffset = value;
                this.NotifyPropertyChanged();
            }
        }
    }

    public override void ApplyInput(Entity parentEntity, UserCommand command, WorldContainer world, ECS ecs)
    {

    }

    public override Component Clone()
    {
        return new RenderComponent()
        {
            SortByY = this.SortByY,
            RenderLayer = this.RenderLayer,
            FeetOffset = this.FeetOffset
        };
    }

    public override ulong GetHash()
    {
        return Utilities.CombineHash(this.SortByY.Hash(), this.RenderLayer.Hash(), this.FeetOffset.X.Hash(), this.FeetOffset.Y.Hash());
    }

    public override string ToString()
    {
        return $"SortByY: {this.SortByY}, RenderLayer: {this.RenderLayer}";
    }
}