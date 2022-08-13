using System;
using System.Collections.Generic;
using System.Drawing;
using System.Numerics;
using System.Text;
using AGame.Engine;
using AGame.Engine.Assets;
using AGame.Engine.Assets.Scripting;
using AGame.Engine.ECSys;
using AGame.Engine.Graphics;
using AGame.Engine.Networking;
using AGame.Engine.World;
using GameUDPProtocol;

namespace DefaultMod;

[ComponentNetworking(CreateTriggersNetworkUpdate = true, UpdateTriggersNetworkUpdate = false), ScriptType(Name = "animator_component")]
public class AnimatorComponent : Component
{
    private string _animator;
    [ComponentProperty(0, typeof(StringPacker), typeof(StringInterpolator), InterpolationType.ToInstant)]
    public string Animator
    {
        get => _animator;
        set
        {
            if (_animator != value)
            {
                _animator = value;
                this.NotifyPropertyChanged();
            }
        }
    }

    private Animator _animatorInstance;
    public Animator GetAnimator()
    {
        if (_animatorInstance == null)
        {
            this._animatorInstance = ModManager.GetAsset<AnimatorDescription>(_animator).GetAnimator();
        }

        return _animatorInstance;
    }

    public override void ApplyInput(Entity parentEntity, UserCommand command, WorldContainer world, ECS ecs)
    {

    }

    public override Component Clone()
    {
        return new AnimatorComponent()
        {
            Animator = this.Animator
        };
    }

    public override ulong GetHash()
    {
        return this.Animator.Hash();
    }

    public override string ToString()
    {
        return "PlayerAnimationsComponent";
    }
}