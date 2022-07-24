using AGame.Engine.Assets.Scripting;
using AGame.Engine.Configuration;
using AGame.Engine.ECSys;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Threading.Tasks;

namespace AGame.Engine.Assets;

public class FailedAsset
{
    public string Mod { get; set; }
    public Exception Exception { get; set; }
    public string FileName { get; set; }
}

public class AssetLoadedEventArgs : EventArgs
{
    public Asset Asset { get; set; }
    public ModContainer Mod { get; set; }

    public AssetLoadedEventArgs(Asset asset, ModContainer mod)
    {
        Asset = asset;
        Mod = mod;
    }
}

public class AssetFailedLoadEventArgs : EventArgs
{
    public FailedAsset FailedAsset { get; set; }
    public ModContainer Mod { get; set; }
}

public class OverwroteAssetEventArgs : EventArgs
{
    public ModOverwriteDefinition Overwrite { get; set; }
}

public static class ModManager
{
    private static string _modsFolder => $"{Utilities.GetExecutableDirectory()}/mods";
    private static string _loadOrderFile => $"{_modsFolder}/_loadorder.txt";

    private static Dictionary<string, ModContainer> _mods = new Dictionary<string, ModContainer>();
    private static Dictionary<string, IAssetLoader> _assetLoaders = new Dictionary<string, IAssetLoader>();
    private static Dictionary<string, Asset> _assets = new Dictionary<string, Asset>();

    public static event EventHandler<AssetLoadedEventArgs> AssetLoaded;
    public static event EventHandler<AssetFailedLoadEventArgs> AssetFailedLoad;
    public static event EventHandler<EventArgs> AllCoreAssetsLoaded;
    public static event EventHandler<EventArgs> AllNonCoreAssetsLoaded;
    public static event EventHandler<EventArgs> AllAssetsFinalized;
    public static event EventHandler<OverwroteAssetEventArgs> OverwroteAsset;

    public static void Init()
    {
        _assetLoaders = new Dictionary<string, IAssetLoader>() {
            { ".ttf", new FontLoader() },
            { ".shader", new ShaderLoader() },
            { ".png", new TextureLoader() },
            { ".cs", new ScriptLoader() },
            { ".entity", new EntityLoader() },
            { ".tile", new TileLoader() },
            { ".locale", new LocaleLoader() },
            { ".wav", new AudioLoader() },
            { ".anim", new AnimationLoader() },
            { ".animator", new AnimatorLoader() },
            { ".tileset", new TileSetLoader() },
            { ".item", new ItemLoader() }
        };
    }

    public static async Task LoadAllModsAsync()
    {
        ModContainer[] collectedMods = CollectMods();

        foreach (ModContainer mod in collectedMods)
        {
            ModContainer modContainer = mod;

            if (TryLoadCoreAssetsInMod(ref modContainer, out Asset[] assets, out FailedAsset[] failed))
            {
                AddAssets(assets);
            }
        }

        PerformOverwrites(collectedMods);

        AllCoreAssetsLoaded?.Invoke(null, EventArgs.Empty);

        await Task.Run(() =>
        {
            foreach (ModContainer mod in collectedMods)
            {
                try
                {

                    ModContainer modContainer = mod;

                    if (TryLoadNonCoreAssetsInMod(ref modContainer, out Asset[] assets, out FailedAsset[] failed))
                    {
                        AddAssets(assets);
                    }
                }
                catch (Exception ex)
                {
                    Logging.Log(LogLevel.Error, $"Failed to load assets in mod {mod.Name}, error: {ex.Message}");
                }
            }
        });

        PerformOverwrites(collectedMods);

        AllNonCoreAssetsLoaded?.Invoke(null, EventArgs.Empty);
    }

    private static void PerformOverwrites(ModContainer[] mods)
    {
        foreach (ModContainer mod in mods)
        {
            List<ModOverwriteDefinition> overwrites = mod.MetaData.Overwrites;

            foreach (ModOverwriteDefinition def in overwrites)
            {
                if (_assets.ContainsKey(def.Original) && _assets.ContainsKey(def.New))
                {
                    _assets[def.Original] = _assets[def.New];
                    _assets.Remove(def.New);
                    OverwroteAsset?.Invoke(null, new OverwroteAssetEventArgs() { Overwrite = def });
                }
            }
        }
    }

    private static void AddAssets(Asset[] assets)
    {
        foreach (Asset asset in assets)
        {
            AddAsset(asset);
        }
    }

