namespace AGame.Engine.Assets
{
    interface IAssetLoader<T> where T : Asset
    {
        T LoadAsset(string filePath);
    }
}