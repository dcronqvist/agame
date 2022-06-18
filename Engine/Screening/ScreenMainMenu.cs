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

    string host = "mc.dcronqvist.se";
    string port = "28000";

    public override void Render()
    {
        Renderer.SetRenderTarget(null, null);

        Renderer.Clear(ColorF.Darken(ColorF.LightGray, 1.05f));

        Vector2 middleOfScreen = DisplayManager.GetWindowSizeInPixels() / 2f;
        float widths = 300f;

        GUI.TextField("host...", new Vector2(middleOfScreen.X - widths / 2f, middleOfScreen.Y - 50f), new Vector2(widths, 40f), ref host);
        GUI.TextField("port...", new Vector2(middleOfScreen.X - widths / 2f, middleOfScreen.Y), new Vector2(widths, 40f), ref port);

        string connectText = Localization.GetString("screen.main_menu.button.connect");
        if (GUI.Button(connectText, new Vector2(middleOfScreen.X - widths / 2f, middleOfScreen.Y + 50f), new Vector2(widths / 2f - 5f, 40f)))
        {
            if (host != "" && port != "")
            {
                ScreenManager.GoToScreen("remotescreen", host, port);
            }
        }

        string hostButton = Localization.GetString("screen.main_menu.button.host");
        if (GUI.Button(hostButton, new Vector2(middleOfScreen.X + 5f, middleOfScreen.Y + 50f), new Vector2(widths / 2f - 5f, 40f)))
        {
            if (port != "")
            {
                ScreenManager.GoToScreen("testscreen", port);
            }
        }

        Locale[] locales = Localization.GetAvailableLocales();
        int currentLocale = Array.IndexOf(locales, Localization.GetLocale());
        string[] localeNames = locales.Select(x => x.LocaleName).ToArray();
        if (GUI.Dropdown(localeNames, new Vector2(20, 20), new Vector2(200, 40), ref currentLocale))
        {
            Localization.SetLocale(locales[currentLocale].Name, prepend: false);
        }
    }

    public override void Update()
    {
    }
}