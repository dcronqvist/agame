using System;
using System.Numerics;
using AGame.Engine.Assets;
using AGame.Engine.Graphics;
using AGame.World;

namespace AGame.Engine.World
{
    public class Tile
    {
        public int Width { get; set; }
        public int Height { get; set; }
        public string Name { get; set; }

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

        public void SetTileName(string tileName)
        {
            this.Name = tileName;
        }

        public virtual void Update()
        {

        }

        public virtual IRenderable GetRenderable(Vector2 worldPos)
        {
            float scale = ((TileGrid.TILE_SIZE / (float)this.Texture.Width) * this.Width);
            return new TileRenderable(worldPos - (scale * this.TopLeftInTexture), this.Texture, this.TopLeftInTexture, this.Width, this.Height);
        }
    }
}