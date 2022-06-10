using System.Numerics;
using AGame.Engine.ECSys;
using AGame.Engine.World;

namespace AGame.Engine.Assets;

public class TileDescription : Asset
{
    public string TileName { get; set; }
    public string Texture { get; set; }
    public bool Solid { get; set; }
    public Vector2 TopLeftInTexture { get; set; }
    public int WidthInTiles { get; set; }
    public int HeightInTiles { get; set; }

    public override bool InitOpenGL()
    {
        // Do nothing
        return true;
    }

    public Tile GetAsTile()
    {
        return new Tile(Texture, Solid, TopLeftInTexture, WidthInTiles, HeightInTiles);
    }
}