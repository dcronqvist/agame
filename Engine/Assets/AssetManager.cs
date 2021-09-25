using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using AGame.Engine.DebugTools;
using Newtonsoft.Json;

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
        public static string AssetDirectory { get => BaseDirectory + @"/res"; }
        public static string CoreDirectory { get => AssetDirectory + @"/core"; }

        public static event EventHandler OnAllAssetsLoaded;
        public static event EventHandler OnAllCoreAssetsLoaded;
        public static event EventHandler<Asset> OnAssetLoaded;
        public static event EventHandler OnFinalizeStart;
        public static event EventHandler OnFinalizeEnd;

        public static bool AllAssetsLoaded { get; set; }
        public static int TotalAssetsToLoad { get; set; }
        public static int AssetsLoaded { get; set; }
        public static float LoadedPercentage
        {
            get
            {
                if (TotalAssetsToLoad == 0)
                {
                    return 0f;
                }
                return AssetsLoaded / TotalAssetsToLoad;
            }
        }

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
                { ".png", new TextureLoader() },
                { ".cs", new ScriptLoader() }
            };

            AllAssetsLoaded = false;
        }

        private static bool FilterOnlyFilesWithAssetExtensions(string file)
        {
            return AssetLoaders.ContainsKey(Path.GetExtension(file));
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
            OnAssetLoaded?.Invoke(null, asset);
        }

        private static string[] GetAllAssets()
        {
            List<string> coreAssets = GetCoreAssets().Select(x => Path.GetFileName(x)).ToList();

            // Get all files resources directory
            string[] files = Directory.GetFiles(AssetDirectory, "*.*", SearchOption.AllDirectories);
            // Remove all files that are in the scripts directory and remove the resource.types file.
            // files = files.Where(x => !x.Contains(ScriptManager.ScriptDirectory) && x != ResourceTypeFile && ResourceTypes.Keys.Contains(Path.GetExtension(x)) && !x.Contains("screen.ing")).ToArray();
            files = files.Where(x => FilterOnlyFilesWithAssetExtensions(x) && !coreAssets.Contains(Path.GetFileName(x))).ToArray();
            return files;
        }

        private static Asset LoadAsset(string file, out IAssetLoader assetLoader)
        {
            // Get the corresponding resource type connected to the file's 
            assetLoader = AssetLoaders[Path.GetExtension(file)];
            return assetLoader.LoadAsset(file);
        }

        private static string[] GetCoreAssets()
        {
            string[] files = Directory.GetFiles(CoreDirectory, "*.*", SearchOption.AllDirectories).Where(FilterOnlyFilesWithAssetExtensions).ToArray();
            return files;
        }

        private static void LoadAllCoreAssets()
        {
            string[] coreAssets = GetCoreAssets();
            TotalAssetsToLoad = coreAssets.Length;

            foreach (string file in coreAssets)
            {
                // Load each resource file
                string assetName = Path.GetFileNameWithoutExtension(file);
                Asset asset = LoadAsset(file, out IAssetLoader assetLoader);
                asset.Name = assetLoader.AssetPrefix() + "_" + assetName;
                asset.IsCore = true;
                asset.InitOpenGL();

                AddAsset(asset.Name, asset);

                AssetsLoaded++;
            }

            OnAllCoreAssetsLoaded?.Invoke(null, EventArgs.Empty);
        }

        /// <summary>
        /// Loads all resource in the resources directory of the game.
        /// </summary>
        public static void LoadAllAssets(bool skipCore, bool finalize)
        {
            if (!skipCore)
            {
                LoadAllCoreAssets();
            }

            string[] assetFiles = GetAllAssets();

            foreach (string file in assetFiles)
            {
                // Load each resource file
                string assetName = Path.GetFileNameWithoutExtension(file);
                Asset asset = LoadAsset(file, out IAssetLoader assetLoader);
                asset.Name = assetLoader.AssetPrefix() + "_" + assetName;
                asset.IsCore = false;

                AddAsset(asset.Name, asset);

                AssetsLoaded++;
            }

            OnAllAssetsLoaded?.Invoke(null, EventArgs.Empty);
            AllAssetsLoaded = true;

            if (finalize)
            {
                FinalizeAssets();
            }
        }

        public static void LoadAllAssetsAsync()
        {
            LoadAllCoreAssets();

            string[] assetFiles = GetAllAssets();
            TotalAssetsToLoad += assetFiles.Length;
            Thread thread = new Thread(() => LoadAllAssets(true, false));

            thread.Start();
        }

        public static void FinalizeAssets()
        {
            OnFinalizeStart?.Invoke(null, EventArgs.Empty);

            foreach (KeyValuePair<string, Asset> kvp in Assets.Where(x => !x.Value.IsCore))
            {
                kvp.Value.InitOpenGL();
            }

            OnFinalizeEnd?.Invoke(null, EventArgs.Empty);
        }
    }
}