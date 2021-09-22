using System;
using System.Diagnostics;
using AGame.Engine.Graphics.Rendering;

namespace AGame.Engine.Assets
{
    class FontLoader : IAssetLoader
    {
        public string AssetPrefix()
        {
            return "font";
        }

        public Asset LoadAsset(string filePath)
        {
            return new Font(filePath, 16, Font.FontFilter.NearestNeighbour, Font.FontFilter.NearestNeighbour);
        }
    }
}