    private static ModContainer[] CollectMods()
    {
        List<string> loadOrder = new List<string>();
        List<ModContainer> mods = new List<ModContainer>();

        Logging.Log(LogLevel.Info, "Discovering mods...");

        if (File.Exists(_loadOrderFile))
        {
            // Load the load order file
            using (StreamReader sr = new StreamReader(_loadOrderFile))
            {
                string line;
                while ((line = sr.ReadLine()) != null)
                {
                    loadOrder.Add(line);
                }
            }
        }

        // Folder mods
        string[] folderMods = Directory.GetDirectories(_modsFolder, "*", SearchOption.TopDirectoryOnly);
        foreach (string folderMod in folderMods)
        {
            string modName = (new DirectoryInfo(folderMod)).Name;

            if (!File.Exists($"{folderMod}{Path.DirectorySeparatorChar}meta.json"))
            {
                // This is an invalid mod, skip it.
                Logging.Log(LogLevel.Warning, $"Skipping mod {modName} because it is missing a meta.json file.");
                continue;
            }

            ModContainer mod = new ModContainer(modName, ModContainerType.Folder, folderMod);

            using (StreamReader sr = new StreamReader($"{folderMod}{Path.DirectorySeparatorChar}meta.json"))
            {
                string json = sr.ReadToEnd();
                if (ModMetaData.TryFromJson(json, out ModMetaData meta))
                {
                    Logging.Log(LogLevel.Info, $"Found valid mod {modName}");
                    mod.SetMetaData(meta);
                }
                else
                {
                    Logging.Log(LogLevel.Warning, $"Skipping mod {modName} because it has an invalid meta.json file.");
                    continue;
                }
            }

            mods.Add(mod);
        }

        // Zip mods
        string[] zipMods = Directory.GetFiles(_modsFolder, "*.zip", SearchOption.TopDirectoryOnly);
        foreach (string zipMod in zipMods)
        {
            string modName = Path.GetFileNameWithoutExtension(zipMod);
            ZipArchive zip = ZipFile.OpenRead(zipMod);

            if (!zip.Entries.Any(entry => entry.Name == "meta.json"))
            {
                // This is an invalid mod, skip it.
                zip.Dispose();
                Logging.Log(LogLevel.Warning, $"Skipping mod {modName} because it is missing a meta.json file.");
                continue;
            }

            ModContainer mod = new ModContainer(modName, ModContainerType.Zip, zipMod);

            using (StreamReader sr = new StreamReader(zip.GetEntry("meta.json").Open()))
            {
                string json = sr.ReadToEnd();
                if (ModMetaData.TryFromJson(json, out ModMetaData meta))
                {
                    Logging.Log(LogLevel.Info, $"Found valid mod {modName}");
                    mod.SetMetaData(meta);
                }
                else
                {
                    Logging.Log(LogLevel.Warning, $"Skipping mod {modName} because it has an invalid meta.json file.");
                    continue;
                }
            }

            zip.Dispose();
            mods.Add(mod);
        }

        ModContainer[] modOrder = mods.OrderBy(x => loadOrder.Contains(x.Name) ? loadOrder.IndexOf(x.Name) : int.MaxValue).ToArray();

        Logging.Log(LogLevel.Info, $"Found {modOrder.Length} mods, loading them in order: {string.Join(@", ", modOrder.Select(x => x.Name))}");

        return modOrder;
    }

    private static bool TryLoadCoreAssetsInMod(ref ModContainer mod, out Asset[] loadedAssets, out FailedAsset[] failedAssets)
    {
        if (mod.Type == ModContainerType.Folder)
        {
            return TryLoadCoreAssetsInFolderMod(ref mod, out loadedAssets, out failedAssets);
        }
        else if (mod.Type == ModContainerType.Zip)
        {
            return TryLoadCoreAssetsInZipMod(ref mod, out loadedAssets, out failedAssets);
        }
        else
        {
            loadedAssets = new Asset[0];
            failedAssets = new FailedAsset[0];
            return false;
        }
    }

    private static bool TryLoadCoreAssetsInFolderMod(ref ModContainer mod, out Asset[] loadedAssets, out FailedAsset[] failedAssets)
    {
        List<Asset> successfullyLoaded = new List<Asset>();
        List<FailedAsset> failedToLoad = new List<FailedAsset>();

        string folder = mod.Path;
        string coreFilesFolder = $"{folder}{Path.DirectorySeparatorChar}_core";

        if (!Directory.Exists(coreFilesFolder))
        {
            loadedAssets = new Asset[0];
            failedAssets = new FailedAsset[0];
            return true;
        }

        string[] coreFiles = Directory.GetFiles(coreFilesFolder, "*.*", SearchOption.AllDirectories)
                                .Where(x => FileHasLoader(x)).ToArray();

        foreach (string coreFile in coreFiles)
        {
            string fileName = Path.GetFileName(coreFile);
            string fileExtension = Path.GetExtension(coreFile);
            IAssetLoader loader = _assetLoaders[fileExtension];

            using (Stream fileStream = File.OpenRead(coreFile))
            {
                if (TryLoadAsset(mod, fileName, loader, fileStream, true, out Asset loadedAsset, out FailedAsset failedAsset))
                {
                    mod.Assets.Add(loadedAsset.Name, loadedAsset);
                    successfullyLoaded.Add(loadedAsset);
                }
                else
                {
                    failedToLoad.Add(failedAsset);
                }
            }
        }

        loadedAssets = successfullyLoaded.ToArray();
        failedAssets = failedToLoad.ToArray();

        return failedToLoad.Count == 0;
    }

