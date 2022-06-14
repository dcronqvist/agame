using System;
using System.Drawing;
using System.Numerics;
using AGame.Engine;
using AGame.Engine.Assets;
using AGame.Engine.DebugTools;
using AGame.Engine.ECSys;
using AGame.Engine.ECSys.Components;
using AGame.Engine.GLFW;
using AGame.Engine.Graphics;
using AGame.Engine.Graphics.Cameras;
using AGame.Engine.Graphics.Rendering;
using AGame.Engine.Networking;
using AGame.Engine.Screening;
using AGame.Engine.UI;
using AGame.Engine.World;

namespace AGame.Engine.Screening;

class UIScreen : Screen
{
    public UIScreen() : base("uiscreen")
    {

    }

    public override Screen Initialize()
    {
        GUI.Init();
        return this;
    }

    public override async void OnEnter(string[] args)
    {

    }

    public override async void OnLeave()
    {

    }

    public override void Update()
    {

    }

    float val = 0f;
    ColorF col = new ColorF(0x4A9077, 255);
    string text = "";

    public override void Render()
    {
        Renderer.SetRenderTarget(null, null);

        Renderer.Clear(new ColorF(val, 0f, 0f, 1f));

        GUI.Begin();

        if (GUI.Button("Press 1", new Vector2(100, 100), new Vector2(150, 40)))
        {
            Console.WriteLine("PRESSED");
        }

        if (GUI.Button("testscreen", new Vector2(100, 150), new Vector2(150, 40)))
        {
            ScreenManager.GoToScreen("testscreen");
        }

        if (GUI.Slider("Slide 1", new Vector2(100, 200), new Vector2(150, 40), ref val))
        {
            Console.WriteLine("SLIDER is now " + val);
        }

        if (GUI.TextField("name...", new Vector2(100, 250), new Vector2(150f, 40), ref text))
        {
            Console.WriteLine("TEXT FIELD is now " + text);
        }

        Renderer.Text.RenderText(AssetManager.GetAsset<Font>("font_rainyhearts"), GUI._hotID.ToString(), new Vector2(400, 50), 1f, ColorF.White, Renderer.Camera);
        Renderer.Text.RenderText(AssetManager.GetAsset<Font>("font_rainyhearts"), GUI._activeID.ToString(), new Vector2(400, 70), 1f, ColorF.White, Renderer.Camera);
        Renderer.Text.RenderText(AssetManager.GetAsset<Font>("font_rainyhearts"), GUI._kbdFocusID.ToString(), new Vector2(400, 90), 1f, ColorF.White, Renderer.Camera);


        GUI.End();
    }
}