using System.Numerics;
using AGame.Engine.ECSys;
using AGame.Engine.Graphics;
using AGame.Engine.Graphics.Rendering;
using AGame.Engine.Networking;
using AGame.Engine.UI;
using AGame.Engine.World;

namespace AGame.Engine.Screening;

public class EnterSelectNameArgs : ScreenEnterArgs
{
    public Action<string> OnNameSelected;

    public EnterSelectNameArgs(Action<string> onNameSelected)
    {
        this.OnNameSelected = onNameSelected;
    }
}

public class ScreenSelectName : Screen<EnterSelectNameArgs>
{
    Action<string> OnNameSelected;

    public override void Initialize()
    {

    }

    public override void OnEnter(EnterSelectNameArgs args)
    {
        this.OnNameSelected = args.OnNameSelected;
    }

    public override void OnLeave()
    {
    }

    public override void Update()
    {

    }

    string _playerName = "";

    public override void Render()
    {
        Renderer.SetRenderTarget(null, null);
        Renderer.Clear(ColorF.Darken(ColorF.LightGray, 1.05f));

        GUI.Begin();

        Vector2 middleOfScreen = DisplayManager.GetWindowSizeInPixels() / 2f;
        float width = 350f;

        GUI.TextField("player name", new Vector2(middleOfScreen.X - width / 2f, middleOfScreen.Y - 50f), new Vector2(width, 100f), ref _playerName);

        if (GUI.Button("confirm", new Vector2(middleOfScreen.X - width / 2f, middleOfScreen.Y + 50f), new Vector2(width, 100f)))
        {
            if (_playerName.Length > 0)
            {
                this.OnNameSelected(_playerName);
            }
        }

        GUI.End();
    }
}