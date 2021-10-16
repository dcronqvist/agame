using System;
using System.Numerics;
using AGame.Engine.Assets;
using AGame.World;

namespace AGame.Engine.World
{
    class Tile
    {
        public int Width { get; set; }
        public int Height { get; set; }

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

        public Tile(string texName, bool solid, Vector2 topLeftInTexture, int width, int height)
        {
            this.TopLeftInTexture = topLeftInTexture;
            this.TextureName = texName;
            this.Solid = solid;
            this.Width = width;
            this.Height = height;
        }
    }
}