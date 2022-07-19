using AGame.Engine.Networking;
using AGame.Engine.World;

namespace AGame.Engine.ECSys.Components;

[ComponentNetworking(CreateTriggersNetworkUpdate = true, UpdateTriggersNetworkUpdate = true)]
public class RenderComponent : Component
{
    private bool _sortByY;
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

    public override void ApplyInput(Entity parentEntity, UserCommand command, WorldContainer world, ECS ecs)
    {

    }

    public override Component Clone()
    {
        return new RenderComponent()
        {
            SortByY = this.SortByY,
            RenderLayer = this.RenderLayer
        };
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(this.SortByY, this.RenderLayer);
    }

    public override void InterpolateProperties(Component from, Component to, float amt)
    {
        var toC = (RenderComponent)to;
        this.SortByY = toC.SortByY;
        this.RenderLayer = toC.RenderLayer;
    }

    public override int Populate(byte[] data, int offset)
    {
        int start = offset;
        this.SortByY = BitConverter.ToBoolean(data, offset);
        offset += sizeof(bool);
        this.RenderLayer = data[offset];
        offset += sizeof(byte);
        return offset - start;
    }

    public override byte[] ToBytes()
    {
        List<byte> bytes = new List<byte>();
        bytes.AddRange(BitConverter.GetBytes(this.SortByY));
        bytes.Add(this.RenderLayer);
        return bytes.ToArray();
    }

    public override string ToString()
    {
        return $"SortByY: {this.SortByY}, RenderLayer: {this.RenderLayer}";
    }

    public override void UpdateComponent(Component newComponent)
    {
        var newC = (RenderComponent)newComponent;
        this.SortByY = newC.SortByY;
        this.RenderLayer = newC.RenderLayer;
    }
}