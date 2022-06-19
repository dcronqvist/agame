using System.Text.Json;
using System.Text.Json.Serialization;

namespace AGame.Engine.Configuration;

public class Setting
{
    [JsonIgnore]
    public string Name { get; set; }
    public object Value { get; set; }

    public T GetValue<T>()
    {
        return (T)Value;
    }
}

public static class Settings
{
    private static string _settingsFileLocation = "./";
    private static string _settingsFile = "settings.json";
    private static JsonSerializerOptions _jsonSerializerOptions = new JsonSerializerOptions
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        IncludeFields = true,
        AllowTrailingCommas = true,
        WriteIndented = true
    };
    private static Dictionary<string, Setting> _settings = new Dictionary<string, Setting>();

    private static List<Setting> GetDefaultSettings()
    {
        return new List<Setting>() {
            new Setting() { Name = "locale", Value = "en_US" },
        };
    }

    public static string GetSettingsFilePath()
    {
        return _settingsFileLocation + _settingsFile;
    }

    private static async Task CreateSettingsFileWithDefaults()
    {
        List<Setting> settings = GetDefaultSettings();

        Dictionary<string, object> settingsDict = new Dictionary<string, object>();

        foreach (Setting setting in settings)
        {
            settingsDict.Add(setting.Name, setting.Value);
        }

        await SaveSettingsAsync(settingsDict);
    }

    public static async Task LoadSettings()
    {
        if (!File.Exists(GetSettingsFilePath()))
        {
            await CreateSettingsFileWithDefaults();
        }

        List<Setting> defaultSettings = GetDefaultSettings();

        using (StreamReader sr = new StreamReader(GetSettingsFilePath()))
        {
            string json = await sr.ReadToEndAsync();
            Dictionary<string, JsonElement> settings = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(json, _jsonSerializerOptions);

            foreach (KeyValuePair<string, JsonElement> kvp in settings)
            {
                if (defaultSettings.Find(x => x.Name == kvp.Key) != null)
                {
                    defaultSettings.Find(x => x.Name == kvp.Key).Value = kvp.Value.Deserialize(defaultSettings.Find(x => x.Name == kvp.Key).Value.GetType(), _jsonSerializerOptions);
                }
            }
        }

        foreach (Setting setting in defaultSettings)
        {
            _settings.Add(setting.Name, setting);
        }
    }

    public static async Task SaveSettingsAsync(Dictionary<string, object> settings)
    {
        string json = JsonSerializer.Serialize(settings, _jsonSerializerOptions);

        using (StreamWriter sw = new StreamWriter(GetSettingsFilePath()))
        {
            await sw.WriteAsync(json);
        }
    }

    public static async Task SaveSettingsAsync(Dictionary<string, Setting> settings)
    {
        Dictionary<string, object> settingsDict = new Dictionary<string, object>();
        foreach (KeyValuePair<string, Setting> kvp in settings)
        {
            settingsDict.Add(kvp.Key, kvp.Value.Value);
        }

        await SaveSettingsAsync(settingsDict);
    }

    public static T GetSetting<T>(string name)
    {
        return _settings[name].GetValue<T>();
    }

    public static async Task SetSettingAsync(string name, object value)
    {
        _settings[name].Value = value;
        await SaveSettingsAsync(_settings);
    }
}