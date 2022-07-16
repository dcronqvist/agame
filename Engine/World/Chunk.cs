using System.Numerics;
using System.Text;
using System.Text.Json.Serialization;
using AGame.Engine.Assets;
using AGame.Engine.Graphics.Rendering;
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

public class ChunkLayer : IPacketable
{
    public int Order { get; set; }
    public int[,] Grid { get; set; }
    public string TileSet { get; set; }
    public bool UseSideTiles { get; set; }

    [PacketPropIgnore]
    [JsonIgnore]
    public WorldContainer ParentWorld { get; set; }

    public ChunkLayer()
    {

    }

    public ChunkLayer(string tileSet, int order, int[,] grid, bool useSidetiles)
    {
        this.TileSet = tileSet;
        this.Order = order;
        this.Grid = grid;
        this.UseSideTiles = useSidetiles;
    }

    public TileSet GetTileSet()
    {
        return ModManager.GetAsset<TileSet>(this.TileSet);
    }

    public byte[] ToBytes()
    {
        List<byte> bytes = new List<byte>();
        bytes.AddRange(BitConverter.GetBytes(this.Order));
        bytes.AddRange(BitConverter.GetBytes(this.UseSideTiles));
        bytes.AddRange(BitConverter.GetBytes(this.TileSet.Length));
        bytes.AddRange(Encoding.UTF8.GetBytes(this.TileSet));

        int[] groundGrid = this.Grid.Cast<int>().ToArray();

        List<byte> groundGridBytes = new List<byte>();
        byte currByte = 0;
        int currByteIndex = 0;

        for (int i = 0; i < groundGrid.Length; i++)
        {
            bool value = groundGrid[i] == 1;

            if (value)
            {
                currByte |= (byte)(1 << currByteIndex);
            }

            if (currByteIndex == 7)
            {
                groundGridBytes.Add(currByte);
                currByte = 0;
                currByteIndex = 0;
            }
            else
            {
                currByteIndex++;
            }
        }

        bytes.AddRange(groundGridBytes);

        return bytes.ToArray();
    }

    public int Populate(byte[] data, int offset)
    {
        int startOffset = offset;

        int order = BitConverter.ToInt32(data, offset);
        offset += sizeof(int);

        bool useSidetiles = BitConverter.ToBoolean(data, offset);
        offset += sizeof(bool);

        int tileSetLength = BitConverter.ToInt32(data, offset);
        offset += sizeof(int);

        string tileSet = Encoding.UTF8.GetString(data, offset, tileSetLength);
        offset += tileSetLength;

        this.Grid = new int[Chunk.CHUNK_SIZE, Chunk.CHUNK_SIZE];

        int groundGridLength = Chunk.CHUNK_SIZE * Chunk.CHUNK_SIZE / 8;
        byte[] groundGridBytes = new byte[groundGridLength];

        for (int i = 0; i < groundGridLength; i++)
        {
            groundGridBytes[i] = data[offset++];
        }

        bool[] values = new bool[Chunk.CHUNK_SIZE * Chunk.CHUNK_SIZE];

        for (int i = 0; i < groundGridLength; i++)
        {
            byte currByte = groundGridBytes[i];

            for (int j = 0; j < 8; j++)
            {
                bool value = (currByte & (1 << j)) != 0;
                values[i * 8 + j] = value;
            }
        }

        for (int i = 0; i < Chunk.CHUNK_SIZE; i++)
        {
            for (int j = 0; j < Chunk.CHUNK_SIZE; j++)
            {
                this.Grid[i, j] = values[i * Chunk.CHUNK_SIZE + j] ? 1 : 0;
            }
        }

        this.UseSideTiles = useSidetiles;
        this.Order = order;
        this.TileSet = tileSet;
        return offset - startOffset;
    }
}

public class Chunk : IPacketable
{
    public const int CHUNK_SIZE = 8;

    public int X { get; set; }
    public int Y { get; set; }
    public List<ChunkLayer> Layers { get; set; }

    [PacketPropIgnore]
    [JsonIgnore]
    public WorldContainer ParentWorld { get; set; }

    public Chunk()
    {

    }

    public Chunk(int x, int y, IEnumerable<ChunkLayer> layers)
    {
        this.X = x;
        this.Y = y;
        this.Layers = layers.OrderByDescending(x => x.Order).ToList();
    }

    public int GetTileValue(int order, int x, int y)
    {
        return this.Layers.Find(x => x.Order == order).Grid[x, y];
    }

    public byte[] ToBytes()
    {
        List<byte> bytes = new List<byte>();

        bytes.AddRange(BitConverter.GetBytes(X));
        bytes.AddRange(BitConverter.GetBytes(Y));

        bytes.AddRange(BitConverter.GetBytes(this.Layers.Count));

        foreach (ChunkLayer layer in this.Layers)
        {
            bytes.AddRange(layer.ToBytes());
        }

        return bytes.ToArray();
    }

    public int Populate(byte[] data, int offset)
    {
        int startOffset = offset;

        this.X = BitConverter.ToInt32(data, offset);
        offset += sizeof(int);
        this.Y = BitConverter.ToInt32(data, offset);
        offset += sizeof(int);

        int layerCount = BitConverter.ToInt32(data, offset);
        offset += sizeof(int);

        this.Layers = new List<ChunkLayer>();
        for (int i = 0; i < layerCount; i++)
        {
            ChunkLayer layer = new ChunkLayer();
            layer.ParentWorld = this.ParentWorld;
            offset += layer.Populate(data, offset);
            this.Layers.Add(layer);
        }

        this.Layers = this.Layers.OrderByDescending(l => l.Order).ToList();

        return offset - startOffset;
    }
}