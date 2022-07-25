using System.Drawing;
using System.Numerics;
using AGame.Engine.OpenGL;
using static AGame.Engine.OpenGL.GL;

namespace AGame.Engine.Graphics.Rendering
{
    public class RenderTexture
    {
        public uint framebuffer;
        public uint renderedTexture;
        public uint quadVao;

        public int Width { get; set; }
        public int Height { get; set; }

        public RenderTexture(Vector2 size) : this((int)size.X, (int)size.Y) { }

        public RenderTexture(int width, int height, bool sizeFollowWindow = true)
        {
            this.Width = width;
            this.Height = height;

            this.InitRenderData(width, height);

            if (sizeFollowWindow)
            {
                DisplayManager.OnFramebufferResize += (window, size) =>
                {
                    this.Resize((int)size.X, (int)size.Y);
                };
            }
        }

        public unsafe void Resize(int width, int height)
        {
            this.Width = width;
            this.Height = height;

            GLSM.BindTexture(GL_TEXTURE_2D, this.renderedTexture);
            glTexImage2D(GL_TEXTURE_2D, 0, GL_RGBA, width, height, 0, GL_RGBA, GL_UNSIGNED_BYTE, NULL);
            GLSM.BindTexture(GL_TEXTURE_2D, 0);
        }

        public unsafe ColorF ReadPixel(int x, int y)
        {
            byte[] pixelData = new byte[4];
            fixed (byte* pix = &pixelData[0])
            {
                glReadPixels(x, y, 1, 1, GL_RGBA, GL_UNSIGNED_BYTE, pix);
            }
            return new ColorF(pixelData[0], pixelData[1], pixelData[2], pixelData[3]);
        }

        public unsafe void InitRenderData(int width, int height)
        {
            this.framebuffer = glGenFramebuffer();
            glBindFramebuffer(GL_FRAMEBUFFER, this.framebuffer);

            this.renderedTexture = glGenTexture();
            GLSM.BindTexture(GL_TEXTURE_2D, this.renderedTexture);

            glTexImage2D(GL_TEXTURE_2D, 0, GL_RGBA, width, height, 0, GL_RGBA, GL_UNSIGNED_BYTE, NULL);

            glTexParameteri(GL_TEXTURE_2D, GL_TEXTURE_MIN_FILTER, GL_NEAREST);
            glTexParameteri(GL_TEXTURE_2D, GL_TEXTURE_MAG_FILTER, GL_NEAREST);

            glFramebufferTexture2D(GL_FRAMEBUFFER, GL_COLOR_ATTACHMENT0, GL_TEXTURE_2D, this.renderedTexture, 0);
            glBindFramebuffer(GL_FRAMEBUFFER, 0);

            float[] vertices = { 
                // pos      // tex
                0.0f, 1.0f, 0.0f, 0.0f, //downLeft
                1.0f, 0.0f, 1.0f, 1.0f, //topRight
                0.0f, 0.0f, 0.0f, 1.0f, //topLeft

                0.0f, 1.0f, 0.0f, 0.0f, //downLeft
                1.0f, 1.0f, 1.0f, 0.0f, //downRight
                1.0f, 0.0f, 1.0f, 1.0f  //topRight
            };

            this.quadVao = glGenVertexArray();
            glBindVertexArray(this.quadVao);

            uint vbo = glGenBuffer();
            glBindBuffer(GL_ARRAY_BUFFER, vbo);

            fixed (float* v = &vertices[0])
            {
                glBufferData(GL_ARRAY_BUFFER, sizeof(float) * vertices.Length, v, GL_STATIC_DRAW);
            }

            glEnableVertexAttribArray(0);
            glVertexAttribPointer(0, 2, GL_FLOAT, false, 4 * sizeof(float), (void*)0);

            glEnableVertexAttribArray(1);
            glVertexAttribPointer(1, 2, GL_FLOAT, false, 4 * sizeof(float), (void*)(2 * sizeof(float)));
        }
    }
}