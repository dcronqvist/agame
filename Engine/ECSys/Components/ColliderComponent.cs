using System;
using System.Collections.Generic;
using System.Drawing;
using System.Numerics;
using AGame.Engine.Networking;
using AGame.Engine.World;

namespace AGame.Engine.ECSys.Components;

[ComponentNetworking(CreateTriggersNetworkUpdate = true, UpdateTriggersNetworkUpdate = true)]
public class ColliderComponent : Component
{
    private RectangleF _box;
    public RectangleF Box
    {
        get => _box;
        set
        {
            if (_box != value)
            {
                _box = value;
                this.NotifyPropertyChanged();
            }
        }
    }

    private Vector2 _transformOffset;
    public Vector2 TransformOffset
    {
        get => _transformOffset;
        set
        {
            if (_transformOffset != value)
            {
                _transformOffset = value;
                this.NotifyPropertyChanged();
            }
        }
    }

    private bool _solid;
    public bool Solid
    {
        get => _solid;
        set
        {
            if (_solid != value)
            {
                _solid = value;
                this.NotifyPropertyChanged();
            }
        }
    }

    public override void ApplyInput(Entity parentEntity, UserCommand command, WorldContainer world, ECS ecs)
    {
        // Assume that this is a player, so it should have a transform component.
        var transform = parentEntity.GetComponent<TransformComponent>();
        WorldVector playerWorld = transform.Position.ToWorldVector();
        this.UpdateBox(playerWorld);
    }

    public override Component Clone()
    {
        return new ColliderComponent()
        {
            Box = this.Box,
            TransformOffset = this.TransformOffset,
            Solid = this.Solid
        };
    }

    public override ulong GetHash()
    {
        return Utilities.Hash(this.ToBytes());
    }

    public override void InterpolateProperties(Component from, Component to, float amt)
    {
        var fromCollider = (ColliderComponent)from;
        var toCollider = (ColliderComponent)to;
        this.Box = fromCollider.Box.Lerp(toCollider.Box, amt);
        this.TransformOffset = Vector2.Lerp(fromCollider.TransformOffset, toCollider.TransformOffset, amt);
        this.Solid = toCollider.Solid;
    }

    public override int Populate(byte[] data, int offset)
    {
        int startOffset = offset;
        this.Box = new RectangleF(
            BitConverter.ToSingle(data, offset),
            BitConverter.ToSingle(data, offset + 4),
            BitConverter.ToSingle(data, offset + 8),
            BitConverter.ToSingle(data, offset + 12)
        );
        offset += 16;
        this.TransformOffset = new Vector2(
            BitConverter.ToSingle(data, offset),
            BitConverter.ToSingle(data, offset + 4)
        );
        offset += 8;
        this.Solid = BitConverter.ToBoolean(data, offset);
        offset += sizeof(bool);
        return offset - startOffset;
    }

    public override byte[] ToBytes()
    {
        List<byte> bytes = new List<byte>();
        bytes.AddRange(BitConverter.GetBytes(this.Box.X));
        bytes.AddRange(BitConverter.GetBytes(this.Box.Y));
        bytes.AddRange(BitConverter.GetBytes(this.Box.Width));
        bytes.AddRange(BitConverter.GetBytes(this.Box.Height));
        bytes.AddRange(BitConverter.GetBytes(this.TransformOffset.X));
        bytes.AddRange(BitConverter.GetBytes(this.TransformOffset.Y));
        bytes.AddRange(BitConverter.GetBytes(this.Solid));
        return bytes.ToArray();
    }

    public override string ToString()
    {
        return $"ColliderComponent=[solid={this.Solid}, x={this.Box.X}, y={this.Box.Y}, w={this.Box.Width}, h={this.Box.Height}, offsetX={this.TransformOffset.X}, offsetY={this.TransformOffset.Y}]";
    }

    public override void UpdateComponent(Component newComponent)
    {
        var newCollider = (ColliderComponent)newComponent;
        this.Box = newCollider.Box;
        this.TransformOffset = newCollider.TransformOffset;
        this.Solid = newCollider.Solid;
    }

    public void UpdateBox(WorldVector position)
    {
        this.Box = new RectangleF(
            position.X + this.TransformOffset.X,
            position.Y + this.TransformOffset.Y,
            this.Box.Width,
            this.Box.Height
        );
    }
}