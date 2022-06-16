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

        if (GUI.Button("connect", new Vector2(middleOfScreen.X - widths / 2f, middleOfScreen.Y + 50f), new Vector2(widths / 2f - 5f, 40f)))
        {
            if (host != "" && port != "")
            {
                ScreenManager.GoToScreen("remotescreen", host, port);
            }
        }

        if (GUI.Button("host", new Vector2(middleOfScreen.X + 5f, middleOfScreen.Y + 50f), new Vector2(widths / 2f - 5f, 40f)))
        {
            if (port != "")
            {
                ScreenManager.GoToScreen("testscreen", port);
            }
        }
    }
}