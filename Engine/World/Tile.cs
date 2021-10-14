using System;
using System.Numerics;
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
                return (int)Math.Ceiling((double)this.Texture.Width / StaticTileGrid.TILE_SIZE);
            }
        }
        public int Height
        {
            get
            {
                return (int)Math.Ceiling((double)this.Texture.Height / StaticTileGrid.TILE_SIZE);
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
        public Vector2 TopLeftInTexture { get; set; }

        public Tile(string texName, bool solid, Vector2 topLeftInTexture)
        {
            this.TopLeftInTexture = topLeftInTexture;
            this.TextureName = texName;
            this.Solid = solid;
        }
    }
}