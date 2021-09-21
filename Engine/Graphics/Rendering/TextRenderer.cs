using AGame.Engine.Graphics.Shaders;
using static AGame.Engine.OpenGL.GL;
using System.Numerics;
using AGame.Engine.Graphics.Cameras;

namespace AGame.Engine.Graphics.Rendering
{
    class TextRenderer
    {
        private Shader shader;
        private uint fontVAO;
        private uint fontVBO;

        public TextRenderer(Shader shader)
        {
            this.shader = shader;
            InitRenderData();
        }

        public unsafe void InitRenderData()
        {
            // Create VAO
            fontVAO = glGenVertexArray();
            // Bind VAO
            glBindVertexArray(fontVAO);

            // Create VBO
            fontVBO = glGenBuffer();
            // BIND VBO
            glBindBuffer(GL_ARRAY_BUFFER, fontVBO);

            // Add data to VBO that is NULL, nothing
            glBufferData(GL_ARRAY_BUFFER, sizeof(float) * 6 * 4, NULL, GL_DYNAMIC_DRAW);
            // Enable the data
            glEnableVertexAttribArray(0);
            glVertexAttribPointer(0, 4, GL_FLOAT, false, 4 * sizeof(float), (void*)0);

            // Unbind VBO
            glBindBuffer(GL_ARRAY_BUFFER, 0);
            // Unbind VAO
            glBindVertexArray(0);
        }

        public unsafe void RenderText(Font f, string text, Vector2 position, float scale, ColorF color, Camera2D cam)
        {
            this.shader.Use();
            this.shader.SetMatrix4x4("projection", cam.GetProjectionMatrix());
            this.shader.SetInt("text", 0);
            this.shader.SetVec4("textColor", color.R, color.G, color.B, color.A);
            glActiveTexture(GL_TEXTURE0);
            glBindVertexArray(fontVAO);

            float x = position.X;
            float y = position.Y;

            float max = 0;

            foreach (char c in text)
            {
                FontCharacter ch = f.Characters[c];

                if (ch.Size.Y > max)
                    max = ch.Size.Y;
            }

            foreach (char c in text)
            {
                FontCharacter ch = f.Characters[c];

                float xPos = x + ch.Bearing.X * scale;
                float yPos = y + (max - ch.Bearing.Y) * scale;

                float w = ch.Size.X * scale;
                float h = ch.Size.Y * scale;

                float[] vertices = new float[]
                {
                xPos + w, yPos, 1, 0,
                xPos, yPos, 0, 0,
                xPos, yPos + h, 0, 1,


                xPos + w, yPos + h, 1, 1,
                xPos + w, yPos, 1, 0,
                xPos, yPos + h, 0, 1,
                };

                glBindTexture(GL_TEXTURE_2D, ch.TextureID);

                glBindBuffer(GL_ARRAY_BUFFER, fontVBO);

                fixed (float* vert = &vertices[0])
                {
                    glBufferSubData(GL_ARRAY_BUFFER, 0, sizeof(float) * vertices.Length, vert);
                }

                glBindBuffer(GL_ARRAY_BUFFER, 0);
                glDrawArrays(GL_TRIANGLES, 0, 6);
                x += ch.Advance * scale;
            }

            glBindVertexArray(0);
            glBindTexture(GL_TEXTURE_2D, 0);
        }
    }
}