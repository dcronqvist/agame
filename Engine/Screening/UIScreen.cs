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

    public override void Render()
    {
        Renderer.SetRenderTarget(null, null);

        Renderer.Clear(new ColorF(val, 0f, 0f, 1f));

        GUI.Begin();

        if (GUI.Button(new Vector2(100, 100), new Vector2(200, 100)))
        {
            Console.WriteLine("PRESSED");
        }

        if (GUI.Slider(new Vector2(100, 220), new Vector2(200, 30), ref val))
        {
            Console.WriteLine("SLIDER is now " + val);
        }

        if (GUI.Button(new Vector2(100, 270), new Vector2(200, 100)))
        {
            Console.WriteLine("PRESSED 2");
        }


        GUI.End();
    }
}