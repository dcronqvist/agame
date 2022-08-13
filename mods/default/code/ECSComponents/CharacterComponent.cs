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

[ComponentNetworking(CreateTriggersNetworkUpdate = true, UpdateTriggersNetworkUpdate = true), ScriptType(Name = "character_component")]
public class CharacterComponent : Component
{
    private string _name;
    [ComponentProperty(0, typeof(StringPacker), typeof(StringInterpolator), InterpolationType.ToInstant)]
    public string Name
    {
        get => _name;
        set
        {
            if (_name != value)
            {
                _name = value;
                this.NotifyPropertyChanged();
            }
        }
    }

    private Vector2 _nameRenderOffset;
    [ComponentProperty(1, typeof(Vector2Packer), typeof(Vector2Interpolator), InterpolationType.Linear)]
    public Vector2 NameRenderOffset
    {
        get => _nameRenderOffset;
        set
        {
            if (_nameRenderOffset != value)
            {
                _nameRenderOffset = value;
                this.NotifyPropertyChanged();
            }
        }
    }

    public override void ApplyInput(Entity parentEntity, UserCommand command, WorldContainer world, ECS ecs)
    {

    }

    public override Component Clone()
    {
        return new CharacterComponent()
        {
            Name = this.Name,
            NameRenderOffset = this.NameRenderOffset
        };
    }

    public override ulong GetHash()
    {
        return Utilities.CombineHash(this.Name.Hash(), this.NameRenderOffset.X.Hash(), this.NameRenderOffset.Y.Hash());
    }

    public override string ToString()
    {
        return $"CharacterComponent: {this.Name}";
    }
}