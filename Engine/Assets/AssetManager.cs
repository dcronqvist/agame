using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using AGame.Engine.DebugTools;

namespace AGame.Engine.Assets
{
    static class AssetManager
    {
        /// <summary>
        /// The dictionary which holds all the resources.
        /// </summary>
        private static Dictionary<string, Asset> Assets { get; set; }
        /// <summary>
        /// The different resource types - and resource loaders which are used in the game.
        /// </summary>
        private static Dictionary<string, IAssetLoader> AssetLoaders { get; set; }

        public static string BaseDirectory { get; set; }
        public static string ResourceDirectory { get => BaseDirectory + @"/res"; }

        //-------------------------------------------------------------------------//

        /// <summary>
        /// Simple little constructor for this static asset manager class.
        /// </summary>
        static AssetManager()
        {
            Assets = new Dictionary<string, Asset>();
            BaseDirectory = Utilities.GetExecutableDirectory();

            AssetLoaders = new Dictionary<string, IAssetLoader>() {
                { ".ttf", new FontLoader() },
                { ".shader", new ShaderLoader() },
                { ".png", new TextureLoader() }
            };
        }

        public static T GetAsset<T>(string resName) where T : Asset
        {
            return (T)Assets[resName];
        }

        public static T[] GetAssetsOfType<T>() where T : Asset
        {
            List<T> resources = new List<T>();
            foreach (Asset r in Assets.Values)
            {
                if (r.GetType() == typeof(T))
                    resources.Add((T)r);
            }
            return resources.ToArray();
        }

        public static void AddAsset(string assetName, Asset asset)
        {
            Assets.Add(assetName, asset);
            GameConsole.WriteLine("ASSETS", $"Loaded asset {assetName} successfully!");
        }

        private static string[] GetAllAssets()
        {
            // Get all files resources directory
            string[] files = Directory.GetFiles(ResourceDirectory, "*.*", SearchOption.AllDirectories);
            // Remove all files that are in the scripts directory and remove the resource.types file.
            // files = files.Where(x => !x.Contains(ScriptManager.ScriptDirectory) && x != ResourceTypeFile && ResourceTypes.Keys.Contains(Path.GetExtension(x)) && !x.Contains("screen.ing")).ToArray();
            files = files.Where(x => AssetLoaders.ContainsKey(Path.GetExtension(x)) && Path.GetExtension(x) != ".cs").ToArray();
            return files;
        }

        private static Asset LoadAsset(string file, out IAssetLoader assetLoader)
        {
            // Get the corresponding resource type connected to the file's 
            assetLoader = AssetLoaders[Path.GetExtension(file)];
            return assetLoader.LoadAsset(file);
        }

        /// <summary>
        /// Loads all resource in the resources directory of the game.
        /// </summary>
        public static void LoadAllAssets()
        {
            //StbImageSharp.StbImage.stbi_set_flip_vertically_on_load(1);

            string[] assetFiles = GetAllAssets();

            foreach (string file in assetFiles)
            {
                // Load each resource file
                string assetName = Path.GetFileNameWithoutExtension(file);
                Asset asset = LoadAsset(file, out IAssetLoader assetLoader);
                asset.Name = assetLoader.AssetPrefix() + "_" + assetName;

                AddAsset(asset.Name, asset);
            }
        }
    }
}