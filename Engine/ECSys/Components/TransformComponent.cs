using System.Numerics;
using AGame.Engine.Networking;

namespace AGame.Engine.ECSys.Components;

public class TransformComponent : Component
{
    public InterpolatedVector2 Position { get; set; }

    public TransformComponent()
    {

    }

    public override Component Clone()
    {
        return new TransformComponent()
        {
            Position = new InterpolatedVector2(this.Position.TargetValue, this.Position.InterpolationFactor)
        };
    }

    public override int Populate(byte[] data, int offset)
    {
        int initialOffset = offset;
        this.Position = new InterpolatedVector2(Vector2.Zero, 0f);
        offset = this.Position.Populate(data, offset);
        return offset - initialOffset;
    }

    public override byte[] ToBytes()
    {
        List<byte> bytes = new List<byte>();
        bytes.AddRange(this.Position.ToBytes());
        return bytes.ToArray();
    }

    public override string ToString()
    {
        return $"Position=[x={this.Position.CurrentValue.X}, y={this.Position.CurrentValue.Y}, if={this.Position.InterpolationFactor}]";
    }

    public override void UpdateComponent(Component newComponent)
    {
        TransformComponent tc = newComponent as TransformComponent;

        this.Position.TargetValue = tc.Position.TargetValue;
    }
}