using System.Numerics;
using AGame.Engine.Assets;
using AGame.Engine.Graphics;
using AGame.Engine.Graphics.Rendering;
using AGame.Engine.UI;

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
        Vector2 middleOfScreen = DisplayManager.GetWindowSizeInPixels() / 2f;
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