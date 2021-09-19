using static OpenGL.GL;
using StbImageSharp;
using System.IO;

namespace AGame.Graphics.Textures
{
    class Texture2D
    {
        public uint ID { get; set; }
        public int Width { get; set; }
        public int Height { get; set; }

        private byte[] pixelData;

        public Texture2D(string file)
        {
            if (TryLoadFromFile(file))
                ID = InitGL(GL_CLAMP_TO_EDGE, GL_CLAMP_TO_EDGE, GL_NEAREST, GL_NEAREST);
        }

        public unsafe uint InitGL(int wrapS, int wrapT, int minFilter, int magFilter)
        {
            // Create texture object
            uint id = glGenTexture();
            glBindTexture(GL_TEXTURE_2D, id);

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
            glBindTexture(GL_TEXTURE_2D, 0);
            return id;
        }

        public void BindTexture(int activeTexture)
        {
            glActiveTexture(activeTexture);
            glBindTexture(GL_TEXTURE_2D, ID);
        }

        public bool TryLoadFromFile(string file)
        {
            try
            {
                // ImageResult for loading texture data
                ImageResult imageResult;
                using (Stream stream = File.OpenRead(file))
                {
                    imageResult = ImageResult.FromStream(stream, ColorComponents.RedGreenBlueAlpha);
                }

                // Setting a couple of properties
                this.Width = imageResult.Width;
                this.Height = imageResult.Height;
                this.pixelData = imageResult.Data;
                return true;
            }
            catch
            {

            }

            //Failsafe
            return false;
        }
    }
}