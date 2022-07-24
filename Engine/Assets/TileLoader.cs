using System;
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;
using AGame.Engine.ECSys;
using AGame.Engine.World;

namespace AGame.Engine.Assets;

public class TileLoader : IAssetLoader
{
    public string AssetPrefix()
    {
        return "tile";
    }

    public Asset LoadAsset(Stream fileStream)
    {
        // Assume a JSON description of the entity
        JsonSerializerOptions options = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            IncludeFields = true,
            Converters = { new TileConverter(), new JsonStringEnumConverter() },
            AllowTrailingCommas = true
        };

        using (StreamReader sr = new StreamReader(fileStream))
        {
            string json = sr.ReadToEnd();
            TileDescription ed = JsonSerializer.Deserialize<TileDescription>(json, options);
            return ed;
        }
    }
}

public class TileConverter : JsonConverter<TileDescription>
{
    public override bool CanConvert(Type typeToConvert)
    {
        return typeof(TileDescription).IsAssignableFrom(typeToConvert);
    }

    public override TileDescription Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        JsonDocument document = JsonDocument.ParseValue(ref reader);
        document.RootElement.TryGetProperty("tileType", out JsonElement ct);

        string s = ct.GetString();
        TileType type = Enum.Parse<TileType>(s);

        JsonSerializerOptions opts = new JsonSerializerOptions(options);
        opts.Converters.Remove(this);

        switch (type)
        {
            case TileType.Ground:
                return document.Deserialize<GroundTileDescription>(opts);
            case TileType.Floor:
                return document.Deserialize<FloorTileDescription>(opts);
            case TileType.Building:
                return document.Deserialize<BuildingTileDescription>(opts);
            case TileType.Air:
                return document.Deserialize<AirTileDescription>(opts);
        }

        return null;
    }

    public override void Write(Utf8JsonWriter writer, TileDescription value, JsonSerializerOptions options)
    {
        writer.WriteRawValue(JsonSerializer.Serialize(value, options));
    }
}
