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
        private uint uvVBO;
        private uint existsVBO;

        private int matricesAmount;
        private Texture2D texture2D;
        private List<InstancingInfo> instances;

        private Dictionary<InstancingInfo, int> _instanceToIndex;
        private int _capacity;

        private List<float> _matrixVBOData;
        private List<float> _uvVBOData;
        private List<bool> _existsData;

        public DynamicInstancedTextureRenderer(Shader s, Texture2D texture, IEnumerable<InstancingInfo> instances = null)
        {
            this.shader = s;
            this.texture2D = texture;
            this.instances = instances == null ? new List<InstancingInfo>() : instances.ToList();
            this._instanceToIndex = new Dictionary<InstancingInfo, int>();
            this._capacity = 128;
            this._matrixVBOData = Utilities.NValues(0.0f, this._capacity * 16);
            this._uvVBOData = Utilities.NValues(0.0f, this._capacity * 12);
            this._existsData = Utilities.NValues(false, this._capacity);

            this.InitGL();
            this.UpdateVBOCapacities(this._capacity);
        }

        private unsafe void UpdateVBOCapacities(int capacity)
        {
            glBindVertexArray(quadVAO);
            glBindBuffer(GL_ARRAY_BUFFER, matricesVBO);

            // Update matrix VBO capacity, 16 floats per matrix
            float[] data = new float[capacity * 16];

            for (int i = 0; i < capacity * 16; i++)
            {
                if (i < _matrixVBOData.Count)
                {
                    data[i] = _matrixVBOData[i];
                }
                else
                {
                    data[i] = 0f;
                }
            }

            this._matrixVBOData = data.ToList();

            fixed (float* f = &data[0])
            {
                glBufferData(GL_ARRAY_BUFFER, data.Length * sizeof(float), f, GL_STREAM_DRAW);
            }

            // Update UV VBO capacity, 12 floats per UV and instance
            data = new float[capacity * 12];

            for (int i = 0; i < capacity * 12; i++)
            {
                if (i < _uvVBOData.Count)
                {
                    data[i] = _uvVBOData[i];
                }
                else
                {
                    data[i] = 0f;
                }
            }

            this._uvVBOData = data.ToList();

            glBindBuffer(GL_ARRAY_BUFFER, uvVBO);

            fixed (float* f = &data[0])
            {
                glBufferData(GL_ARRAY_BUFFER, data.Length * sizeof(float), f, GL_STREAM_DRAW);
            }

            // Update exists capacity
            float[] exists = new float[capacity];
            for (int i = 0; i < capacity; i++)
            {
                if (i < _existsData.Count)
                {
                    exists[i] = _existsData[i] ? 1f : 0f;
                }
                else
                {
                    exists[i] = 0f;
                }
            }

            this._existsData = exists.Select(x => x == 1f ? true : false).ToList();

            glBindBuffer(GL_ARRAY_BUFFER, existsVBO);

            fixed (float* f = &exists[0])
            {
                glBufferData(GL_ARRAY_BUFFER, exists.Length * sizeof(float), f, GL_STREAM_DRAW);
            }

            glBindVertexArray(0);
        }

        private unsafe void SetInstanceExists(int startIndex, InstancingInfo info)
        {
            this._instanceToIndex[info] = startIndex;

            glBindVertexArray(quadVAO);

            glBindBuffer(GL_ARRAY_BUFFER, matricesVBO);
            float[] data = info.GetModelMatrixData();

            for (int i = 0; i < data.Length; i++)
            {
                int offset = startIndex * 16 + i;
                _matrixVBOData[offset] = data[i];
            }

            fixed (float* f = &data[0])
            {
                glBufferSubData(GL_ARRAY_BUFFER, startIndex * sizeof(float) * 16, data.Length * sizeof(float), f);
            }

            glBindBuffer(GL_ARRAY_BUFFER, uvVBO);
            data = info.GetUVCoordinateData(this.texture2D);

            for (int i = 0; i < data.Length; i++)
            {
                int offset = startIndex * 12 + i;
                _uvVBOData[offset] = data[i];
            }

            fixed (float* f = &data[0])
            {
                glBufferSubData(GL_ARRAY_BUFFER, startIndex * sizeof(float) * 12, data.Length * sizeof(float), f);
            }

            glBindBuffer(GL_ARRAY_BUFFER, existsVBO);
            float[] exists = new float[1] { 1.0f };

            this._existsData[startIndex] = true;

            fixed (float* f = &exists[0])
            {
                glBufferSubData(GL_ARRAY_BUFFER, startIndex * sizeof(float), sizeof(float), f);
            }

            glBindVertexArray(0);
        }

        private unsafe void SetInstanceNotExist(int index)
        {
            glBindVertexArray(quadVAO);

            glBindBuffer(GL_ARRAY_BUFFER, existsVBO);

            float[] exists = new float[1] { 0.0f };

            this._existsData[index] = false;

            fixed (float* f = &exists[0])
            {
                glBufferSubData(GL_ARRAY_BUFFER, index * sizeof(float), sizeof(float), f);
            }

            glBindVertexArray(0);
        }

        public int GetNextAvailableIndex()
        {
            int index = this._existsData.FindIndex(0, x => x == false);

            if (index == -1)
            {
                int oldCap = this._capacity;
                this._capacity = this._capacity * 2;
                this.UpdateVBOCapacities(this._capacity);
                return oldCap + 1;
            }

            return index;
        }

        public void AddInstances(params InstancingInfo[] instances)
        {
            foreach (InstancingInfo ii in instances)
            {
                int index = this.GetNextAvailableIndex();
                this.SetInstanceExists(index, ii);
                this.instances.Add(ii);
            }
        }

        public void RemoveInstances(params InstancingInfo[] instances)
        {
            foreach (InstancingInfo ii in instances)
            {
                int index = this._instanceToIndex[ii];
                this.SetInstanceNotExist(index);
                this.instances.Remove(ii);
            }
        }

        public unsafe void InitGL()
        {
            // Configure VAO, VBO
            quadVAO = glGenVertexArray(); // Created vertex array object
            glBindVertexArray(quadVAO);

            quadVBO = glGenBuffer(); // Created vertex buffer object
            float[] quadVertices = {
                // pos     
                0.0f, 1.0f, // downLeft
                1.0f, 0.0f, // topRight
                0.0f, 0.0f, // topLeft

                0.0f, 1.0f, // downLeft
                1.0f, 1.0f, // downRight
                1.0f, 0.0f, // topRight
            };

            glBindBuffer(GL_ARRAY_BUFFER, quadVBO);

            fixed (float* v = &quadVertices[0])
            {
                glBufferData(GL_ARRAY_BUFFER, sizeof(float) * quadVertices.Length, v, GL_STATIC_DRAW);
            }

            glEnableVertexAttribArray(0);
            glVertexAttribPointer(0, 2, GL_FLOAT, false, 2 * sizeof(float), (void*)0);

            matricesVBO = glGenBuffer();
            glBindBuffer(GL_ARRAY_BUFFER, matricesVBO);

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

            uvVBO = glGenBuffer();
            glBindBuffer(GL_ARRAY_BUFFER, uvVBO);

            glEnableVertexAttribArray(5);
            glVertexAttribPointer(5, 2, GL_FLOAT, false, 12 * sizeof(float), (void*)0);
            glVertexAttribDivisor(5, 1);
            glEnableVertexAttribArray(6);
            glVertexAttribPointer(6, 2, GL_FLOAT, false, 12 * sizeof(float), (void*)(1 * 2 * sizeof(float)));
            glVertexAttribDivisor(6, 1);
            glEnableVertexAttribArray(7);
            glVertexAttribPointer(7, 2, GL_FLOAT, false, 12 * sizeof(float), (void*)(2 * 2 * sizeof(float)));
            glVertexAttribDivisor(7, 1);
            glEnableVertexAttribArray(8);
            glVertexAttribPointer(8, 2, GL_FLOAT, false, 12 * sizeof(float), (void*)(3 * 2 * sizeof(float)));
            glVertexAttribDivisor(8, 1);
            glEnableVertexAttribArray(9);
            glVertexAttribPointer(9, 2, GL_FLOAT, false, 12 * sizeof(float), (void*)(4 * 2 * sizeof(float)));
            glVertexAttribDivisor(9, 1);
            glEnableVertexAttribArray(10);
            glVertexAttribPointer(10, 2, GL_FLOAT, false, 12 * sizeof(float), (void*)(5 * 2 * sizeof(float)));
            glVertexAttribDivisor(10, 1);

            existsVBO = glGenBuffer();
            glBindBuffer(GL_ARRAY_BUFFER, existsVBO);

            glEnableVertexAttribArray(11);
            glVertexAttribPointer(11, 1, GL_FLOAT, false, sizeof(float), (void*)0);
            glVertexAttribDivisor(11, 1);

            glBindBuffer(GL_ARRAY_BUFFER, 0);
            glBindVertexArray(0);
        }

        // public unsafe void RemapBuffer()
        // {
        //     glBindVertexArray(quadVAO);
        //     glBindBuffer(GL_ARRAY_BUFFER, matricesVBO);

        //     float[] matrixValues = new float[16 * instances.Count];
        //     float[] instanceUVData = new float[instances.Count * 2 * 6]; // 2 floats per vertex, 6 vertices per quad

        //     for (int i = 0; i < instances.Count; i++)
        //     {
        //         float[] m = instances[i].GetModelMatrixData();
        //         for (int j = 0; j < 16; j++)
        //         {
        //             matrixValues[i * 16 + j] = m[j];
        //         }
        //         float[] instanceVertexData = instances[i].GetUVCoordinateData(this.texture2D);
        //         for (int j = 0; j < instanceVertexData.Length; j++)
        //         {
        //             instanceUVData[i * instanceVertexData.Length + j] = instanceVertexData[j];
        //         }
        //     }

        //     fixed (float* mat = &matrixValues[0])
        //     {
        //         glBufferData(GL_ARRAY_BUFFER, sizeof(float) * matrixValues.Length, mat, GL_STREAM_DRAW);
        //     }

        //     glBindBuffer(GL_ARRAY_BUFFER, uvVBO);

        //     fixed (float* uv = &instanceUVData[0])
        //     {
        //         glBufferData(GL_ARRAY_BUFFER, sizeof(float) * instanceUVData.Length, uv, GL_STREAM_DRAW);
        //     }

        //     glBindBuffer(GL_ARRAY_BUFFER, 0);
        //     glBindVertexArray(0);
        // }

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
            glDrawArraysInstanced(GL_TRIANGLES, 0, 6, this._capacity);
            glBindVertexArray(0);
        }
    }
}