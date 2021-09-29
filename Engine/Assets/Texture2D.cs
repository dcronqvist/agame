using static AGame.Engine.OpenGL.GL;
using StbImageSharp;
using System.IO;
using AGame.Engine.Assets;
using System.Numerics;
using System.Threading;
using System;
using AGame.Engine.OpenGL;

namespace AGame.Engine.Assets
{
    public class Texture2D : Asset
    {
        public uint ID { get; set; }
        public int Width { get; set; }
        public int Height { get; set; }
        public Vector2 Middle { get; set; }

        private byte[] pixelData;

        public Texture2D(int width, int height, byte[] pixelData)
        {
            this.Width = width;
            this.Height = height;
            this.Middle = new Vector2(width / 2f, height / 2f);
            this.pixelData = pixelData;
        }

        public unsafe uint InitGL(int wrapS, int wrapT, int minFilter, int magFilter)
        {
            // Create texture object
            uint id = glGenTexture();
            GLSM.BindTexture(GL_TEXTURE_2D, id);

            // Set texture data
            fixed (byte* pix = &pixelData[0])
            {
                glTexImage2D(GL_TEXTURE_2D, 0, GL_RGBA, Width, Height, 0, GL_RGBA, GL_UNSIGNED_BYTE, pix);
            }

            // Set a bunch of texture parameters
            glTexParameteri(GL_TEXTURE_2D, GL_TEXTURE_WRAP_S, wrapS);
            glTexParameteri(GL_TEXTURE_2D, GL_TEXTURE_WRAP_T, wrapT);

            glTexParameteri(GL_TEXTURE_2D, GL_TEXTURE_MIN_FILTER, minFilter);
            glTexParameteri(GL_TEXTURE_2D, GL_TEXTURE_MAG_FILTER, magFilter);

            // Generate mip maps
            //glGenerateMipmap(GL_TEXTURE_2D);

            // Done! unbind
            GLSM.BindTexture(GL_TEXTURE_2D, 0);
            return id;
        }

        public void BindTexture(int activeTexture)
        {
            glActiveTexture(activeTexture);
            GLSM.BindTexture(GL_TEXTURE_2D, ID);
        }

        public byte[] GetPixelData()
        {
            return this.pixelData;
        }

        public static bool TryLoadFromFile(string file, out Texture2D tex)
        {
            try
            {
                // ImageResult for loading texture data
                ImageResult imageResult;
                using (Stream stream = File.OpenRead(file))
                {
                    imageResult = ImageResult.FromStream(stream, ColorComponents.RedGreenBlueAlpha);
                }

                tex = new Texture2D(imageResult.Width, imageResult.Height, imageResult.Data);
                return true;
            }
            catch
            {

            }

            tex = null;

            //Failsafe
            return false;
        }

        public override bool InitOpenGL()
        {
            this.ID = InitGL(GL_CLAMP_TO_EDGE, GL_CLAMP_TO_EDGE, GL_NEAREST, GL_NEAREST);
            return true;
        }
    }
}