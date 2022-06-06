using System.Drawing;
using System.Numerics;
using AGame.Engine.Assets;
using AGame.Engine.OpenGL;
using static AGame.Engine.OpenGL.GL;

namespace AGame.Engine.Graphics.Rendering
{
    public class TextureRenderer
    {
        private Shader shader;
        private uint quadVAO;
        private uint VBO;
        private uint modelInstanceVBO;
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
            RenderInstanced(texture, new Vector2[] { position }, scale, rotation, color, origin, sourceRectangle);
        }

        public unsafe void RenderInstanced(Texture2D texture, Vector2[] positions, Vector2 scale, float rotation, ColorF color, Vector2 origin, RectangleF sourceRectangle)
        {
            float[] matrixValues = new float[positions.Length * 16];

            for (int i = 0; i < positions.Length; i++)
            {
                Matrix4x4 transPos = Matrix4x4.CreateTranslation(new Vector3(positions[i], 0.0f));
                Matrix4x4 transMid = Matrix4x4.CreateTranslation(new Vector3(origin.X * scale.X, origin.Y * scale.Y, 0.0f));
                Matrix4x4 rot = Matrix4x4.CreateRotationZ(rotation);
                Matrix4x4 transOrigin = Matrix4x4.CreateTranslation(new Vector3(-origin.X * scale.X, -origin.Y * scale.Y, 0.0f));
                Matrix4x4 mscale = Matrix4x4.CreateScale(new Vector3(new Vector2(sourceRectangle.Width, sourceRectangle.Height) * scale, 1.0f));

                float[] m = Utilities.GetMatrix4x4Values(mscale * transOrigin * rot * transMid * transPos);
                for (int j = 0; j < 16; j++)
                {
                    matrixValues[i * 16 + j] = m[j];
                }
            }

            RenderInstanced(texture, matrixValues, color, sourceRectangle);
        }

        public unsafe void RenderInstanced(Texture2D texture, float[] instancedModelMatrices, ColorF color, RectangleF sourceRectangle)
        {
            shader.Use();
            shader.SetMatrix4x4("projection", Renderer.Camera.GetProjectionMatrix());

            shader.SetVec4("textureColor", color.R, color.G, color.B, color.A);
            shader.SetInt("image", 0);

            glActiveTexture(GL_TEXTURE0);
            GLSM.BindTexture(GL_TEXTURE_2D, texture.ID);

            if (sourceRectangle != currentSourceRectangle)
            {
                glBindBuffer(GL_ARRAY_BUFFER, VBO);
                // Assign new texture coordinates based on the src rectangle.
                float[][] newTex = new float[][]
                {
                    // down left
                    new float[] { sourceRectangle.X / texture.Width, (sourceRectangle.Y + sourceRectangle.Height) / texture.Height },
                    // top right
                    new float[] { (sourceRectangle.X + sourceRectangle.Width) / texture.Width, sourceRectangle.Y / texture.Height },
                    // top left
                    new float[] { sourceRectangle.X / texture.Width, sourceRectangle.Y / texture.Height },
                    // down left
                    new float[] { sourceRectangle.X / texture.Width, (sourceRectangle.Y + sourceRectangle.Height) / texture.Height },
                    // down right
                    new float[] { (sourceRectangle.X + sourceRectangle.Width) / texture.Width, (sourceRectangle.Y + sourceRectangle.Height) / texture.Height },
                    // top right
                    new float[] { (sourceRectangle.X + sourceRectangle.Width) / texture.Width, sourceRectangle.Y / texture.Height },
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

            glBindBuffer(GL_ARRAY_BUFFER, modelInstanceVBO);
            fixed (float* m = &instancedModelMatrices[0])
            {
                glBufferData(GL_ARRAY_BUFFER, sizeof(float) * instancedModelMatrices.Length, m, GL_DYNAMIC_DRAW);
            }

            glBindBuffer(GL_ARRAY_BUFFER, 0);
            glBindVertexArray(quadVAO);
            glDrawArraysInstanced(GL_TRIANGLES, 0, 6, instancedModelMatrices.Length / 16);
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
                0.0f, 1.0f, 0.0f, 0.0f, //downLeft
                1.0f, 0.0f, 1.0f, 1.0f, //topRight
                0.0f, 0.0f, 0.0f, 1.0f, //topLeft

                0.0f, 1.0f, 0.0f, 0.0f, //downLeft
                1.0f, 1.0f, 1.0f, 0.0f, //downRight
                1.0f, 0.0f, 1.0f, 1.0f  //topRight
            };

            glBindBuffer(GL_ARRAY_BUFFER, VBO);

            fixed (float* v = &vertices[0])
            {
                glBufferData(GL_ARRAY_BUFFER, sizeof(float) * vertices.Length, v, GL_DYNAMIC_DRAW);
            }

            glEnableVertexAttribArray(0);
            glVertexAttribPointer(0, 4, GL_FLOAT, false, 4 * sizeof(float), (void*)0);

            modelInstanceVBO = glGenBuffer();
            glBindBuffer(GL_ARRAY_BUFFER, modelInstanceVBO);

            float[] m = Utilities.GetMatrix4x4Values(Matrix4x4.Identity);

            fixed (float* mat = &m[0])
            {
                glBufferData(GL_ARRAY_BUFFER, sizeof(float) * 16, mat, GL_STATIC_DRAW);
            }

            glEnableVertexAttribArray(1);
            glVertexAttribPointer(1, 4, GL_FLOAT, false, 16 * sizeof(float), (void*)0);
            glVertexAttribDivisor(1, 1);
            glEnableVertexAttribArray(2);
            glVertexAttribPointer(2, 4, GL_FLOAT, false, 16 * sizeof(float), (void*)(1 * 4 * sizeof(float)));
            glVertexAttribDivisor(2, 1);
            glEnableVertexAttribArray(3);
            glVertexAttribPointer(3, 4, GL_FLOAT, false, 16 * sizeof(float), (void*)(2 * 4 * sizeof(float)));
            glVertexAttribDivisor(3, 1);
            glEnableVertexAttribArray(4);
            glVertexAttribPointer(4, 4, GL_FLOAT, false, 16 * sizeof(float), (void*)(3 * 4 * sizeof(float)));
            glVertexAttribDivisor(4, 1);

            glBindBuffer(GL_ARRAY_BUFFER, 0);
            glBindVertexArray(0);
        }
    }
}