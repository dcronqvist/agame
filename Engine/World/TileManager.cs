using System.Collections.Generic;
using System.Numerics;

namespace AGame.Engine.World
{
    static class TileManager
    {
        public static Dictionary<int, Tile> Tiles { get; set; }
        public static Dictionary<string, int> TileNamesToID { get; set; }

        static TileManager()
        {
            Tiles = new Dictionary<int, Tile>();
            TileNamesToID = new Dictionary<string, int>();
        }

        public static void Init()
        {
            AddTile("game:dirt", 1, new Tile("tex_marsdirt", false, Vector2.Zero, 1, 1));
            AddTile("game:grass", 2, new Tile("tex_grass_base", true, Vector2.Zero, 1, 1));
            AddTile("game:dirtlight", 3, new Tile("tex_marsdirt-lighter", true, Vector2.Zero, 1, 1));
            AddTile("game:chest", 4, new Tile("tex_chest", true, new Vector2(0, 9), 2, 2));
            AddTile("game:chestbig", 5, new Tile("tex_chestbig", true, new Vector2(0, 0), 2, 1));
        }

        public static void AddTile(string tileName, int tileID, Tile tile)
        {
            Tiles.Add(tileID, tile);
            tile.SetTileName(tileName);
            TileNamesToID.Add(tileName, tileID);
        }

        public static Tile GetTileFromID(int tileID)
        {
            return Tiles[tileID];
        }
    }
}