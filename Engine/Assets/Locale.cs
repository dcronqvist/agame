using System.Collections.Generic;

namespace AGame.Engine.Assets;

public class Locale : Asset
{
    public class Context : Dictionary<string, string> { }

    public string LocaleName { get; set; }
    private Dictionary<string, string> _locale;

    public Locale(string name, Dictionary<string, string> locale)
    {
        this.LocaleName = name;
        this._locale = locale;
    }

    public override bool InitOpenGL()
    {
        // Do nothing
        return true;
    }

    public string GetString(string key, params (string, string)[] context)
    {
        Context ctx = new Context();
        foreach ((string, string) t in context)
        {
            ctx.Add(t.Item1, t.Item2);
        }

        return GetString(key, ctx);
    }

    // A method to retrieve a localized string, together with a context with available variables
    public string GetString(string key, Context context = null)
    {
        if (!_locale.ContainsKey(key))
        {
            return $"{key}_NOT_LOCALIZED";
        }

        if (context == null)
        {
            return _locale[key];
        }
        else
        {
            string value = _locale[key];
            foreach (string key2 in context.Keys)
            {
                value = value.Replace("${" + key2 + "}", context[key2]);
            }
            return value;
        }
    }
}