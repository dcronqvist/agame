using System.Collections.Generic;

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
            AddTile("dirt", 1, new Tile("tex_marsdirt", false));
        }

        public static void AddTile(string tileName, int tileID, Tile tile)
        {
            Tiles.Add(tileID, tile);
            TileNamesToID.Add(tileName, tileID);
        }

        public static Tile GetTileFromID(int tileID)
        {
            return Tiles[tileID];
        }
    }
}