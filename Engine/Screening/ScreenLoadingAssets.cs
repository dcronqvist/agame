using System.Numerics;
using AGame.Engine.Assets;
using AGame.Engine.Assets.Scripting;
using AGame.Engine.DebugTools;
using AGame.Engine.Graphics;
using AGame.Engine.Graphics.Rendering;
using AGame.Engine.World;

namespace AGame.Engine.Screening;

public class EnterLoadingAssetsArgs : ScreenEnterArgs
{
    public string FinalCoreAsset { get; set; }
}

public class ScreenLoadingAssets : Screen<EnterLoadingAssetsArgs>
{
    private string _windowIconTexture = "tex_marsdirt";
    private string _currentLoadingAsset = "";
    private bool _allAssetsLoaded = false;

    public override void Initialize()
    {

    }

    public override void OnEnter(EnterLoadingAssetsArgs args)
    {
        _currentLoadingAsset = Localization.GetString("screen.loading_assets.subtitle", ("asset_name", args.FinalCoreAsset)); // Get the last loaded texture from ImplGame
        DisplayManager.SetWindowIcon(AssetManager.GetAsset<Texture2D>(_windowIconTexture));

        AssetManager.OnAssetStartLoad += (sender, e) =>
        {
            _currentLoadingAsset = Localization.GetString("screen.loading_assets.subtitle", ("asset_name", e));
        };

        AssetManager.OnAllAssetsLoaded += (sender, e) =>
        {
            _allAssetsLoaded = true;
        };
    }

    public override void OnLeave()
    {

    }

    public override void Update()
    {
        if (_allAssetsLoaded)
        {
            Audio.Init();
            AssetManager.FinalizeAssets();
            ScreenManager.GoToScreen<ScreenMainMenu, EnterMainMenuArgs>(new EnterMainMenuArgs());
        }
    }

    public override void Render()
    {
        Renderer.Clear(ColorF.Black);
        Font coreFont = AssetManager.GetAsset<Font>("font_rainyhearts");

        Vector2 middleOfScreen = DisplayManager.GetWindowSizeInPixels() / 2.0f;

        string topText = Localization.GetString("screen.loading_assets.title");
        Vector2 topTextPos = (middleOfScreen + new Vector2(0, -50) - coreFont.MeasureString(topText, 1.0f) / 2.0f).Round();
        Renderer.Text.RenderText(coreFont, topText, topTextPos, 1.0f, ColorF.White, Renderer.Camera);

        int loadbarLength = 80;
        int hashtagAmount = (int)(AssetManager.LoadedPercentage * loadbarLength);
        string hashtags = "#".Repeat(hashtagAmount);
        string loadBar = hashtags + "_".Repeat(loadbarLength - hashtagAmount);
        Vector2 loadBarPos = (middleOfScreen - coreFont.MeasureString(loadBar, 1.0f) / 2.0f).Round();
        Renderer.Text.RenderText(coreFont, loadBar, loadBarPos, 1.0f, ColorF.White, Renderer.Camera);

        Vector2 assetNamePos = (middleOfScreen + new Vector2(0, 50) - coreFont.MeasureString(_currentLoadingAsset, 1.0f) / 2.0f).Round();
        Renderer.Text.RenderText(coreFont, _currentLoadingAsset, assetNamePos, 1.0f, ColorF.White, Renderer.Camera);
    }
}