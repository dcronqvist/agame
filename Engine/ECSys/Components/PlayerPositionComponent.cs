using System.Numerics;
using AGame.Engine.Configuration;
using AGame.Engine.Networking;
using AGame.Engine.World;

namespace AGame.Engine.ECSys.Components;

[ComponentNetworking(UpdateTriggersNetworkUpdate = true, CreateTriggersNetworkUpdate = true)]
public class PlayerPositionComponent : Component
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

    private CoordinateVector _velocity;
    public CoordinateVector Velocity
    {
        get => _velocity;
        set
        {
            if (!_velocity.Equals(value))
            {
                _velocity = value;
                this.NotifyPropertyChanged();
            }
        }
    }

    private CoordinateVector _targetVelocity;
    public CoordinateVector TargetVelocity
    {
        get => _targetVelocity;
        set
        {
            if (!_targetVelocity.Equals(value))
            {
                _targetVelocity = value;
                this.NotifyPropertyChanged();
            }
        }
    }

    public PlayerPositionComponent()
    {

    }

    public override Component Clone()
    {
        return new PlayerPositionComponent()
        {
            _position = Position,
            _velocity = Velocity,
            _targetVelocity = TargetVelocity
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

        float vx = BitConverter.ToSingle(data, offset);
        offset += sizeof(float);
        float vy = BitConverter.ToSingle(data, offset);
        offset += sizeof(float);
        this._velocity = new CoordinateVector(vx, vy);

        float tvx = BitConverter.ToSingle(data, offset);
        offset += sizeof(float);
        float tvy = BitConverter.ToSingle(data, offset);
        offset += sizeof(float);
        this._targetVelocity = new CoordinateVector(tvx, tvy);

        return offset - initialOffset;
    }

    public override byte[] ToBytes()
    {
        List<byte> bytes = new List<byte>();
        bytes.AddRange(BitConverter.GetBytes(Position.X));
        bytes.AddRange(BitConverter.GetBytes(Position.Y));
        bytes.AddRange(BitConverter.GetBytes(Velocity.X));
        bytes.AddRange(BitConverter.GetBytes(Velocity.Y));
        bytes.AddRange(BitConverter.GetBytes(TargetVelocity.X));
        bytes.AddRange(BitConverter.GetBytes(TargetVelocity.Y));
        return bytes.ToArray();
    }

    public override string ToString()
    {
        return $"PlayerPosition=[x={Position.X}, y={Position.Y}, vx={Velocity.X}, vy={Velocity.Y}, tvx={TargetVelocity.X}, tvy={TargetVelocity.Y}]";
    }

    public override void UpdateComponent(Component newComponent)
    {
        PlayerPositionComponent tc = newComponent as PlayerPositionComponent;

        this.Position = tc.Position;
        this.Velocity = tc.Velocity;
        this.TargetVelocity = tc.TargetVelocity;
    }

    public override void InterpolateProperties(Component from, Component to, float amt)
    {
        PlayerPositionComponent fromTC = from as PlayerPositionComponent;
        PlayerPositionComponent toTC = to as PlayerPositionComponent;

        this.Position = CoordinateVector.Lerp(fromTC.Position, toTC.Position, amt);
        this.Velocity = CoordinateVector.Lerp(fromTC.Velocity, toTC.Velocity, amt);
        this.TargetVelocity = CoordinateVector.Lerp(fromTC.TargetVelocity, toTC.TargetVelocity, amt);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(this.Position);
    }

    public override void ApplyInput(UserCommand command, WorldContainer world)
    {
        bool w = command.IsKeyDown(UserCommand.KEY_W);
        bool a = command.IsKeyDown(UserCommand.KEY_A);
        bool s = command.IsKeyDown(UserCommand.KEY_S);
        bool d = command.IsKeyDown(UserCommand.KEY_D);

        CoordinateVector movement = new CoordinateVector(0, 0);

        if (w) movement.Y -= 1;
        if (a) movement.X -= 1;
        if (s) movement.Y += 1;
        if (d) movement.X += 1;

        if (movement.X == 0 && movement.Y == 0)
        {
            this.TargetVelocity = CoordinateVector.Zero;
        }
        else
        {
            this.TargetVelocity = movement.Normalize() * 10f;
        }

        if ((TargetVelocity - Velocity).Length() < 0.01f)
        {
            Velocity = TargetVelocity;
        }

        this.Velocity += (this.TargetVelocity - this.Velocity) * command.DeltaTime * 7f;
        this.Position += this.Velocity * command.DeltaTime;
    }
}