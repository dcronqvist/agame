using AGame.Engine.Assets;

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
        }
        else
        {
            _locale = AssetManager.GetAsset<Locale>($"{locale}");
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