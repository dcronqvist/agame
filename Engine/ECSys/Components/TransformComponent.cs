using System.Numerics;
using AGame.Engine.Configuration;
using AGame.Engine.Networking;
using AGame.Engine.World;

namespace AGame.Engine.ECSys.Components;

[ComponentNetworking(CNType.Update, NDirection.ServerToClient, MaxUpdatesPerSecond = 20, IsReliable = false)]
public class TransformComponent : Component
{
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
            _position = Position
        };
    }

    public override int Populate(byte[] data, int offset)
    {
        int initialOffset = offset;
        float x = BitConverter.ToSingle(data, offset);
        offset += sizeof(float);
        float y = BitConverter.ToSingle(data, offset);
        offset += sizeof(float);
        this._position = new CoordinateVector(x, y);
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

        this.Position = tc.Position;
    }

    public override void InterpolateProperties(Component from, Component to, float amt)
    {
        TransformComponent fromTC = from as TransformComponent;
        TransformComponent toTC = to as TransformComponent;

        this.Position = CoordinateVector.Lerp(fromTC.Position, toTC.Position, amt);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(this.Position);
    }

    public override void ApplyInput(UserCommand command)
    {

    }
}