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

    // private StaticTileGrid _staticTileGrid;
    // public StaticTileGrid GetTileGrid(int chunkX, int chunkY)
    // {
    //     if (_staticTileGrid == null)
    //     {
    //         int[,] outerGrid = new int[Chunk.CHUNK_SIZE + 2, Chunk.CHUNK_SIZE + 2];

    //         Chunk topLeft = ParentWorld.GetChunkNoGenerate(chunkX - 1, chunkY - 1);
    //         Chunk top = ParentWorld.GetChunkNoGenerate(chunkX, chunkY - 1);
    //         Chunk topRight = ParentWorld.GetChunkNoGenerate(chunkX + 1, chunkY - 1);
    //         Chunk left = ParentWorld.GetChunkNoGenerate(chunkX - 1, chunkY);
    //         Chunk right = ParentWorld.GetChunkNoGenerate(chunkX + 1, chunkY);
    //         Chunk bottomLeft = ParentWorld.GetChunkNoGenerate(chunkX - 1, chunkY + 1);
    //         Chunk bottom = ParentWorld.GetChunkNoGenerate(chunkX, chunkY + 1);
    //         Chunk bottomRight = ParentWorld.GetChunkNoGenerate(chunkX + 1, chunkY + 1);

    //         // Top left
    //         outerGrid[0, 0] = topLeft == null ? 0 : topLeft.GetTileValue(this.Order, Chunk.CHUNK_SIZE - 1, Chunk.CHUNK_SIZE - 1);
    //         // Top right
    //         outerGrid[Chunk.CHUNK_SIZE + 1, 0] = topRight == null ? 0 : topRight.GetTileValue(this.Order, 0, Chunk.CHUNK_SIZE - 1);
    //         // Along the top
    //         for (int i = 0; i < Chunk.CHUNK_SIZE; i++)
    //         {
    //             outerGrid[i + 1, 0] = top == null ? 0 : top.GetTileValue(this.Order, i, Chunk.CHUNK_SIZE - 1);
    //         }
    //         // Along the left
    //         for (int i = 0; i < Chunk.CHUNK_SIZE; i++)
    //         {
    //             outerGrid[0, i + 1] = left == null ? 0 : left.GetTileValue(this.Order, Chunk.CHUNK_SIZE - 1, i);
    //         }
    //         // Along the right
    //         for (int i = 0; i < Chunk.CHUNK_SIZE; i++)
    //         {
    //             outerGrid[Chunk.CHUNK_SIZE + 1, i + 1] = right == null ? 0 : right.GetTileValue(this.Order, 0, i);
    //         }
    //         // Bottom left
    //         outerGrid[0, Chunk.CHUNK_SIZE + 1] = bottomLeft == null ? 0 : bottomLeft.GetTileValue(this.Order, Chunk.CHUNK_SIZE - 1, 0);
    //         // Bottom right
    //         outerGrid[Chunk.CHUNK_SIZE + 1, Chunk.CHUNK_SIZE + 1] = bottomRight == null ? 0 : bottomRight.GetTileValue(this.Order, 0, 0);
    //         // Along the bottom
    //         for (int i = 0; i < Chunk.CHUNK_SIZE; i++)
    //         {
    //             outerGrid[i + 1, Chunk.CHUNK_SIZE + 1] = bottom == null ? 0 : bottom.GetTileValue(this.Order, i, 0);
    //         }

    //         _staticTileGrid = new StaticTileGrid(ModManager.GetAsset<TileSet>(TileSet), Grid, new Vector2(chunkX * Chunk.CHUNK_SIZE * TileGrid.TILE_SIZE, chunkY * Chunk.CHUNK_SIZE * TileGrid.TILE_SIZE), outerGrid);
    //     }

    //     return _staticTileGrid;
    // }

    public void ResetTileGrid()
    {
        //this._staticTileGrid = null;
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

        for (int i = 0; i < groundGrid.Length; i++)
        {
            bytes.AddRange(BitConverter.GetBytes(groundGrid[i]));
        }

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

        // int length = BitConverter.ToInt32(data, offset);
        // offset += sizeof(int);

        // List<byte> bytes = new List<byte>();
        // for (int i = 0; i < length; i++)
        // {
        //     bytes.Add(data[offset + i]);
        // }
        // offset += length;

        // byte[] decodedBuffer = Utilities.RunLengthDecode(bytes.ToArray());

        // int[] groundGrid = new int[Chunk.CHUNK_SIZE * Chunk.CHUNK_SIZE];

        // for (int i = 0; i < groundGrid.Length; i++)
        // {
        //     groundGrid[i] = BitConverter.ToInt32(decodedBuffer, offset);
        //     offset += sizeof(int);
        // }

        // int[,] grid = new int[Chunk.CHUNK_SIZE, Chunk.CHUNK_SIZE];

        // for (int i = 0; i < Chunk.CHUNK_SIZE; i++)
        // {
        //     for (int j = 0; j < Chunk.CHUNK_SIZE; j++)
        //     {
        //         grid[i, j] = groundGrid[(i * Chunk.CHUNK_SIZE) + j];
        //     }
        // }

        this.Grid = new int[Chunk.CHUNK_SIZE, Chunk.CHUNK_SIZE];

        for (int i = 0; i < Chunk.CHUNK_SIZE; i++)
        {
            for (int j = 0; j < Chunk.CHUNK_SIZE; j++)
            {
                this.Grid[i, j] = BitConverter.ToInt32(data, offset);
                offset += sizeof(int);
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