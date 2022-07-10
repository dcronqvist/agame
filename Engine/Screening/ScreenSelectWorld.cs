using System.Numerics;
using AGame.Engine.ECSys;
using AGame.Engine.Graphics;
using AGame.Engine.Graphics.Rendering;
using AGame.Engine.Networking;
using AGame.Engine.UI;
using AGame.Engine.World;

namespace AGame.Engine.Screening;

public class EnterSelectWorldArgs : ScreenEnterArgs
{
    public Action<WorldMetaData> OnWorldSelected { get; set; }

    public EnterSelectWorldArgs(Action<WorldMetaData> onSelect)
    {
        this.OnWorldSelected = onSelect;
    }
}

public class ScreenSelectWorld : Screen<EnterSelectWorldArgs>
{
    private Action<WorldMetaData> _onWorldSelected;

    public override void Initialize()
    {

    }

    public override void OnEnter(EnterSelectWorldArgs args)
    {
        this._onWorldSelected = args.OnWorldSelected;

        WorldManager.Instance.LoadWorlds();
    }

    public override void OnLeave()
    {
    }

    public override void Update()
    {

    }

    string _newWorldName = "";

    public override void Render()
    {
        Renderer.SetRenderTarget(null, null);
        Renderer.Clear(ColorF.Darken(ColorF.LightGray, 1.05f));

        GUI.Begin();
        WorldMetaData[] worlds = WorldManager.Instance.GetAllWorlds();

        Vector2 middleOfScreen = DisplayManager.GetWindowSizeInPixels() / 2f;
        Vector2 bottomLeft = new Vector2(0, DisplayManager.GetWindowSizeInPixels().Y);

        GUI.TextField("world name...", new Vector2(middleOfScreen.X - 100, 20), new Vector2(200, 50), ref _newWorldName);

        if (GUI.Button("Create World", new Vector2(middleOfScreen.X - 100, 90), new Vector2(200, 50)))
        {
            WorldManager.Instance.CreateNewWorld(_newWorldName, "CoolWorldGenerator");
            WorldManager.Instance.LoadWorlds();
        }

        for (int i = 0; i < worlds.Length; i++)
        {
            if (GUI.Button(worlds[i].Name + " " + worlds[i].LastPlayedAt.ToShortDateString(), new Vector2(middleOfScreen.X - 200f, middleOfScreen.Y + i * 50f), new Vector2(400f, 40f)))
            {
                this._onWorldSelected?.Invoke(worlds[i]);
            }
        }

        if (GUI.Button(Localization.GetString("menu.button.back"), new Vector2(10f, bottomLeft.Y - 50f), new Vector2(200f, 40f)))
        {
            ScreenManager.GoToScreen<ScreenMainMenu, EnterMainMenuArgs>(new EnterMainMenuArgs());
        }
        GUI.End();
    }
}