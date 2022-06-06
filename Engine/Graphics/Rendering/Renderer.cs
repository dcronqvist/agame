using System.Drawing;
using System.Numerics;
using AGame.Engine.Assets;
using AGame.Engine.Graphics.Cameras;
using AGame.Engine.OpenGL;
using static AGame.Engine.OpenGL.GL;

namespace AGame.Engine.Graphics.Rendering
{
    public static class Renderer
    {
        public static TextRenderer Text { get; private set; }
        public static TextureRenderer Texture { get; private set; }
        public static PrimitiveRenderer Primitive { get; private set; }

        public static ColorF ClearColor { get; set; }
        public static Camera2D Camera { get; set; }

        public static Camera2D DefaultCamera;
        private static Shader renderTextureShader;
        private static RenderTexture renderTarget;

        public static void Init()
        {
            Shader textRendererShader = AssetManager.GetAsset<Shader>("shader_basictext");
            Text = new TextRenderer(textRendererShader);
            Shader textureShader = AssetManager.GetAsset<Shader>("shader_texture");
            Texture = new TextureRenderer(textureShader);
            Shader primitiveShader = AssetManager.GetAsset<Shader>("shader_primitives");
            Primitive = new PrimitiveRenderer(primitiveShader);

            DefaultCamera = new Camera2D(DisplayManager.GetWindowSizeInPixels() / 2.0f, 1.0f);
            Camera = DefaultCamera;

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
                renderTarget = null;
            }

            if (camera2D != null)
            {
                Camera = camera2D;
            }
            else
            {
                Camera = DefaultCamera;
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
            RenderRenderTexture(renderTexture, Vector2.Zero, Vector2.Zero, Vector2.One, 0.0f, ColorF.White, renderTextureShader);
        }

        public static void RenderRenderTexture(RenderTexture r1, RenderTexture r2, Vector2 pos, Vector2 origin, Vector2 scale, float rotation, ColorF color, Shader s)
        {
            // Use the shader
            s.Use();
            s.SetInt("renderTexture0", 0); // Set to GL_TEXTURE0 unit
            s.SetInt("renderTexture1", 1);
            s.SetMatrix4x4("projection", Camera.GetProjectionMatrix());

            Matrix4x4 transPos = Matrix4x4.CreateTranslation(new Vector3(pos, 0.0f));
            Matrix4x4 transMid = Matrix4x4.CreateTranslation(new Vector3(origin.X * scale.X, origin.Y * scale.Y, 0.0f));
            Matrix4x4 rot = Matrix4x4.CreateRotationZ(rotation);
            Matrix4x4 transOrigin = Matrix4x4.CreateTranslation(new Vector3(-origin.X * scale.X, -origin.Y * scale.Y, 0.0f));

            Matrix4x4 mscale = Matrix4x4.CreateScale(new Vector3(new Vector2(r1.Width * scale.X, r1.Height * scale.Y), 1.0f));

            s.SetMatrix4x4("model", mscale * transOrigin * rot * transMid * transPos);
            s.SetVec4("textureColor", color.R, color.G, color.B, color.A);

            // Make correct texture active
            glActiveTexture(GL_TEXTURE0);

            // Bind the texture
            GLSM.BindTexture(GL_TEXTURE_2D, r1.renderedTexture);

            glActiveTexture(GL_TEXTURE1);
            GLSM.BindTexture(GL_TEXTURE_2D, r2.renderedTexture);

            // renderTexture VAO, that has simple quad
            glBindVertexArray(r1.quadVao);
            // Draw the texture and unbind
            glDrawArrays(GL_TRIANGLES, 0, 6);
            glBindVertexArray(0);
        }

        public static void RenderRenderTexture(RenderTexture renderTexture, Vector2 pos, Vector2 origin, Vector2 scale, float rotation, ColorF color, Shader s)
        {
            // Use the shader
            s.Use();
            s.SetInt("renderTexture", 0); // Set to GL_TEXTURE0 unit    
            s.SetMatrix4x4("projection", Camera.GetProjectionMatrix());

            Matrix4x4 transPos = Matrix4x4.CreateTranslation(new Vector3(pos, 0.0f));
            Matrix4x4 transMid = Matrix4x4.CreateTranslation(new Vector3(origin.X * scale.X, origin.Y * scale.Y, 0.0f));
            Matrix4x4 rot = Matrix4x4.CreateRotationZ(rotation);
            Matrix4x4 transOrigin = Matrix4x4.CreateTranslation(new Vector3(-origin.X * scale.X, -origin.Y * scale.Y, 0.0f));

            Matrix4x4 mscale = Matrix4x4.CreateScale(new Vector3(new Vector2(renderTexture.Width * scale.X, renderTexture.Height * scale.Y), 1.0f));

            s.SetMatrix4x4("model", mscale * transOrigin * rot * transMid * transPos);
            s.SetVec4("textureColor", color.R, color.G, color.B, color.A);

            // Make correct texture active
            glActiveTexture(GL_TEXTURE0);

            // Bind the texture
            GLSM.BindTexture(GL_TEXTURE_2D, renderTexture.renderedTexture);

            // renderTexture VAO, that has simple quad
            glBindVertexArray(renderTexture.quadVao);
            // Draw the texture and unbind
            glDrawArrays(GL_TRIANGLES, 0, 6);
            glBindVertexArray(0);
        }
    }
}