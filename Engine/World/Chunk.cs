using System.Numerics;
using System.Text.Json.Serialization;
using GameUDPProtocol;

namespace AGame.Engine.World;

public struct ChunkAddress
{
    public int X;
    public int Y;

    public ChunkAddress(int x, int y)
    {
        this.X = x;
        this.Y = y;
    }

    public override bool Equals(object obj)
    {
        return obj is ChunkAddress address &&
               X == address.X &&
               Y == address.Y;
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(this.X, this.Y);
    }
}

public class Chunk : IPacketable
{
    public const int CHUNK_SIZE = 8;

    public int X { get; set; }
    public int Y { get; set; }

    [JsonIgnore]
    public int[,] GroundLayer { get; set; }

    public int[] GroundLayerData
    {
        get
        {
            int[] data = new int[CHUNK_SIZE * CHUNK_SIZE];
            for (int i = 0; i < CHUNK_SIZE; i++)
            {
                for (int j = 0; j < CHUNK_SIZE; j++)
                {
                    data[i * CHUNK_SIZE + j] = GroundLayer[i, j];
                }
            }
            return data;
        }

        set
        {
            GroundLayer = new int[CHUNK_SIZE, CHUNK_SIZE];
            for (int i = 0; i < CHUNK_SIZE; i++)
            {
                for (int j = 0; j < CHUNK_SIZE; j++)
                {
                    GroundLayer[i, j] = value[i * CHUNK_SIZE + j];
                }
            }
        }
    }

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

        byte[] encodedBuffer = Utilities.RunLengthEncode(bytes.ToArray());
        bytes = new List<byte>();

        bytes.AddRange(BitConverter.GetBytes(encodedBuffer.Length));
        bytes.AddRange(encodedBuffer);

        return bytes.ToArray();
    }

    public int Populate(byte[] data, int offset)
    {
        int startOffset = offset;

        int length = BitConverter.ToInt32(data, offset);
        offset += sizeof(int);
        List<byte> bytes = new List<byte>();

        for (int i = 0; i < length; i++)
        {
            bytes.Add(data[offset + i]);
        }

        byte[] decodedBuffer = Utilities.RunLengthDecode(bytes.ToArray());

        int x = BitConverter.ToInt32(decodedBuffer, 0);
        int y = BitConverter.ToInt32(decodedBuffer, 4);

        int[] groundGrid = new int[CHUNK_SIZE * CHUNK_SIZE];

        for (int i = 0; i < groundGrid.Length; i++)
        {
            groundGrid[i] = BitConverter.ToInt32(decodedBuffer, 8 + (i * 4));
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

        return length + sizeof(int);
    }

    public void Render()
    {
        this.TileGrid.Render();
    }
}