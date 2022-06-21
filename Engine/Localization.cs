using AGame.Engine.Assets;
using AGame.Engine.Configuration;

namespace AGame.Engine;

public static class Localization
{
    private static Locale _locale;

    public static void Init(string defaultLocale)
    {
        _locale = AssetManager.GetAsset<Locale>($"locale_{defaultLocale}");
    }

    public static void SetLocale(string locale, bool prepend = true)
    {
        if (prepend)
        {
            _locale = AssetManager.GetAsset<Locale>($"locale_{locale}");
            // Also set settings
            _ = Settings.SetSettingAsync("locale", locale);
        }
        else
        {
            _locale = AssetManager.GetAsset<Locale>($"{locale}");
            // Also set settings
            _ = Settings.SetSettingAsync("locale", locale.Replace("locale_", ""));
        }

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
        return AssetManager.GetAssetsOfType<Locale>().ToArray();
    }

    public static Locale GetLocale()
    {
        return _locale;
    }
}