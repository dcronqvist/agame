using System;
using System.Collections.Generic;
using System.Drawing;
using System.Numerics;
using AGame.Engine.Configuration;
using AGame.Engine.Networking;
using AGame.Engine.World;

namespace AGame.Engine.ECSys.Components;

[ComponentNetworking(UpdateTriggersNetworkUpdate = true, CreateTriggersNetworkUpdate = true)]
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

    private float _speed;
    public float Speed
    {
        get => _speed;
        set
        {
            if (_speed != value)
            {
                _speed = value;
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
            _position = Position,
            _velocity = Velocity,
            _targetVelocity = TargetVelocity,
            _speed = Speed
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

        this._speed = BitConverter.ToSingle(data, offset);
        offset += sizeof(float);

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
        bytes.AddRange(BitConverter.GetBytes(Speed));
        return bytes.ToArray();
    }

    public override string ToString()
    {
        return $"PlayerPosition=[x={Position.X}, y={Position.Y}, vx={Velocity.X}, vy={Velocity.Y}, tvx={TargetVelocity.X}, tvy={TargetVelocity.Y}]";
    }

    public override void UpdateComponent(Component newComponent)
    {
        TransformComponent tc = newComponent as TransformComponent;

        this.Position = tc.Position;
        this.Velocity = tc.Velocity;
        this.TargetVelocity = tc.TargetVelocity;
        this.Speed = tc.Speed;
    }

    public override void InterpolateProperties(Component from, Component to, float amt)
    {
        TransformComponent fromTC = from as TransformComponent;
        TransformComponent toTC = to as TransformComponent;

        this.Position = CoordinateVector.Lerp(fromTC.Position, toTC.Position, amt);
        this.Velocity = CoordinateVector.Lerp(fromTC.Velocity, toTC.Velocity, amt);
        this.TargetVelocity = CoordinateVector.Lerp(fromTC.TargetVelocity, toTC.TargetVelocity, amt);
        this.Speed = Utilities.Lerp(fromTC.Speed, toTC.Speed, amt);
    }

    public override ulong GetHash()
    {
        return Utilities.Hash(this.ToBytes());
    }

    public override void ApplyInput(Entity parentEntity, UserCommand command, WorldContainer world, ECS ecs)
    {
        // If input is being applied to a transform, it must be a player, assume the following components.
        // TransformComponent,
        // ColliderComponent

        float range = 2f;
        List<Entity> tileAlignedWithinRange = ecs.GetAllEntities(e => e.HasComponent<ColliderComponent>() &&
                                                                    e.HasComponent<TransformComponent>() &&
                                                                    e.GetComponent<ColliderComponent>().Solid &&
                                                                    e.ID != parentEntity.ID &&
                                                                    (e.GetComponent<TransformComponent>().Position - this.Position).Length() <= range);

        bool w = command.IsInputDown(UserCommand.KEY_W);
        bool a = command.IsInputDown(UserCommand.KEY_A);
        bool s = command.IsInputDown(UserCommand.KEY_S);
        bool d = command.IsInputDown(UserCommand.KEY_D);

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
            this.TargetVelocity = movement.Normalize() * Speed;
        }

        if ((TargetVelocity - Velocity).Length() < 0.01f)
        {
            Velocity = TargetVelocity;
        }

        this.Velocity += (this.TargetVelocity - this.Velocity) * command.DeltaTime * 10f;

        Vector2 playerWorldPos = this.Position.ToWorldVector().ToVector2();
        Vector2 playerVelocity = (this.Velocity * command.DeltaTime).ToWorldVector().ToVector2();
        var collider = parentEntity.GetComponent<ColliderComponent>();

        RectangleF GetPlayerRec(float xOff, float yOff)
        {
            return new RectangleF(collider.Box.X + xOff, collider.Box.Y + yOff, collider.Box.Width, collider.Box.Height);
        }

        foreach (Entity e in tileAlignedWithinRange)
        {
            TransformComponent tapc = e.GetComponent<TransformComponent>();
            ColliderComponent tac = e.GetComponent<ColliderComponent>();

            Vector2 worldPos = tapc.Position.ToWorldVector().ToVector2();

            RectangleF rec = tac.Box;

            if (rec.IntersectsWith(GetPlayerRec(playerVelocity.X, 0)))
            {
                // There will be a horizontal collision if move entire velocity.X

                // Start moving horizontally until there is collision

                while (!GetPlayerRec(Math.Sign(playerVelocity.X), 0).IntersectsWith(rec))
                {
                    // While there is no collision by only moving 1 pixel horizontally, move horizontally
                    playerWorldPos.X += Math.Sign(playerVelocity.X);
                }

                // Moving another would cause a collision, therefore we are at the desired position
                this.Velocity = new CoordinateVector(0, this.Velocity.Y);
                this.TargetVelocity = new CoordinateVector(0, this.TargetVelocity.Y);
            }

            if (rec.IntersectsWith(GetPlayerRec(0, playerVelocity.Y)))
            {
                // There will be a vertical collision if move entire velocity.X

                // Start moving vertically until there is collision
                while (!GetPlayerRec(0, Math.Sign(playerVelocity.Y)).IntersectsWith(rec))
                {
                    // While there is no collision by only moving 1 pixel vertically, move vertically
                    playerWorldPos.Y += Math.Sign(playerVelocity.Y);
                }

                // Moving another would cause a collision, therefore we are at the desired position
                this.Velocity = new CoordinateVector(this.Velocity.X, 0);
                this.TargetVelocity = new CoordinateVector(this.TargetVelocity.X, 0);
            }
        }

        this.Position += this.Velocity * command.DeltaTime;
    }
}