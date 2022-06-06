using static AGame.Engine.OpenGL.GL;
using System.Numerics;
using AGame.Engine.Graphics.Cameras;
using AGame.Engine.Assets;
using System.Collections.Generic;
using AGame.Engine.OpenGL;

namespace AGame.Engine.Graphics.Rendering
{
    public class TextRenderer
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

        public unsafe void RenderText(Font f, string s, Vector2 position, float scale, ColorF color, Camera2D cam, bool doFormatting = false)
        {
            if (doFormatting)
            {
                RenderText(f, new FormattedText(s), position, scale, color, cam);
                return;
            }

            this.shader.Use();
            this.shader.SetMatrix4x4("projection", cam.GetProjectionMatrix());

            Matrix4x4 transPos = Matrix4x4.CreateTranslation(new Vector3(position, 0.0f));
            Matrix4x4 mscale = Matrix4x4.CreateScale(scale);

            shader.SetMatrix4x4("model", mscale * transPos);

            this.shader.SetInt("text", 0);
            this.shader.SetVec4("textColor", color.R, color.G, color.B, color.A);

            glActiveTexture(GL_TEXTURE0);
            glBindVertexArray(fontVAO);

            float x = position.X;
            float y = position.Y;

            foreach (char c in s)
            {
                FontCharacter ch = f.Characters[c];

                float xPos = x + ch.Bearing.X * scale;
                float yPos = y + (f.MaxY - ch.Bearing.Y) * scale;

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

                GLSM.BindTexture(GL_TEXTURE_2D, ch.TextureID);

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
            GLSM.BindTexture(GL_TEXTURE_2D, 0);
        }

        public unsafe void RenderText(Font f, FormattedText ft, Vector2 position, float scale, ColorF color, Camera2D cam)
        {
            this.shader.Use();
            this.shader.SetMatrix4x4("projection", cam.GetProjectionMatrix());

            Matrix4x4 transPos = Matrix4x4.CreateTranslation(new Vector3(position, 0.0f));
            Matrix4x4 mscale = Matrix4x4.CreateScale(scale);

            shader.SetMatrix4x4("model", mscale * transPos);

            this.shader.SetInt("text", 0);
            glActiveTexture(GL_TEXTURE0);
            glBindVertexArray(fontVAO);

            float x = position.X;
            float y = position.Y;

            FormattedText.FTToken[] tokens = ft.PerformFormatting();
            Stack<ColorF> colorStack = new Stack<ColorF>();
            colorStack.Push(color);

            foreach (FormattedText.FTToken token in tokens)
            {
                if (token.type == FormattedText.FTTokenType.Text)
                {
                    ColorF col = colorStack.Peek();
                    this.shader.SetVec4("textColor", col.R, col.G, col.B, col.A);
                    foreach (char c in token.value)
                    {
                        FontCharacter ch = f.Characters[c];

                        float xPos = x + ch.Bearing.X * scale;
                        float yPos = y + (f.MaxY - ch.Bearing.Y) * scale;

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

                        GLSM.BindTexture(GL_TEXTURE_2D, ch.TextureID);

                        glBindBuffer(GL_ARRAY_BUFFER, fontVBO);

                        fixed (float* vert = &vertices[0])
                        {
                            glBufferSubData(GL_ARRAY_BUFFER, 0, sizeof(float) * vertices.Length, vert);
                        }

                        glBindBuffer(GL_ARRAY_BUFFER, 0);
                        glDrawArrays(GL_TRIANGLES, 0, 6);
                        x += ch.Advance * scale;
                    }
                }
                else if (token.type == FormattedText.FTTokenType.OpeningTag)
                {
                    colorStack.Push(ColorF.FromString(token.value));
                }
                else if (token.type == FormattedText.FTTokenType.ClosingTag)
                {
                    colorStack.Pop();
                }

            }

            glBindVertexArray(0);
            GLSM.BindTexture(GL_TEXTURE_2D, 0);
        }
    }
}