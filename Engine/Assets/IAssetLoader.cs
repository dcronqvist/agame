namespace AGame.Engine.Assets
{
    interface IAssetLoader
    {
        Asset LoadAsset(string filePath);
        string AssetPrefix();
    }
}