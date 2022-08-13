using System;
using System.Collections.Generic;
using System.Text;
using AGame.Engine;
using AGame.Engine.Assets.Scripting;
using AGame.Engine.ECSys;
using AGame.Engine.Networking;
using AGame.Engine.World;

namespace DefaultMod;

public interface IOnInteract
{
    void OnInteract(Entity playerEntity, Entity interactingWith, UserCommand command, ECS ecs);
}

[ComponentNetworking(CreateTriggersNetworkUpdate = true), ScriptType(Name = "interactable_component")]
public class InteractableComponent : Component
{
    private string _onInteract;
    [ComponentProperty(0, typeof(StringPacker), typeof(StringInterpolator), InterpolationType.ToInstant)]
    public string OnInteract
    {
        get => _onInteract;
        set
        {
            if (_onInteract != value && value is not null)
            {
                _onInteract = value;
                this.NotifyPropertyChanged();
            }
        }
    }

    private int _interactDistance;
    [ComponentProperty(1, typeof(IntPacker), typeof(IntInterpolator), InterpolationType.ToInstant)]
    public int InteractDistance
    {
        get => _interactDistance;
        set
        {
            if (_interactDistance != value)
            {
                _interactDistance = value;
                this.NotifyPropertyChanged();
            }
        }
    }

    private IOnInteract _instance;
    public IOnInteract GetOnInteract()
    {
        if (_instance == null)
        {
            _instance = ScriptingManager.CreateInstance<IOnInteract>(this.OnInteract);
        }
        return _instance;
    }

    public override void ApplyInput(Entity parentEntity, UserCommand command, WorldContainer world, ECS ecs)
    {

    }

    public override Component Clone()
    {
        return new InteractableComponent()
        {
            OnInteract = this.OnInteract,
            InteractDistance = this.InteractDistance
        };
    }

    public override ulong GetHash()
    {
        return Utilities.CombineHash(this.OnInteract.Hash(), this.InteractDistance.Hash());
    }

    public override string ToString()
    {
        return $"InteractableComponent: {OnInteract}";
    }
}