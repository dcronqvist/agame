using System.Text.Json;
using System.Text.Json.Serialization;

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
            ItemDescription ed = JsonSerializer.Deserialize<ItemDescription>(json, options);
            return ed;
        }
    }
}

public class ItemConverter : JsonConverter<ItemDescription>
{
    public override bool CanConvert(Type typeToConvert)
    {
        return typeof(ItemDescription).IsAssignableFrom(typeToConvert);
    }

    public override ItemDescription Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        JsonDocument document = JsonDocument.ParseValue(ref reader);
        document.RootElement.TryGetProperty("itemType", out JsonElement ct);

        string s = ct.GetString();
        ItemType type = Enum.Parse<ItemType>(s, true);

        JsonSerializerOptions opts = new JsonSerializerOptions(options);
        opts.Converters.Remove(this);

        switch (type)
        {
            case ItemType.Tool:
                return document.Deserialize<ToolDescription>(opts);
            case ItemType.Consumable:
                return document.Deserialize<ConsumableDescription>(opts);
            case ItemType.Equipable:
                return document.Deserialize<EquipableDescription>(opts);
            case ItemType.Placeable:
                return document.Deserialize<PlaceableDescription>(opts);
        }

        return null;
    }

    public override void Write(Utf8JsonWriter writer, ItemDescription value, JsonSerializerOptions options)
    {
        writer.WriteRawValue(JsonSerializer.Serialize(value, options));
    }
}