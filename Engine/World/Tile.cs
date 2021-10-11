using System;
using AGame.Engine.Assets;
using AGame.World;

namespace AGame.Engine.World
{
    class Tile
    {
        public int Width
        {
            get
            {
                return (int)Math.Ceiling((double)this.Texture.Width / TileGrid.TILE_SIZE);
            }
        }
        public int Height
        {
            get
            {
                return (int)Math.Ceiling((double)this.Texture.Height / TileGrid.TILE_SIZE);
            }
        }

        public bool Solid { get; set; }
        public string TextureName { get; set; }
        private Texture2D _texture;
        public Texture2D Texture
        {
            get
            {
                if (_texture == null)
                {
                    _texture = AssetManager.GetAsset<Texture2D>(this.TextureName);
                }
                return _texture;
            }
        }

        public Tile(string texName, bool solid)
        {
            this.TextureName = texName;
            this.Solid = solid;
        }
    }
}