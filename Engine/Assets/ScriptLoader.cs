using System;
using System.Collections.Generic;
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

        public Asset LoadAsset(Stream fileStream)
        {
            using (BinaryReader sr = new BinaryReader(fileStream))
            {
                // ScriptCompiler sc = new ScriptCompiler();
                // byte[] iass = sc.Compile(sr.ReadToEnd(), out string[] errorMsgs);
                // if (errorMsgs.Length > 0)
                // {
                //     // This failed to load the script.
                //     throw new Exception(string.Join("\n", errorMsgs));
                // }

                // //Assembly assembly = Assembly.LoadFile(filePath);
                // Assembly assembly = Assembly.Load(iass);
                // return new Script(assembly);
                var assembly = System.Runtime.Loader.AssemblyLoadContext.Default.LoadFromStream(fileStream);
                return new Script(assembly);
            }
        }
    }
}