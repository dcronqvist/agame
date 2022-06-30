using AGame.Engine.Assets;
using AGame.Engine.Configuration;

namespace AGame.Engine;

public static class Localization
{
    private static Locale _locale;

    public static void Init(string defaultLocale)
    {
        _locale = ModManager.GetAsset<Locale>(defaultLocale);
    }

    public static void SetLocale(string locale)
    {
        _locale = ModManager.GetAsset<Locale>(locale);
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