using System.Drawing;
using System.Numerics;
using AGame.Engine.Assets;
using static AGame.Engine.OpenGL.GL;

namespace AGame.Engine.Graphics.Rendering
{
    public class TextureRenderer
    {
        private Shader shader;
        private uint quadVAO;
        private uint VBO;
        public RectangleF currentSourceRectangle;

        public TextureRenderer(Shader shader)
        {
            this.shader = shader;
            InitRenderData();
        }

        public void Render(Texture2D texture, Vector2 position, Vector2 scale, float rotation, ColorF color)
        {
            if (texture != null)
                Render(texture, position, scale, rotation, color, new Vector2(texture.Width, texture.Height) * scale / 2f, new RectangleF(0, 0, texture.Width, texture.Height));
        }

        public void Render(Texture2D texture, Vector2 position, Vector2 scale, float rotation, ColorF color, Vector2 origin)
        {
            if (texture != null)
                Render(texture, position, scale, rotation, color, origin, new Rectangle(0, 0, texture.Width, texture.Height));
        }

        public unsafe void Render(Texture2D texture, Vector2 position, Vector2 scale, float rotation, ColorF color, Vector2 origin, RectangleF sourceRectangle)
        {
            shader.Use();
            shader.SetMatrix4x4("projection", Renderer.Camera.GetProjectionMatrix());

            Matrix4x4 transPos = Matrix4x4.CreateTranslation(new Vector3(position, 0.0f));
            Matrix4x4 transMid = Matrix4x4.CreateTranslation(new Vector3(origin.X * scale.X, origin.Y * scale.Y, 0.0f));
            Matrix4x4 rot = Matrix4x4.CreateRotationZ(rotation);
            Matrix4x4 transOrigin = Matrix4x4.CreateTranslation(new Vector3(-origin.X * scale.X, -origin.Y * scale.Y, 0.0f));

            Matrix4x4 mscale = Matrix4x4.CreateScale(new Vector3(new Vector2(sourceRectangle.Width, sourceRectangle.Height) * scale, 1.0f));

            shader.SetMatrix4x4("model", mscale * transOrigin * rot * transMid * transPos);
            shader.SetVec4("textureColor", color.R, color.G, color.B, color.A);
            shader.SetInt("image", 0);

            glActiveTexture(GL_TEXTURE0);
            glBindTexture(GL_TEXTURE_2D, texture.ID);

            glBindBuffer(GL_ARRAY_BUFFER, VBO);

            if (sourceRectangle != currentSourceRectangle)
            {
                // Assign new texture coordinates based on the src rectangle.
                float[][] newTex = new float[][]
                {
                    new float[] { (float)sourceRectangle.X / texture.Width, ((float)sourceRectangle.Y + sourceRectangle.Height) / texture.Height },
                    new float[] { (float)(sourceRectangle.X + sourceRectangle.Width) / texture.Width, (float)sourceRectangle.Y / texture.Height },
                    new float[] { (float)sourceRectangle.X / texture.Width, (float)sourceRectangle.Y / texture.Height },
                    new float[] { (float)sourceRectangle.X / texture.Width, ((float)sourceRectangle.Y + sourceRectangle.Height) / texture.Height },
                    new float[] { (float)(sourceRectangle.X + sourceRectangle.Width) / texture.Width, ((float)sourceRectangle.Y + sourceRectangle.Height) / texture.Height },
                    new float[] { (float)(sourceRectangle.X + sourceRectangle.Width) / texture.Width, (float)sourceRectangle.Y / texture.Height }
                };

                fixed (float* tex1 = &newTex[0][0], tex2 = &newTex[1][0], tex3 = &newTex[2][0], tex4 = &newTex[3][0], tex5 = &newTex[4][0], tex6 = &newTex[5][0])
                {
                    glBufferSubData(GL_ARRAY_BUFFER, 2 * sizeof(float), 2 * sizeof(float), tex1);
                    glBufferSubData(GL_ARRAY_BUFFER, 6 * sizeof(float), 2 * sizeof(float), tex2);
                    glBufferSubData(GL_ARRAY_BUFFER, 10 * sizeof(float), 2 * sizeof(float), tex3);
                    glBufferSubData(GL_ARRAY_BUFFER, 14 * sizeof(float), 2 * sizeof(float), tex4);
                    glBufferSubData(GL_ARRAY_BUFFER, 18 * sizeof(float), 2 * sizeof(float), tex5);
                    glBufferSubData(GL_ARRAY_BUFFER, 22 * sizeof(float), 2 * sizeof(float), tex6);
                }
                currentSourceRectangle = sourceRectangle;
            }

            glBindBuffer(GL_ARRAY_BUFFER, 0);
            glBindVertexArray(quadVAO);
            glDrawArrays(GL_TRIANGLES, 0, 6);
            glBindVertexArray(0);
        }


        private unsafe void InitRenderData()
        {
            // Configure VAO, VBO
            quadVAO = glGenVertexArray(); // Created vertex array object
            glBindVertexArray(quadVAO);

            VBO = glGenBuffer();

            float[] vertices = { 
                // pos      // tex
                0.0f, 1.0f, 0.0f, 1.0f, //downLeft
                1.0f, 0.0f, 1.0f, 0.0f, //topRight
                0.0f, 0.0f, 0.0f, 0.0f, //topLeft

                0.0f, 1.0f, 0.0f, 1.0f, //downLeft
                1.0f, 1.0f, 1.0f, 1.0f, //downRight
                1.0f, 0.0f, 1.0f, 0.0f  //
            };

            glBindBuffer(GL_ARRAY_BUFFER, VBO);

            fixed (float* v = &vertices[0])
            {
                glBufferData(GL_ARRAY_BUFFER, sizeof(float) * vertices.Length, v, GL_DYNAMIC_DRAW);
            }

            glEnableVertexAttribArray(0);
            glVertexAttribPointer(0, 4, GL_FLOAT, false, 4 * sizeof(float), (void*)0);

            glBindBuffer(GL_ARRAY_BUFFER, 0);
            glBindVertexArray(0);
        }
    }
}