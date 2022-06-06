using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Numerics;
using System.Runtime.InteropServices;
using AGame.Engine.Assets;
using AGame.Engine.OpenGL;
using static AGame.Engine.OpenGL.GL;

namespace AGame.Engine.Graphics.Rendering
{
    class DynamicInstancedTextureRenderer
    {
        private Shader shader;
        private uint quadVAO;
        private uint quadVBO;
        private uint matricesVBO;

        public Dictionary<Matrix4x4, int> matrixToBufferIndex;
        private List<float> modelBufferArray;
        private Texture2D texture2D;
        private RectangleF sourceRectangle;

        public DynamicInstancedTextureRenderer(Shader s, Texture2D texture, RectangleF sourceRec, Matrix4x4[] initialModelMatrices)
        {
            this.shader = s;
            this.texture2D = texture;
            this.sourceRectangle = sourceRec;
            this.modelBufferArray = new List<float>();
            this.matrixToBufferIndex = new Dictionary<Matrix4x4, int>();
            AddMatrices(initialModelMatrices, false);
            InitGL(this.modelBufferArray.ToArray());
        }

        public bool HasMatrix(Matrix4x4 matrix)
        {
            return this.matrixToBufferIndex.ContainsKey(matrix);
        }

        public void AddMatrix(Matrix4x4 matrix)
        {
            AddMatrices(new Matrix4x4[] { matrix });
        }

        public void AddMatrices(Matrix4x4[] matrices, bool remap = true)
        {
            for (int i = 0; i < matrices.Length; i++)
            {
                float[] m = Utilities.GetMatrix4x4Values(matrices[i]);

                matrixToBufferIndex[matrices[i]] = modelBufferArray.Count;
                modelBufferArray.AddRange(m);
            }

            if (remap)
                this.RemapBuffer();
        }

        public Matrix4x4 GetMatrix(int modelBufferindex)
        {
            float[] arr = this.modelBufferArray.GetRange(modelBufferindex, 16).ToArray();
            return Utilities.CreateMatrix4x4FromValues(arr);
        }

        public void RemoveMatrix(int modelBufferIndex)
        {
            Matrix4x4 mat = this.GetMatrix(modelBufferIndex);
            this.RemoveMatrix(mat);
        }

        public void RemoveMatrix(Matrix4x4 matrix)
        {
            int startIndex = this.matrixToBufferIndex[matrix];
            this.modelBufferArray.RemoveRange(startIndex, 16);
            this.matrixToBufferIndex.Remove(matrix);

            Matrix4x4[] keysToReduce = this.matrixToBufferIndex.Keys.Where(x => this.matrixToBufferIndex[x] > startIndex).ToArray();

            foreach (Matrix4x4 key in keysToReduce)
            {
                this.matrixToBufferIndex[key] = this.matrixToBufferIndex[key] - 16;
            }

            this.RemapBuffer();
        }

        public unsafe void InitGL(float[] arr)
        {
            // Configure VAO, VBO
            this.quadVAO = glGenVertexArray(); // Created vertex array object
            glBindVertexArray(this.quadVAO);

            this.quadVBO = glGenBuffer();

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

            glBindBuffer(GL_ARRAY_BUFFER, this.quadVBO);

            fixed (float* v = &vertices[0])
            {
                glBufferData(GL_ARRAY_BUFFER, sizeof(float) * vertices.Length, v, GL_STATIC_DRAW);
            }

            glEnableVertexAttribArray(0);
            glVertexAttribPointer(0, 4, GL_FLOAT, false, 4 * sizeof(float), (void*)0);

            this.matricesVBO = glGenBuffer();
            glBindBuffer(GL_ARRAY_BUFFER, this.matricesVBO);

            fixed (float* mat = &arr[0])
            {
                glBufferData(GL_ARRAY_BUFFER, sizeof(float) * arr.Length, mat, GL_DYNAMIC_DRAW);
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

        private unsafe void RemapBuffer()
        {
            glBindVertexArray(quadVAO);
            glBindBuffer(GL_ARRAY_BUFFER, matricesVBO);

            fixed (float* mat = &(this.modelBufferArray.ToArray())[0])
            {
                glBufferData(GL_ARRAY_BUFFER, sizeof(float) * this.modelBufferArray.Count, mat, GL_DYNAMIC_DRAW);
            }

            glBindBuffer(GL_ARRAY_BUFFER, 0);
            glBindVertexArray(0);
        }

        public void Render(ColorF color)
        {
            shader.Use();
            shader.SetMatrix4x4("projection", Renderer.Camera.GetProjectionMatrix());

            shader.SetVec4("textureColor", color.R, color.G, color.B, color.A);
            shader.SetInt("image", 0);

            glActiveTexture(GL_TEXTURE0);
            GLSM.BindTexture(GL_TEXTURE_2D, this.texture2D.ID);

            glBindBuffer(GL_ARRAY_BUFFER, 0);
            glBindVertexArray(quadVAO);
            glDrawArraysInstanced(GL_TRIANGLES, 0, 6, this.matrixToBufferIndex.Count);
            glBindVertexArray(0);
        }
    }
}