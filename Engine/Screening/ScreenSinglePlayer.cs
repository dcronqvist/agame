using System.Numerics;
using AGame.Engine.Graphics;
using AGame.Engine.Graphics.Rendering;
using AGame.Engine.UI;
using AGame.Engine.World;

namespace AGame.Engine.Screening;

public class ScreenSinglePlayer : Screen
{
    public ScreenSinglePlayer() : base("screen_single_player")
    {

    }

    public override Screen Initialize()
    {
        return this;
    }

    public override void OnEnter(string[] args)
    {
        WorldManager.Instance.LoadWorlds();
    }

    public override void OnLeave()
    {
    }

    public override void Update()
    {

    }

    public override void Render()
    {
        Renderer.SetRenderTarget(null, null);
        Renderer.Clear(ColorF.Darken(ColorF.LightGray, 1.05f));

        GUI.Begin();
        WorldMetaData[] worlds = WorldManager.Instance.GetAllWorlds();

        Vector2 middleOfScreen = DisplayManager.GetWindowSizeInPixels() / 2f;
        Vector2 bottomLeft = new Vector2(0, DisplayManager.GetWindowSizeInPixels().Y);

        for (int i = 0; i < worlds.Length; i++)
        {
            if (GUI.Button(worlds[i].Name + " " + worlds[i].CreatedAt.ToShortDateString(), new Vector2(middleOfScreen.X - 200f, middleOfScreen.Y + i * 50f), new Vector2(400f, 40f)))
            {
                WorldManager.Instance.SinglePlayWorld(worlds[i]);
            }
        }

        if (GUI.Button(Localization.GetString("menu.button.back"), new Vector2(10f, bottomLeft.Y - 50f), new Vector2(200f, 40f)))
        {
            ScreenManager.GoToScreen("screen_main_menu");
        }
        GUI.End();
    }
}