using System.Collections.Generic;
using System.Numerics;
using AGame.Engine.Assets;
using System.Linq;

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
            // Get all tiles from the assetmanager
            TileDescription[] descriptions = AssetManager.GetAssetsOfType<TileDescription>().ToArray();


            foreach (TileDescription td in descriptions)
            {
                AddTile(td.TileName, Tiles.Count, td.GetAsTile());
            }
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

        public static int GetTileIDFromName(string tileName)
        {
            return TileNamesToID[tileName];
        }

        public static string GetTileNameFromID(int id)
        {
            return Tiles[id].Name;
        }
    }
}