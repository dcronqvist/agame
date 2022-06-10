using System;
using System.Numerics;
using AGame.Engine.Assets;
using AGame.Engine.Graphics;

namespace AGame.Engine.World;

public enum TileType
{
    Ground,
    Floor,
    Building,
    Air
}

public class Tile
{
    public TileType Type { get; set; }
    public bool Solid { get; set; }
    public string Texture { get; set; }
    public string Name { get; set; }

    public Tile(TileType type, bool solid, string texture, string name)
    {
        this.Type = type;
        this.Solid = solid;
        this.Texture = texture;
        this.Name = name;
    }

    public void SetTileName(string tileName)
    {
        this.Name = tileName;
    }

    public int GetID()
    {
        return TileManager.GetTileIDFromName(this.Name);
    }

    public Texture2D GetTexture() => AssetManager.GetAsset<Texture2D>(this.Texture);
}

public class GroundTile : Tile
{
    public GroundTile(string texture, string name) : base(TileType.Ground, false, texture, name)
    {

    }
}

public class FloorTile : Tile
{
    public FloorTile(string texture, string name) : base(TileType.Floor, false, texture, name)
    {

    }
}

public class BuildingTile : Tile
{
    public BuildingTile(string texture, string name, bool solid) : base(TileType.Building, solid, texture, name)
    {

    }
}

public class AirTile : Tile
{
    public AirTile(string texture, string name) : base(TileType.Air, false, texture, name)
    {

    }
}