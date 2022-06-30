namespace AGame.Engine.Assets
{
    public interface IAssetLoader
    {
        Asset LoadAsset(Stream fileStream);
        string AssetPrefix();
    }
}