using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace AGame.Engine.Assets;

public enum ModContainerType
{
    Folder,
    Zip
}

public enum ModOverwriteType
{
    Asset,
    Script
}

public class ModOverwriteDefinition
{
    public ModOverwriteType Type { get; set; }
    public string Original { get; set; }
    public string New { get; set; }
}

public class ModMetaData
{
    public string Author { get; set; }
    public string Version { get; set; }

    public List<ModOverwriteDefinition> Overwrites { get; set; }

    public static bool TryFromJson(string json, out ModMetaData meta)
    {
        JsonSerializerOptions ops = new JsonSerializerOptions()
        {
            IncludeFields = true,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            AllowTrailingCommas = true,
            ReadCommentHandling = JsonCommentHandling.Skip,
            Converters = { new JsonStringEnumConverter(JsonNamingPolicy.CamelCase) }
        };

        try
        {
            meta = JsonSerializer.Deserialize<ModMetaData>(json, ops);
            return true;
        }
        catch (JsonException)
        {
            meta = null;
            return false;
        }
    }
}

public class ModContainer
{
    public string Name { get; set; }
    public ModContainerType Type { get; set; }
    public string Path { get; set; }
    public Dictionary<string, Asset> Assets { get; set; }
    public ModMetaData MetaData { get; private set; }

    public ModContainer(string name, ModContainerType type, string path)
    {
        Name = name;
        Type = type;
        Path = path;
        Assets = new Dictionary<string, Asset>();
    }

    public void SetMetaData(ModMetaData metaData)
    {
        MetaData = metaData;
    }
}