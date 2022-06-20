using System.Numerics;
using AGame.Engine.Assets;
using AGame.Engine.Configuration;
using AGame.Engine.Graphics;
using AGame.Engine.Graphics.Rendering;
using AGame.Engine.UI;
using OpenTK.Audio.OpenAL;

namespace AGame.Engine.Screening;

public class ScreenMainMenu : Screen
{
    public ScreenMainMenu() : base("screen_main_menu")
    {

    }

    public override Screen Initialize()
    {
        return this;
    }

    public override void OnEnter(string[] args)
    {
    }

    public override void OnLeave()
    {

    }

    public override void Render()
    {
        Renderer.SetRenderTarget(null, null);
        Renderer.Clear(ColorF.Darken(ColorF.LightGray, 1.05f));

        GUI.Begin();
        Vector2 middleOfScreen = DisplayManager.GetWindowSizeInPixels() / 2f - new Vector2(0, 200);
        float width = 300f;
        float height = 50f;
        float distance = 10f;

        if (GUI.Button(Localization.GetString("screen.main_menu.button.singleplayer"), new Vector2(middleOfScreen.X - width / 2f, middleOfScreen.Y), new Vector2(width, height)))
        {
            ScreenManager.GoToScreen("screen_single_player");
        }

        if (GUI.Button(Localization.GetString("screen.main_menu.button.multiplayer"), new Vector2(middleOfScreen.X - width / 2f, middleOfScreen.Y + height + distance), new Vector2(width, height)))
        {
            ScreenManager.GoToScreen("screen_multiplayer");
        }

        if (GUI.Button(Localization.GetString("screen.main_menu.button.exit"), new Vector2(middleOfScreen.X - width / 2f, middleOfScreen.Y + height * 2 + distance * 2), new Vector2(width, height)))
        {
            Environment.Exit(0);
        }

        if (GUI.Button("Play Test Sound", new Vector2(middleOfScreen.X - width / 2f, middleOfScreen.Y + height * 3 + distance * 3), new Vector2(width, height)))
        {
            Audio.Play("audio_click", Utilities.GetRandomFloat(0.8f, 1.2f));
        }

        if (GUI.Button("Play Test Sound 2", new Vector2(middleOfScreen.X - width / 2f, middleOfScreen.Y + height * 4 + distance * 4), new Vector2(width, height)))
        {
            Audio.Play("audio_click_2", Utilities.GetRandomFloat(0.8f, 1.2f));
        }

        float volume = Settings.GetSetting<float>("volume_master");
        if (GUI.Slider("Master Volume", new Vector2(middleOfScreen.X - width / 2f, middleOfScreen.Y + height * 5 + distance * 5), new Vector2(width, height), ref volume))
        {
            AL.Listener(ALListenerf.Gain, volume);
            _ = Settings.SetSettingAsync("volume_master", volume);
        }

        Locale[] locales = Localization.GetAvailableLocales();
        int currentLocale = Array.IndexOf(locales, Localization.GetLocale());
        string[] localeNames = locales.Select(x => x.LocaleName).ToArray();
        if (GUI.Dropdown(localeNames, new Vector2(20, 20), new Vector2(200, 40), ref currentLocale))
        {
            Localization.SetLocale(locales[currentLocale].Name, prepend: false);
        }
        GUI.End();
    }

    public override void Update()
    {
    }
}