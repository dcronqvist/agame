using System;
using System.Diagnostics;
using System.IO;
using AGame.Engine.Graphics.Rendering;
namespace AGame.Engine.Assets
{
    class ShaderLoader : IAssetLoader
    {
        public string AssetPrefix()
        {
            return "shader";
        }

        // Get the contained substring of a source string between two delimiters. 
        // Delimiters are not included in the result
        private string GetSubstringContainedWithin(string source, string startTag, string endTag)
        {
            int startIndex = source.IndexOf(startTag) + startTag.Length;
            int endIndex = source.IndexOf(endTag);
            return source.Substring(startIndex, endIndex - startIndex);
        }

        public Asset LoadAsset(Stream fileStream)
        {
            using (StreamReader sr = new StreamReader(fileStream))
            {
                string text = sr.ReadToEnd();

                string vertexShader = GetSubstringContainedWithin(text, "#VERTEX_SHADER_BEGIN", "#VERTEX_SHADER_END");
                string fragmentShader = GetSubstringContainedWithin(text, "#FRAGMENT_SHADER_BEGIN", "#FRAGMENT_SHADER_END");

                Shader s = new Shader(vertexShader, fragmentShader);

                return s;
            }
        }
    }
}