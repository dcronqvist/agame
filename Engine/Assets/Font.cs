using System.Numerics;
using FreeTypeSharp;
using FreeTypeSharp.Native;
using System.Collections.Generic;
using static AGame.Engine.OpenGL.GL;
using static FreeTypeSharp.Native.FT;
using System;
using System.Diagnostics;
using AGame.Engine.Assets;
using System.Threading;
using AGame.Engine.OpenGL;

namespace AGame.Engine.Assets
{
    public struct FontCharacter
    {
        public string Chara { get; set; }
        public uint TextureID { get; set; }
        public Vector2 Size { get; set; }
        public Vector2 Bearing { get; set; }
        public int Advance { get; set; }
    }

    public class Font : Asset
    {
        /// <summary>
        /// Enum for choosing which GL_TEXTURE_MAG_FILTER or GL_TEXTURE_MIN_FILTER during initialization.
        /// </summary>
        public enum FontFilter : int
        {
            NearestNeighbour = 0x2600,
            Linear = 0x2601
        }

        /// <summary>
        /// Contains all characters for this font, and their values
        /// </summary>
        public Dictionary<char, FontCharacter> Characters { get; set; }
        /// <summary>
        /// Which font size this font is.
        /// </summary>
        public uint Size { get; set; }
        /// <summary>
        /// FreeType2 lib.
        /// </summary>
        public FreeTypeLibrary Lib { get; set; }
        /// <summary>
        /// Which GL_TEXTURE_MAG_FILTER to use.
        /// </summary>
        public FontFilter MagFilter { get; set; }
        /// <summary>
        /// Which GL_TEXTURE_MIN_FILTER to use.
        /// </summary>
        public FontFilter MinFilter { get; set; }

        public float MaxY { get; private set; }

        public byte[] Data { get; private set; }

        public Font(byte[] data, uint size, FontFilter magFilter, FontFilter minFilter)
        {
            this.Characters = new Dictionary<char, FontCharacter>();
            this.Size = size;
            this.Data = data;

            this.MagFilter = magFilter;
            this.MinFilter = minFilter;
        }

        private unsafe void Load()
        {
            // Have to init the freetype2 lib.
            this.Lib = new FreeTypeLibrary();

            // Then make sure to use correct byte alignment.
            glPixelStorei(GL_UNPACK_ALIGNMENT, 1);

            // Create empty pointer for the font.
            IntPtr aFace;

            // Init the font using FreeType2
            //FT_New_Face(Lib.Native, TTFFile, 0, out aFace);
            fixed (byte* ptr = &this.Data[0])
            {
                FT_New_Memory_Face(Lib.Native, new IntPtr(ptr), this.Data.Length, 0, out aFace);
            }
            // Set font size
            FT_Set_Pixel_Sizes(aFace, 0, Size);
            // Then create facade for getting all the data.
            FreeTypeFaceFacade ftff = new FreeTypeFaceFacade(Lib, aFace);

            // Loop 128 times, first 128 characters for this font
            for (uint i = 0; i < 256; i++)
            {
                // Check if the character exists for this font.
                FT_Error error = FT_Load_Char(aFace, i, FT_LOAD_RENDER);
                if (error != FT_Error.FT_Err_Ok)
                {
                    // TODO: Fix this shit man, should use integrated console when that is done.
                    //Debug.WriteLine("FREETYPE ERROR: FAILED TO LOAD GLYPH FOR INDEX: " + i);
                    continue;
                }

                // Create new texture
                uint textureId = glGenTexture();
                // Bind it
                GLSM.BindTexture(GL_TEXTURE_2D, textureId);
                // Fill texture with data from font
                glTexImage2D(GL_TEXTURE_2D,
                            0,
                            GL_RED,
                            (int)ftff.GlyphBitmap.width,
                            (int)ftff.GlyphBitmap.rows,
                            0,
                            GL_RED,
                            GL_UNSIGNED_BYTE,
                            ftff.GlyphBitmap.buffer);

                // Simple texture parameters.
                glTexParameteri(GL_TEXTURE_2D, GL_TEXTURE_WRAP_S, GL_CLAMP_TO_EDGE);
                glTexParameteri(GL_TEXTURE_2D, GL_TEXTURE_WRAP_T, GL_CLAMP_TO_EDGE);
                glTexParameteri(GL_TEXTURE_2D, GL_TEXTURE_MIN_FILTER, (int)MinFilter);
                glTexParameteri(GL_TEXTURE_2D, GL_TEXTURE_MAG_FILTER, (int)MagFilter);

                // Init the character struct
                FontCharacter character = new FontCharacter()
                {
                    TextureID = textureId,
                    Size = new Vector2(ftff.GlyphBitmap.width, ftff.GlyphBitmap.rows),
                    Bearing = new Vector2(ftff.GlyphBitmapLeft, ftff.GlyphBitmapTop),
                    Advance = ftff.GlyphMetricHorizontalAdvance,
                    Chara = ((char)i).ToString(),
                };

                if (character.Size.Y > MaxY)
                {
                    this.MaxY = character.Size.Y;
                }

                // Add it to the character dictionary
                Characters.Add((char)i, character);
            }

            FT_Done_Face(aFace);
            FT_Done_FreeType(Lib.Native);
        }

        public Vector2 MeasureString(string text, float scale)
        {
            float sizeX = 0;

            foreach (char c in text)
            {
                FontCharacter ch = Characters[c];

                sizeX += ch.Advance * scale;
            }

            return new Vector2(sizeX, this.MaxY * scale);
        }

        public override bool InitOpenGL()
        {
            Load();
            return true;
        }
    }
}