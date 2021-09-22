using AGame.Engine.Assets;

namespace AGame.Engine.Graphics.Rendering
{
    public static class Renderer
    {
        public static TextRenderer Text { get; private set; }

        public static void Init()
        {
            Shader textRendererShader = AssetManager.GetAsset<Shader>("shader_basictext");
            Text = new TextRenderer(textRendererShader);


        }
    }
}