using System.Numerics;
using AGame.Engine.Assets;
using AGame.Engine.Graphics;
using AGame.Engine.Graphics.Rendering;

namespace AGame.Engine.Screening;

public class EnterTemporaryLoading : ScreenEnterArgs
{
    public string Text { get; set; }
}

public class ScreenTemporaryLoading : Screen<EnterTemporaryLoading>
{
    private string _text;

    public override void Initialize()
    {
    }

    public override void OnEnter(EnterTemporaryLoading args)
    {
        this._text = args.Text;
    }

    public override void OnLeave()
    {

    }

    public override void Render()
    {
        Renderer.SetRenderTarget(null, null);
        Renderer.Clear(ColorF.Black);

        Renderer.Text.RenderText(AssetManager.GetAsset<Font>("font_rainyhearts"), this._text, new Vector2(100, 100), 3f, ColorF.White, Renderer.Camera);
    }

    public override void Update()
    {
    }
}