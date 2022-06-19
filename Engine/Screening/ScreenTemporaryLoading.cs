using System.Numerics;
using AGame.Engine.Assets;
using AGame.Engine.Graphics;
using AGame.Engine.Graphics.Rendering;

namespace AGame.Engine.Screening;

public class ScreenTemporaryLoading : Screen
{
    public ScreenTemporaryLoading() : base("screen_temporary_loading")
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
        Renderer.Clear(ColorF.Black);

        string text = "loading...";

        Renderer.Text.RenderText(AssetManager.GetAsset<Font>("font_rainyhearts"), text, new Vector2(100, 100), 3f, ColorF.White, Renderer.Camera);
    }

    public override void Update()
    {
    }
}