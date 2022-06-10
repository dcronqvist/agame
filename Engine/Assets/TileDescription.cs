using System.Numerics;
using AGame.Engine.ECSys;
using AGame.Engine.World;

namespace AGame.Engine.Assets;

public abstract class TileDescription : Asset
{
    public TileType TileType { get; set; }
    public string TileName { get; set; }
    public string Texture { get; set; }
    public bool Solid { get; set; }

    public override bool InitOpenGL()
    {
        // Do nothing
        return true;
    }

    public abstract Tile GetAsTile();
}

public class GroundTileDescription : TileDescription
{
    public override Tile GetAsTile()
    {
        return new GroundTile(this.Texture, this.TileName);
    }
}

public class FloorTileDescription : TileDescription
{
    public override Tile GetAsTile()
    {
        return new FloorTile(this.Texture, this.TileName);
    }
}

public class BuildingTileDescription : TileDescription
{
    public override Tile GetAsTile()
    {
        return new BuildingTile(this.Texture, this.TileName, this.Solid);
    }
}

public class AirTileDescription : TileDescription
{
    public override Tile GetAsTile()
    {
        return new AirTile(this.Texture, this.TileName);
    }
}