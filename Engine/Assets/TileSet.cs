using System.Drawing;

namespace AGame.Engine.Assets;

public class TileSet : Asset
{
    private Texture2D _texture;
    public string Texture { get; set; }
    public int TileSize { get; set; }

    public int FullTileVariants { get; set; }
    public int SideTileVariants { get; set; }
    public int OuterCornerVariants { get; set; }
    public int InnerCornerVariants { get; set; }

    public override bool InitOpenGL()
    {
        // Don't need to do anything.
        return true;
    }

    public Texture2D GetTexture()
    {
        if (_texture == null)
        {
            _texture = ModManager.GetAsset<Texture2D>(Texture);
        }
        return _texture;
    }

    public int GetRandomVariant(int max)
    {
        return Utilities.GetRandomInt(0, max);
    }

    public RectangleF GetTileRectangle(int x, int y)
    {
        return new RectangleF(x * this.TileSize, y * this.TileSize, this.TileSize, this.TileSize);
    }

    public RectangleF GetFullTile(int variant)
    {
        return this.GetTileRectangle(0, variant);
    }

    public RectangleF GetRandomFullTile()
    {
        return this.GetFullTile(this.GetRandomVariant(this.FullTileVariants));
    }

    public RectangleF GetSideTile(int variant)
    {
        return this.GetTileRectangle(1, variant);
    }

    public RectangleF GetRandomSideTile()
    {
        return this.GetSideTile(this.GetRandomVariant(this.SideTileVariants));
    }

    public RectangleF GetOuterCornerTile(int variant)
    {
        return this.GetTileRectangle(2, variant);
    }

    public RectangleF GetRandomOuterCornerTile()
    {
        return this.GetOuterCornerTile(this.GetRandomVariant(this.OuterCornerVariants));
    }

    public RectangleF GetInnerCornerTile(int variant)
    {
        return this.GetTileRectangle(3, variant);
    }

    public RectangleF GetRandomInnerCornerTile()
    {
        return this.GetInnerCornerTile(this.GetRandomVariant(this.InnerCornerVariants));
    }
}