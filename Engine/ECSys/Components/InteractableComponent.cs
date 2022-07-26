using System;
using System.Collections.Generic;
using System.Text;
using AGame.Engine.Assets.Scripting;
using AGame.Engine.Networking;
using AGame.Engine.World;

namespace AGame.Engine.ECSys.Components;

public interface IOnInteract
{
    void OnInteract(Entity playerEntity, Entity interactingWith, UserCommand command, ECS ecs);
}

[ComponentNetworking(CreateTriggersNetworkUpdate = true)]
public class InteractableComponent : Component
{
    private string _onInteract;
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

    private IOnInteract _instance;
    public IOnInteract GetOnInteract()
    {
        if (_instance == null)
        {
            _instance = (IOnInteract)ScriptingManager.CreateInstance(this.OnInteract);
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
            OnInteract = this.OnInteract
        };
    }

    public override ulong GetHash()
    {
        return Utilities.Hash(this.ToBytes());
    }

    public override void InterpolateProperties(Component from, Component to, float amt)
    {
        var toC = (InteractableComponent)to;

        this.OnInteract = toC.OnInteract;
        this._instance = null;
    }

    public override int Populate(byte[] data, int offset)
    {
        int start = offset;
        int len = BitConverter.ToInt32(data, offset);
        offset += sizeof(int);
        this.OnInteract = Encoding.UTF8.GetString(data, offset, len);
        offset += len;
        return offset - start;
    }

    public override byte[] ToBytes()
    {
        List<byte> bytes = new List<byte>();
        bytes.AddRange(BitConverter.GetBytes(this.OnInteract.Length));
        bytes.AddRange(Encoding.UTF8.GetBytes(this.OnInteract));
        return bytes.ToArray();
    }

    public override string ToString()
    {
        return $"InteractableComponent: {OnInteract}";
    }

    public override void UpdateComponent(Component newComponent)
    {
        var newIn = (InteractableComponent)newComponent;
        this.OnInteract = newIn.OnInteract;
        this._instance = null;
    }
}