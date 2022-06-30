using System;
using System.Diagnostics;
using AGame.Engine.Graphics.Rendering;

namespace AGame.Engine.Assets
{
    class TextureLoader : IAssetLoader
    {
        public string AssetPrefix()
        {
            return "tex";
        }

        public Asset LoadAsset(Stream fileStream)
        {
            using (MemoryStream ms = new MemoryStream())
            {
                fileStream.CopyTo(ms);

                return Texture2D.FromStream(ms);
            }
        }
    }
}