using System.Numerics;
using AGame.Engine.Networking;

namespace AGame.Engine.ECSys.Components;

[NetworkingBehaviour(NBType.Snapshot)]
public class TransformComponent : Component
{
    private Vector2 _targetPosition;
    public Vector2 Position { get; set; }

    public TransformComponent()
    {

    }

    public override Component Clone()
    {
        return new TransformComponent()
        {
            Position = Position,
            _targetPosition = _targetPosition
        };
    }

    public override int Populate(byte[] data, int offset)
    {
        int initialOffset = offset;
        Position = new Vector2(BitConverter.ToSingle(data, offset), BitConverter.ToSingle(data, offset + 4));
        offset += 8;
        return offset - initialOffset;
    }

    public override byte[] ToBytes()
    {
        List<byte> bytes = new List<byte>();
        bytes.AddRange(BitConverter.GetBytes(Position.X));
        bytes.AddRange(BitConverter.GetBytes(Position.Y));
        return bytes.ToArray();
    }

    public override string ToString()
    {
        return $"Position=[x={Position.X}, y={Position.Y}]";
    }

    public override void UpdateComponent(Component newComponent)
    {
        TransformComponent tc = newComponent as TransformComponent;

        this._targetPosition = tc.Position;
    }

    public override void InterpolateProperties()
    {
        if ((_targetPosition - Position).AbsLength() < 0.05f) return;

        this.Position += (_targetPosition - Position) * 15f * GameTime.DeltaTime;
    }
}