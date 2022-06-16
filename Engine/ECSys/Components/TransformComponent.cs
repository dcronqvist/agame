using System.Numerics;
using AGame.Engine.Networking;
using AGame.Engine.World;

namespace AGame.Engine.ECSys.Components;

[ComponentNetworking(CNType.Snapshot, NDirection.ServerToClient)]
public class TransformComponent : Component
{
    private Vector2 _targetPosition;
    private Vector2 _position;
    public Vector2 Position
    {
        get => _position;
        set
        {
            if (_position != value)
            {
                _position = value;
                this.NotifyPropertyChanged();
            }
        }
    }

    public TransformComponent()
    {

    }

    public Vector2i GetTilePosition()
    {
        float fx = Position.X > 0 ? Position.X : (Position.X - 1);
        float fy = Position.Y > 0 ? Position.Y : (Position.Y - 1);

        int x = (int)MathF.Floor(fx / (TileGrid.TILE_SIZE));
        int y = (int)MathF.Floor(fy / (TileGrid.TILE_SIZE));
        return new Vector2i(x, y);
    }

    public Vector2i GetChunkPosition()
    {
        float fx = Position.X > 0 ? Position.X : (Position.X - 1);
        float fy = Position.Y > 0 ? Position.Y : (Position.Y - 1);

        int x = (int)MathF.Floor(fx / (Chunk.CHUNK_SIZE * TileGrid.TILE_SIZE));
        int y = (int)MathF.Floor(fy / (Chunk.CHUNK_SIZE * TileGrid.TILE_SIZE));

        return new Vector2i(x, y);
    }

    public override Component Clone()
    {
        return new TransformComponent()
        {
            Position = Position,
            _targetPosition = _targetPosition,
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

        this.Position += (_targetPosition - Position) * GameTime.DeltaTime * 10f;
    }
}