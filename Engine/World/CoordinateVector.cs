using System.Numerics;
using GameUDPProtocol;

namespace AGame.Engine.World;

public struct CoordinateVector
{
    public static readonly CoordinateVector Zero = new CoordinateVector(0, 0);

    public float X { get; set; }
    public float Y { get; set; }

    public CoordinateVector(float x, float y)
    {
        X = x;
        Y = y;
    }

    // Operators
    public static CoordinateVector operator +(CoordinateVector a, CoordinateVector b)
    {
        return new CoordinateVector(a.X + b.X, a.Y + b.Y);
    }

    public static CoordinateVector operator -(CoordinateVector a, CoordinateVector b)
    {
        return new CoordinateVector(a.X - b.X, a.Y - b.Y);
    }

    public static CoordinateVector operator *(CoordinateVector a, float b)
    {
        return new CoordinateVector(a.X * b, a.Y * b);
    }

    public static CoordinateVector operator /(CoordinateVector a, float b)
    {
        return new CoordinateVector(a.X / b, a.Y / b);
    }

    public static CoordinateVector operator *(float a, CoordinateVector b)
    {
        return new CoordinateVector(a * b.X, a * b.Y);
    }

    public static CoordinateVector operator +(CoordinateVector a, Vector2 b)
    {
        return new CoordinateVector(a.X + b.X, a.Y + b.Y);
    }

    public static CoordinateVector operator -(CoordinateVector a, Vector2 b)
    {
        return new CoordinateVector(a.X - b.X, a.Y - b.Y);
    }

    public static CoordinateVector operator +(Vector2 a, CoordinateVector b)
    {
        return new CoordinateVector(a.X + b.X, a.Y + b.Y);
    }

    public static CoordinateVector operator -(Vector2 a, CoordinateVector b)
    {
        return new CoordinateVector(a.X - b.X, a.Y - b.Y);
    }

    public Vector2i ToTileAligned()
    {
        float fx = X > 0 ? X : (X - 1);
        float fy = Y > 0 ? Y : (Y - 1);

        return new Vector2i((int)fx, (int)fy);
    }

    public WorldVector ToWorldVector()
    {
        return new WorldVector(X, Y) * TileGrid.TILE_SIZE;
    }

    public ChunkAddress ToChunkAddress()
    {
        return this.ToWorldVector().ToChunkAddress();
    }

    public override bool Equals(object obj)
    {
        return obj is CoordinateVector vector &&
               X == vector.X &&
               Y == vector.Y;
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(X, Y);
    }

    // Linear algebra functions
    public float Length()
    {
        return (float)Math.Sqrt(X * X + Y * Y);
    }

    public CoordinateVector Normalize()
    {
        float length = Length();
        return new CoordinateVector(X / length, Y / length);
    }

    public CoordinateVector MoveToward(CoordinateVector target, float distance)
    {
        CoordinateVector direction = target - this;
        direction = direction.Normalize();
        return this + direction * distance;
    }
}