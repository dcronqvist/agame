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
            return new Texture2D(filePath);
        }
    }
}