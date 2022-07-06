using System;
using System.Diagnostics;
using System.IO;
using AGame.Engine.Graphics.Rendering;
using System.Text.Json;

namespace AGame.Engine.Assets;

class LocaleLoader : IAssetLoader
{
    public string AssetPrefix()
    {
        return "locale";
    }

    public Asset LoadAsset(Stream fileStream)
    {
        using (StreamReader sr = new StreamReader(fileStream))
        {
            string text = sr.ReadToEnd();

            Dictionary<string, string> locale = JsonSerializer.Deserialize<Dictionary<string, string>>(text, new JsonSerializerOptions()
            {
                ReadCommentHandling = JsonCommentHandling.Skip,
                AllowTrailingCommas = true
            });

            return new Locale(locale["locale_name"], locale);
        }
    }
}
