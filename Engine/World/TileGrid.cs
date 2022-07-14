using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Numerics;
using AGame.Engine.Assets;
using AGame.Engine.Graphics;
using AGame.Engine.Graphics.Rendering;

namespace AGame.Engine.World;

public abstract class TileGrid
{
    public const int TILE_SIZE = 32;

    // Size
    public int Width { get; set; }
    public int Height { get; set; }
    public Vector2 GlobalOffset { get; set; }

    public TileGrid(int width, int height, Vector2 globalOffset)
    {
        Width = width;
        Height = height;
        GlobalOffset = globalOffset;
    }

    public abstract string GetTileNameAtPos(int x, int y);
    public abstract int GetTileIDAtPos(int x, int y);
    public virtual void Render() { }
}
