using System.Drawing;
using System.Numerics;
using System.Text;
using AGame.Engine.Assets;
using AGame.Engine.Graphics;
using AGame.Engine.Networking;
using AGame.Engine.World;
using GameUDPProtocol;

namespace AGame.Engine.ECSys.Components;

[ComponentNetworking(CreateTriggersNetworkUpdate = true, UpdateTriggersNetworkUpdate = false)]
public class AnimatorComponent : Component
{
    private string _animator;
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

    public override int GetHashCode()
    {
        return this.Animator.GetHashCode();
    }

    public override void InterpolateProperties(Component from, Component to, float amt)
    {

    }

    public override int Populate(byte[] data, int offset)
    {
        int startOffset = offset;
        int len = BitConverter.ToInt32(data, offset);
        offset += sizeof(int);
        this.Animator = Encoding.UTF8.GetString(data, offset, len);
        offset += len;
        return offset - startOffset;
    }

    public override byte[] ToBytes()
    {
        List<byte> bytes = new List<byte>();
        bytes.AddRange(BitConverter.GetBytes(this.Animator.Length));
        bytes.AddRange(Encoding.UTF8.GetBytes(this.Animator));
        return bytes.ToArray();
    }

    public override string ToString()
    {
        return "PlayerAnimationsComponent";
    }

    public override void UpdateComponent(Component newComponent)
    {
        AnimatorComponent newPlayerAnimationsComponent = (AnimatorComponent)newComponent;
        this.Animator = newPlayerAnimationsComponent.Animator;
        this._animatorInstance = null;
    }
}