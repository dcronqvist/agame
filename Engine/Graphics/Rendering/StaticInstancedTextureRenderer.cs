using System.Collections.Generic;
using System.Drawing;
using System.Numerics;
using AGame.Engine.Assets;
using AGame.Engine.OpenGL;
using static AGame.Engine.OpenGL.GL;

namespace AGame.Engine.Graphics.Rendering;

class StaticInstancedTextureRenderer
{
    private Shader shader;
    private uint quadVAO;

    private uint quadVBO;
    private uint matricesVBO;
    private uint uvVBO;

    private int matricesAmount;
    private Texture2D texture2D;
    private List<InstancingInfo> instances;

    public StaticInstancedTextureRenderer(Shader s, Texture2D texture, IEnumerable<InstancingInfo> instances)
    {
        this.shader = s;
        this.texture2D = texture;
        this.instances = new List<InstancingInfo>(instances);
        if (this.instances.Count > 0)
        {
            InitGL(this.instances);
        }
    }

    public unsafe void InitGL(List<InstancingInfo> instances)
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

        float[] matrixValues = new float[16 * instances.Count];
        float[] instanceUVData = new float[instances.Count * 2 * 6]; // 2 floats per vertex, 6 vertices per quad

        for (int i = 0; i < instances.Count; i++)
        {
            float[] m = instances[i].GetModelMatrixData();
            for (int j = 0; j < 16; j++)
            {
                matrixValues[i * 16 + j] = m[j];
            }
            float[] instanceVertexData = instances[i].GetUVCoordinateData(this.texture2D);
            for (int j = 0; j < instanceVertexData.Length; j++)
            {
                instanceUVData[i * instanceVertexData.Length + j] = instanceVertexData[j];
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

        uvVBO = glGenBuffer();
        glBindBuffer(GL_ARRAY_BUFFER, uvVBO);

        fixed (float* uv = &instanceUVData[0])
        {
            glBufferData(GL_ARRAY_BUFFER, sizeof(float) * instanceUVData.Length, uv, GL_STATIC_DRAW);
        }

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

        glBindBuffer(GL_ARRAY_BUFFER, 0);
        glBindVertexArray(0);
    }

    public void Render(ColorF color)
    {
        if (this.instances.Count > 0)
        {
            shader.Use();
            shader.SetMatrix4x4("projection", Renderer.Camera.GetProjectionMatrix());

            shader.SetVec4("textureColor", color.R, color.G, color.B, color.A);
            shader.SetInt("image", 0);

            glActiveTexture(GL_TEXTURE0);
            GLSM.BindTexture(GL_TEXTURE_2D, this.texture2D.ID);

            glBindBuffer(GL_ARRAY_BUFFER, 0);
            glBindVertexArray(quadVAO);
            glDrawArraysInstanced(GL_TRIANGLES, 0, 6, this.instances.Count);
            glBindVertexArray(0);
        }
    }
}
