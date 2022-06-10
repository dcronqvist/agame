using System.Numerics;
using GameUDPProtocol;

namespace AGame.Engine.World;

public struct ChunkAddress
{
    public int x;
    public int z;

    public ChunkAddress(int x, int z)
    {
        this.x = x;
        this.z = z;
    }

    public override int GetHashCode()
    {
        unchecked
        {
            int hash = 17;
            hash = hash * 23 + x;
            hash = hash * 23 + z;
            return hash;
        }
    }
    public override bool Equals(Object obj)
    {
        if ((obj == null) || !this.GetType().Equals(obj.GetType()))
        {
            return false;
        }
        else
        {
            ChunkAddress other = (ChunkAddress)obj;
            return (x == other.x) && (z == other.z);
        }
    }
}

public class Chunk : IPacketable
{
    public const int CHUNK_SIZE = 8;

    public int X { get; set; }
    public int Y { get; set; }

    public int[,] GroundLayer { get; set; }

    private StaticTileGrid _staticTileGrid;
    private StaticTileGrid TileGrid => _staticTileGrid ?? (_staticTileGrid = new StaticTileGrid(this.GroundLayer, new Vector2(X * CHUNK_SIZE * AGame.Engine.World.TileGrid.TILE_SIZE, Y * CHUNK_SIZE * AGame.Engine.World.TileGrid.TILE_SIZE)));

    public Chunk()
    {

    }

    public Chunk(int x, int y, int[,] groundLayer)
    {
        this.X = x;
        this.Y = y;
        this.GroundLayer = groundLayer;
    }

    public byte[] ToBytes()
    {
        List<byte> bytes = new List<byte>();

        bytes.AddRange(BitConverter.GetBytes(X));
        bytes.AddRange(BitConverter.GetBytes(Y));

        int[] groundGrid = this.GroundLayer.Cast<int>().ToArray();

        for (int i = 0; i < groundGrid.Length; i++)
        {
            bytes.AddRange(BitConverter.GetBytes(groundGrid[i]));
        }

        return bytes.ToArray();
    }

    public int Populate(byte[] data, int offset)
    {
        int startOffset = offset;
        int x = BitConverter.ToInt32(data, offset);
        int y = BitConverter.ToInt32(data, offset + 4);

        int[] groundGrid = new int[CHUNK_SIZE * CHUNK_SIZE];

        for (int i = 0; i < groundGrid.Length; i++)
        {
            groundGrid[i] = BitConverter.ToInt32(data, offset + 8 + (i * 4));
        }

        int[,] grid = new int[CHUNK_SIZE, CHUNK_SIZE];

        for (int i = 0; i < CHUNK_SIZE; i++)
        {
            for (int j = 0; j < CHUNK_SIZE; j++)
            {
                grid[i, j] = groundGrid[(i * CHUNK_SIZE) + j];
            }
        }

        this.GroundLayer = grid;

        this.X = x;
        this.Y = y;

        return offset - startOffset;
    }

    public void Render()
    {
        this.TileGrid.Render();
    }
}