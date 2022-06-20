using System.Numerics;
using GameUDPProtocol;

namespace AGame.Engine.World;

public struct WorldVector
{
    public float X { get; set; }
    public float Y { get; set; }

    public WorldVector(float x, float y)
    {
        X = x;
        Y = y;
    }

    // Operators
    public static WorldVector operator +(WorldVector a, WorldVector b)
    {
        return new WorldVector(a.X + b.X, a.Y + b.Y);
    }

    public static WorldVector operator -(WorldVector a, WorldVector b)
    {
        return new WorldVector(a.X - b.X, a.Y - b.Y);
    }

    public static WorldVector operator *(WorldVector a, float b)
    {
        return new WorldVector(a.X * b, a.Y * b);
    }

    public static WorldVector operator /(WorldVector a, float b)
    {
        return new WorldVector(a.X / b, a.Y / b);
    }

    public static WorldVector operator *(float a, WorldVector b)
    {
        return new WorldVector(a * b.X, a * b.Y);
    }

    // Conversions
    public CoordinateVector ToCoordinateVector()
    {
        float fx = X > 0 ? X : (X - 1);
        float fy = Y > 0 ? Y : (Y - 1);

        float x = fx / (TileGrid.TILE_SIZE);
        float y = fy / (TileGrid.TILE_SIZE);
        return new CoordinateVector(x, y);
    }

    public ChunkAddress ToChunkAddress()
    {
        float fx = X > 0 ? X : (X - 1);
        float fy = Y > 0 ? Y : (Y - 1);

        int x = (int)MathF.Floor(fx / (Chunk.CHUNK_SIZE * TileGrid.TILE_SIZE));
        int y = (int)MathF.Floor(fy / (Chunk.CHUNK_SIZE * TileGrid.TILE_SIZE));

        return new ChunkAddress(x, y);
    }

    public Vector2 ToVector2()
    {
        return new Vector2(X, Y);
    }

    public override bool Equals(object obj)
    {
        return obj is WorldVector vector &&
               X == vector.X &&
               Y == vector.Y;
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(X, Y);
    }
}