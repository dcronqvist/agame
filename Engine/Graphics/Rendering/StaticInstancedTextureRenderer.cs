using System.Drawing;
using System.Numerics;
using AGame.Engine.Assets;
using AGame.Engine.OpenGL;
using static AGame.Engine.OpenGL.GL;

namespace AGame.Engine.Graphics.Rendering
{
    class StaticInstancedTextureRenderer
    {
        private Shader shader;
        private uint quadVAO;
        private uint quadVBO;
        private uint matricesVBO;

        private int matricesAmount;
        private Texture2D texture2D;
        private RectangleF sourceRectangle;

        public StaticInstancedTextureRenderer(Shader s, Texture2D texture, RectangleF sourceRec, Matrix4x4[] modelMatrices)
        {
            this.shader = s;
            this.texture2D = texture;
            this.sourceRectangle = sourceRec;
            this.matricesAmount = modelMatrices.Length;
            InitGL(modelMatrices);
        }

        public unsafe void InitGL(Matrix4x4[] modelMatrices)
        {
            // Configure VAO, VBO
            quadVAO = glGenVertexArray(); // Created vertex array object
            glBindVertexArray(quadVAO);

            quadVBO = glGenBuffer();

            float sourceX = this.sourceRectangle.X / this.texture2D.Width;
            float sourceY = this.sourceRectangle.Y / this.texture2D.Height;
            float sourceWidth = this.sourceRectangle.Width / this.texture2D.Width;
            float sourceHeight = this.sourceRectangle.Height / this.texture2D.Height;

            float[] vertices = { 
                // pos      // tex
                0.0f, 1.0f, sourceX, sourceY + sourceHeight, //downLeft
                1.0f, 0.0f, sourceX + sourceWidth, sourceY, //topRight
                0.0f, 0.0f, sourceX, sourceY, //topLeft

                0.0f, 1.0f, sourceX, sourceY + sourceHeight, //downLeft
                1.0f, 1.0f, sourceX + sourceWidth, sourceY + sourceHeight, //downRight
                1.0f, 0.0f, sourceX + sourceWidth, sourceY  //topRight
            };

            glBindBuffer(GL_ARRAY_BUFFER, quadVBO);

            fixed (float* v = &vertices[0])
            {
                glBufferData(GL_ARRAY_BUFFER, sizeof(float) * vertices.Length, v, GL_DYNAMIC_DRAW);
            }

            glEnableVertexAttribArray(0);
            glVertexAttribPointer(0, 4, GL_FLOAT, false, 4 * sizeof(float), (void*)0);

            matricesVBO = glGenBuffer();
            glBindBuffer(GL_ARRAY_BUFFER, matricesVBO);

            float[] matrixValues = new float[16 * modelMatrices.Length];

            for (int i = 0; i < modelMatrices.Length; i++)
            {
                float[] m = Utilities.GetMatrix4x4Values(modelMatrices[i]);
                for (int j = 0; j < 16; j++)
                {
                    matrixValues[i * 16 + j] = m[j];
                }
            }


            fixed (float* mat = &matrixValues[0])
            {
                glBufferData(GL_ARRAY_BUFFER, sizeof(float) * matrixValues.Length, mat, GL_STATIC_DRAW);
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

        public void Render(Texture2D texture, ColorF color)
        {
            shader.Use();
            shader.SetMatrix4x4("projection", Renderer.Camera.GetProjectionMatrix());

            shader.SetVec4("textureColor", color.R, color.G, color.B, color.A);
            shader.SetInt("image", 0);

            glActiveTexture(GL_TEXTURE0);
            GLSM.BindTexture(GL_TEXTURE_2D, texture.ID);

            glBindBuffer(GL_ARRAY_BUFFER, 0);
            glBindVertexArray(quadVAO);
            glDrawArraysInstanced(GL_TRIANGLES, 0, 6, this.matricesAmount);
            glBindVertexArray(0);
        }
    }
}