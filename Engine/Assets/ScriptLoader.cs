using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using AGame.Engine.Assets.Scripting;
using AGame.Engine.Graphics.Rendering;

namespace AGame.Engine.Assets
{
    class ScriptLoader : IAssetLoader
    {
        public string AssetPrefix()
        {
            return "script";
        }

        public Asset LoadAsset(string filePath)
        {
            ScriptCompiler sc = new ScriptCompiler();
            byte[] iass = sc.Compile(filePath, out string[] errorMsgs);
            if (errorMsgs.Length > 0)
            {
                // This failed to load the script.
                throw new Exception(string.Join("\n", errorMsgs));
            }

            //Assembly assembly = Assembly.LoadFile(filePath);
            Assembly assembly = Assembly.Load(iass);
            return new Script(assembly, Path.GetFileNameWithoutExtension(filePath));
        }
    }
}