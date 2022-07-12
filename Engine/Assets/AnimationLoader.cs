using System.Text.Json;
using System.Text.Json.Serialization;
using AGame.Engine.ECSys;
using AGame.Engine.World;

namespace AGame.Engine.Assets;

public class AnimationLoader : IAssetLoader
{
    public string AssetPrefix()
    {
        return "anim";
    }

    public Asset LoadAsset(Stream fileStream)
    {
        // Assume a JSON description of the entity
        JsonSerializerOptions options = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            IncludeFields = true,
            AllowTrailingCommas = true
        };

        using (StreamReader sr = new StreamReader(fileStream))
        {
            string json = sr.ReadToEnd();
            AnimationDescription ed = JsonSerializer.Deserialize<AnimationDescription>(json, options);
            return ed;
        }
    }
}