using System.Linq;
using AGame.Engine.Assets;
using AGame.Engine.Configuration;

namespace AGame.Engine;

public static class Localization
{
    private static Locale _locale;

    public static bool Init(string defaultLocale)
    {
        if (!ModManager.AssetExists(defaultLocale))
        {
            Logging.Log(LogLevel.Error, $"Locale {defaultLocale} not found");
            return false;
        }

        _locale = ModManager.GetAsset<Locale>(defaultLocale);
        Logging.Log(LogLevel.Info, $"Initialized localization to {_locale.Name}");
        return true;
    }

    public static void SetLocale(string locale)
    {
        _locale = ModManager.GetAsset<Locale>(locale);
        Logging.Log(LogLevel.Info, $"Set localization to {_locale.Name}");

        // Also set settings
        _ = Settings.SetSettingAsync("locale", locale);
    }

    public static string GetString(string key, params (string, string)[] context)
    {
        Locale.Context ctx = new Locale.Context();
        foreach ((string, string) t in context)
        {
            ctx.Add(t.Item1, t.Item2);
        }

        return _locale.GetString(key, ctx);
    }

    public static Locale[] GetAvailableLocales()
    {
        return ModManager.GetAssetsOfType<Locale>().ToArray();
    }

    public static Locale GetLocale()
    {
        return _locale;
    }
}