using AGame.Engine.Assets;
using AGame.Engine.Graphics.Cameras;
using static AGame.Engine.OpenGL.GL;

namespace AGame.Engine.Graphics.Rendering
{
    public static class Renderer
    {
        public static TextRenderer Text { get; private set; }
        public static ColorF ClearColor { get; set; }
        public static Camera2D Camera { get; set; }

        private static Camera2D defaultCamera;
        private static Shader renderTextureShader;
        private static RenderTexture renderTarget;

        public static void Init()
        {
            Shader textRendererShader = AssetManager.GetAsset<Shader>("shader_basictext");

            Text = new TextRenderer(textRendererShader);

            defaultCamera = new Camera2D(DisplayManager.GetWindowSizeInPixels() / 2.0f, 1.0f);

            ClearColor = ColorF.Black;

            renderTextureShader = AssetManager.GetAsset<Shader>("shader_render_texture");
        }

        public static void SetRenderTarget(RenderTexture renderTexture, Camera2D camera2D)
        {
            if (renderTexture != null)
            {
                renderTarget = renderTexture;

                glBindFramebuffer(GL_FRAMEBUFFER, renderTarget.framebuffer);
            }
            else
            {
                glBindFramebuffer(GL_FRAMEBUFFER, 0);
            }

            if (camera2D != null)
            {
                Camera = camera2D;
            }
            else
            {
                Camera = defaultCamera;
            }
        }

        public static void Clear()
        {
            Clear(ClearColor);
        }

        public static void Clear(ColorF color)
        {
            glClearColor(color.R, color.G, color.B, color.A);
            glClear(GL_COLOR_BUFFER_BIT);
        }

        public static void RenderRenderTexture(RenderTexture renderTexture)
        {
            RenderRenderTexture(renderTexture, renderTextureShader);
        }

        public static void RenderRenderTexture(RenderTexture renderTexture, Shader s)
        {
            // Use the shader
            s.Use();
            s.SetInt("renderTexture", 0); // Set to GL_TEXTURE0 unit    

            // Make correct texture active
            glActiveTexture(GL_TEXTURE0);

            // Bind the texture
            glBindTexture(GL_TEXTURE_2D, renderTexture.renderedTexture);

            // renderTexture VAO, that has simple quad
            glBindVertexArray(renderTexture.quadVao);
            // Draw the texture and unbind
            glDrawArrays(GL_TRIANGLES, 0, 6);
            glBindVertexArray(0);
        }
    }
}