    private static bool TryLoadCoreAssetsInZipMod(ref ModContainer mod, out Asset[] loadedAssets, out FailedAsset[] failedAssets)
    {
        List<Asset> successfullyLoaded = new List<Asset>();
        List<FailedAsset> failedToLoad = new List<FailedAsset>();

        string zipFile = mod.Path;
        ZipArchive zip = ZipFile.OpenRead(zipFile);

        // Checks if there is a _core folder in the zip file
        // If it doesn't, then we don't need to load any core assets from the zip file
        if (!zip.Entries.Any(entry => entry.FullName.StartsWith("_core")))
        {
            loadedAssets = new Asset[0];
            failedAssets = new FailedAsset[0];
            return true;
        }

        ZipArchiveEntry[] coreFileEntries = zip.Entries.Where(entry => entry.FullName.StartsWith("_core")).Where(entry => FileHasLoader(entry.FullName)).ToArray();

        foreach (ZipArchiveEntry coreFile in coreFileEntries)
        {
            string fileName = coreFile.Name;
            string fileExtension = Path.GetExtension(fileName);
            IAssetLoader loader = _assetLoaders[fileExtension];

            using (Stream fileStream = coreFile.Open())
            {
                if (TryLoadAsset(mod, fileName, loader, fileStream, true, out Asset loadedAsset, out FailedAsset failedAsset))
                {
                    mod.Assets.Add(loadedAsset.Name, loadedAsset);
                    successfullyLoaded.Add(loadedAsset);
                }
                else
                {
                    failedToLoad.Add(failedAsset);
                }
            }
        }

        loadedAssets = successfullyLoaded.ToArray();
        failedAssets = failedToLoad.ToArray();

        return failedToLoad.Count == 0;
    }

    private static bool TryLoadAsset(ModContainer mod, string fileName, IAssetLoader loader, Stream stream, bool init, out Asset loadedAsset, out FailedAsset failedAsset)
    {
        try
        {
            loadedAsset = loader.LoadAsset(stream);
            loadedAsset.IsCore = init;
            loadedAsset.Name = CreateAssetName(mod.Name, loader.AssetPrefix(), Path.GetFileNameWithoutExtension(fileName));
            if (init)
            {
                loadedAsset.InitOpenGL();
            }

            AssetLoaded?.Invoke(null, new AssetLoadedEventArgs(loadedAsset, mod));

            failedAsset = null;
            return true;
        }
        catch (Exception e)
        {
            loadedAsset = null;
            failedAsset = new FailedAsset()
            {
                Mod = mod.Name,
                FileName = fileName,
                Exception = e
            };

            AssetFailedLoad?.Invoke(null, new AssetFailedLoadEventArgs()
            {
                FailedAsset = failedAsset,
                Mod = mod
            });
            return false;
        }
    }

    private static bool TryLoadNonCoreAssetsInMod(ref ModContainer mod, out Asset[] loadedAssets, out FailedAsset[] failedAssets)
    {
        if (mod.Type == ModContainerType.Folder)
        {
            return TryLoadNonCoreAssetsInFolderMod(ref mod, out loadedAssets, out failedAssets);
        }
        else if (mod.Type == ModContainerType.Zip)
        {
            return TryLoadNonCoreAssetsInZipMod(ref mod, out loadedAssets, out failedAssets);
        }
        else
        {
            loadedAssets = new Asset[0];
            failedAssets = new FailedAsset[0];
            return false;
        }
    }

