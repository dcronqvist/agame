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

        public Asset LoadAsset(Stream fileStream)
        {
            using (BinaryReader br = new BinaryReader(fileStream))
            {
                // Gather all bytes from binary reader
                byte[] data = br.ReadBytes((int)fileStream.Length);
                return new Font(data, 16, Font.FontFilter.NearestNeighbour, Font.FontFilter.NearestNeighbour);
            }
        }
    }
}