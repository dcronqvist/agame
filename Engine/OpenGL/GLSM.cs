using System.Collections.Generic;
using static AGame.Engine.OpenGL.GL;

namespace AGame.Engine.OpenGL
{
    static class GLSM
    {
        private static Dictionary<int, uint> boundTextures;

        static GLSM()
        {
            boundTextures = new Dictionary<int, uint>();
            boundTextures.Add(GL_TEXTURE_2D, 0);
        }

        public static void BindTexture(int target, uint texture)
        {
            if (boundTextures[target] != texture)
            {
                glBindTexture(target, texture);
                boundTextures[target] = texture;
            }
        }
    }
}