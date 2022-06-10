using System.Text.Json;
using System.Text.Json.Serialization;
using AGame.Engine.ECSys;

namespace AGame.Engine.Assets;

public class TileLoader : IAssetLoader
{
    public string AssetPrefix()
    {
        return "tile";
    }

    public Asset LoadAsset(string filePath)
    {
        // Assume a JSON description of the entity
        JsonSerializerOptions options = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            IncludeFields = true,
            AllowTrailingCommas = true
        };

        using (StreamReader sr = new StreamReader(filePath))
        {
            string json = sr.ReadToEnd();
            TileDescription ed = JsonSerializer.Deserialize<TileDescription>(json, options);
            return ed;
        }
    }
}
