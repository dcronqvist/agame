using System.Numerics;
using AGame.Engine.Networking;
using AGame.Engine.World;

namespace AGame.Engine.ECSys.Components;

[ComponentNetworking(CNType.Snapshot, NDirection.ServerToClient)]
public class TransformComponent : Component
{
    public CoordinateVector _targetPosition;
    private CoordinateVector _position;
    public CoordinateVector Position
    {
        get => _position;
        set
        {
            if (!_position.Equals(value))
            {
                _position = value;
                this.NotifyPropertyChanged();
            }
        }
    }

    public TransformComponent()
    {

    }

    public override Component Clone()
    {
        return new TransformComponent()
        {
            Position = Position,
            _targetPosition = Position,
        };
    }

    public override int Populate(byte[] data, int offset)
    {
        int initialOffset = offset;
        float x = BitConverter.ToSingle(data, offset);
        offset += sizeof(float);
        float y = BitConverter.ToSingle(data, offset);
        offset += sizeof(float);
        this.Position = new CoordinateVector(x, y);
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
        if ((_targetPosition - Position).Length() < 0.01f) return;

        this.Position += (_targetPosition - Position) * GameTime.DeltaTime * 9f;
    }
}