    private static bool TryLoadNonCoreAssetsInFolderMod(ref ModContainer mod, out Asset[] loadedAssets, out FailedAsset[] failedAssets)
    {
        List<Asset> successfullyLoaded = new List<Asset>();
        List<FailedAsset> failedToLoad = new List<FailedAsset>();

        string folder = mod.Path;
        string coreFilesFolder = $"{folder}{Path.DirectorySeparatorChar}_core";
        string[] coreFiles = new string[0];

        if (Directory.Exists(coreFilesFolder))
        {
            coreFiles = Directory.GetFiles(coreFilesFolder, "*.*", SearchOption.AllDirectories)
                                .Where(x => FileHasLoader(x)).ToArray();
        }

        string[] nonCoreFiles = Directory.GetFiles(folder, "*.*", SearchOption.AllDirectories)
                                .Where(x => FileHasLoader(x) && !coreFiles.Contains(x)).ToArray();

        foreach (string assetFile in nonCoreFiles)
        {
            string fileName = Path.GetFileName(assetFile);
            string fileExtension = Path.GetExtension(assetFile);
            IAssetLoader loader = _assetLoaders[fileExtension];

            using (Stream fileStream = File.OpenRead(assetFile))
            {
                if (TryLoadAsset(mod, fileName, loader, fileStream, false, out Asset loadedAsset, out FailedAsset failedAsset))
                {
                    mod.Assets.Add(loadedAsset.Name, loadedAsset);
                    successfullyLoaded.Add(loadedAsset);
                }
                else
                {
                    failedToLoad.Add(failedAsset);
                }
            }
        }

        loadedAssets = successfullyLoaded.ToArray();
        failedAssets = failedToLoad.ToArray();

        return failedToLoad.Count == 0;
    }

    private static bool TryLoadNonCoreAssetsInZipMod(ref ModContainer mod, out Asset[] loadedAssets, out FailedAsset[] failedAssets)
    {
        List<Asset> successfullyLoaded = new List<Asset>();
        List<FailedAsset> failedToLoad = new List<FailedAsset>();

        string zipFile = mod.Path;
        ZipArchive zip = ZipFile.OpenRead(zipFile);

        ZipArchiveEntry[] coreFileEntries = zip.Entries.Where(entry => entry.FullName.StartsWith("_core")).Where(entry => FileHasLoader(entry.FullName)).ToArray();
        ZipArchiveEntry[] nonCoreFileEntries = zip.Entries.Where(entry => !entry.FullName.StartsWith("_core")).Where(entry => FileHasLoader(entry.FullName)).ToArray();

        foreach (ZipArchiveEntry assetFile in nonCoreFileEntries)
        {
            string fileName = assetFile.Name;
            string fileExtension = Path.GetExtension(fileName);
            IAssetLoader loader = _assetLoaders[fileExtension];

            using (Stream fileStream = assetFile.Open())
            {
                if (TryLoadAsset(mod, fileName, loader, fileStream, false, out Asset loadedAsset, out FailedAsset failedAsset))
                {
                    mod.Assets.Add(loadedAsset.Name, loadedAsset);
                    successfullyLoaded.Add(loadedAsset);
                }
                else
                {
                    failedToLoad.Add(failedAsset);
                }
            }
        }

        loadedAssets = successfullyLoaded.ToArray();
        failedAssets = failedToLoad.ToArray();

        return failedToLoad.Count == 0;
    }

    public static void FinalizeAllAssets()
    {
        foreach (Asset asset in _assets.Values.Where(a => !a.IsCore))
        {
            asset.InitOpenGL();
        }

        AllAssetsFinalized?.Invoke(null, EventArgs.Empty);
    }

    public static T GetAsset<T>(string assetName) where T : Asset
    {
        return (T)_assets[assetName];
    }

    public static bool AssetExists(string assetName)
    {
        return _assets.ContainsKey(assetName);
    }

    public static T[] GetAssetsOfType<T>() where T : Asset
    {
        return _assets.Values.Where(a => a is T).Cast<T>().ToArray();
    }

    public static T[] GetAssetsOfType<T>(Type type)
    {
        return _assets.Values.Where(a => a is T).Cast<T>().ToArray();
    }

    private static void AddAsset(Asset asset)
    {
        _assets.Add(asset.Name, asset);
    }

    private static bool TryGetAssetLoaderForExtension(string extension, out IAssetLoader loader)
    {
        return _assetLoaders.TryGetValue(extension, out loader);
    }

    private static bool FileHasLoader(string file)
    {
        string extension = Path.GetExtension(file);
        return _assetLoaders.ContainsKey(extension);
    }

    private static string[] GetCoreAssetsInFolderMod(string folder)
    {
        string[] allFiles = Directory.GetFiles($"{folder}{Path.DirectorySeparatorChar}_core", "*.*", SearchOption.AllDirectories);
        string[] coreFiles = allFiles.Where(file => FileHasLoader(file)).ToArray();

        return coreFiles;
    }

    private static string CreateAssetName(string mod, string assetPrefix, string fileName)
    {
        return $"{mod}.{assetPrefix}.{fileName}";
    }
}