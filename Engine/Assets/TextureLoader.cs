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

        public Asset LoadAsset(string filePath)
        {
            Texture2D.TryLoadFromFile(filePath, out Texture2D tex);
            return tex;
        }
    }
}