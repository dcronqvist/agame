using System.Text.Json;
using System.Text.Json.Serialization;
using AGame.Engine.ECSys;

namespace AGame.Engine.Assets;

public class EntityLoader : IAssetLoader
{
    public string AssetPrefix()
    {
        return "entity";
    }

    public Asset LoadAsset(Stream fileStream)
    {
        // Assume a JSON description of the entity
        JsonSerializerOptions options = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            Converters = { new ComponentConverter() },
            IncludeFields = true,
            AllowTrailingCommas = true,
            IgnoreReadOnlyFields = true,
            IgnoreReadOnlyProperties = true,
        };

        using (StreamReader sr = new StreamReader(fileStream))
        {
            string json = sr.ReadToEnd();
            EntityDescription ed = JsonSerializer.Deserialize<EntityDescription>(json, options);
            return ed;
        }
    }
}

public class ComponentConverter : JsonConverter<Component>
{
    public override bool CanConvert(Type typeToConvert)
    {
        return typeof(Component).IsAssignableFrom(typeToConvert);
    }

    public override Component Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        JsonDocument document = JsonDocument.ParseValue(ref reader);
        document.RootElement.TryGetProperty("componentType", out JsonElement ct);
        string typeId = ct.GetString();

        Type componentType = ECS.Instance.Value.GetComponentType(typeId);

        JsonSerializerOptions opts = new JsonSerializerOptions(options);
        opts.Converters.Remove(this);

        Component c = document.Deserialize(componentType, opts) as Component;
        c.ComponentType = typeId;
        return c;
    }

    public override void Write(Utf8JsonWriter writer, Component value, JsonSerializerOptions options)
    {
        JsonSerializerOptions opts = new JsonSerializerOptions(options);
        opts.Converters.Remove(this);

        writer.WriteRawValue(JsonSerializer.Serialize(value, value.GetType(), opts));
    }
}