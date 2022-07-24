using System;
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;
using AGame.Engine.Items;

namespace AGame.Engine.Assets;

public class ItemLoader : IAssetLoader
{
    public string AssetPrefix()
    {
        return "item";
    }

    public Asset LoadAsset(Stream fileStream)
    {
        JsonSerializerOptions options = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            IncludeFields = true,
            AllowTrailingCommas = true,
            Converters = { new JsonStringEnumConverter(), new ItemConverter() }
        };

        using (StreamReader sr = new StreamReader(fileStream))
        {
            string json = sr.ReadToEnd();
            ItemDefinition ed = JsonSerializer.Deserialize<ItemDefinition>(json, options);
            return ed;
        }
    }
}

public class ItemConverter : JsonConverter<ItemComponentDefinition>
{
    public override bool CanConvert(Type typeToConvert)
    {
        return typeof(ItemComponentDefinition).IsAssignableFrom(typeToConvert);
    }

    public override ItemComponentDefinition Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        JsonDocument document = JsonDocument.ParseValue(ref reader);
        document.RootElement.TryGetProperty("type", out JsonElement ct);

        string s = ct.GetString();
        Type componentType = ItemManager.GetComponentTypeByName(s);

        JsonSerializerOptions opts = new JsonSerializerOptions(options);
        opts.Converters.Remove(this);

        return document.Deserialize(componentType, opts) as ItemComponentDefinition;
    }

    public override void Write(Utf8JsonWriter writer, ItemComponentDefinition value, JsonSerializerOptions options)
    {
        writer.WriteRawValue(JsonSerializer.Serialize(value, options));
    }
}