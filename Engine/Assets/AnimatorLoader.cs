using System.Text.Json;
using System.Text.Json.Serialization;
using AGame.Engine.ECSys;
using AGame.Engine.World;

namespace AGame.Engine.Assets;

public class AnimatorLoader : IAssetLoader
{
    public string AssetPrefix()
    {
        return "animator";
    }

    public Asset LoadAsset(Stream fileStream)
    {
        // Assume a JSON description of the entity
        JsonSerializerOptions options = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            IncludeFields = true,
            AllowTrailingCommas = true,
            Converters = { new JsonStringEnumConverter() }
        };

        using (StreamReader sr = new StreamReader(fileStream))
        {
            string json = sr.ReadToEnd();
            AnimatorDescription ed = JsonSerializer.Deserialize<AnimatorDescription>(json, options);
            return ed;
        }
    }
}