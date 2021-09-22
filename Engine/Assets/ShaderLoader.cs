using System;
using System.Diagnostics;
using System.IO;
using AGame.Engine.Graphics.Rendering;
using Newtonsoft.Json;

namespace AGame.Engine.Assets
{
    class ShaderLoader : IAssetLoader
    {
        class ShaderAssetDescription
        {
            public string vertexShaderFile;
            public string fragmentShaderFile;

            public ShaderAssetDescription()
            {
                this.vertexShaderFile = "";
                this.fragmentShaderFile = "";
            }
        }

        public string AssetPrefix()
        {
            return "shader";
        }

        public Asset LoadAsset(string filePath)
        {
            using (StreamReader sr = new StreamReader(filePath))
            {
                ShaderAssetDescription sad = JsonConvert.DeserializeObject<ShaderAssetDescription>(sr.ReadToEnd());

                string pathOfShaderFile = Path.GetDirectoryName(filePath);

                string vsFile = pathOfShaderFile + $"/{sad.vertexShaderFile}";
                string fsFile = pathOfShaderFile + $"/{sad.fragmentShaderFile}";

                Shader s = new Shader(File.ReadAllText(vsFile), File.ReadAllText(fsFile));

                s.Load();

                return s;
            }
        }
    }
}