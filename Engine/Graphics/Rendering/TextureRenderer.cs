using System.Drawing;
using System.Numerics;
using AGame.Engine.Assets;
using AGame.Engine.OpenGL;
using static AGame.Engine.OpenGL.GL;

namespace AGame.Engine.Graphics.Rendering;

[Flags]
public enum TextureRenderEffects
{
    None = 1 << 0,
    FlipHorizontal = 1 << 1,
    FlipVertical = 1 << 2,
}

public class TextureRenderer
{
    private Shader shader;
    private uint quadVAO;
    private uint quadVBO;
    public RectangleF currentSourceRectangle;

    public TextureRenderer(Shader shader)
    {
        this.shader = shader;
        InitRenderData();
    }

    public void Render(Texture2D texture, Vector2 position, Vector2 scale, float rotation, ColorF color, TextureRenderEffects effects = TextureRenderEffects.None)
    {
        if (texture != null)
            Render(texture, position, scale, rotation, color, new Vector2(texture.Width, texture.Height) * scale / 2f, new RectangleF(0, 0, texture.Width, texture.Height), effects);
    }

    public void Render(Texture2D texture, Vector2 position, Vector2 scale, float rotation, ColorF color, Vector2 origin, TextureRenderEffects effects = TextureRenderEffects.None)
    {
        if (texture != null)
            Render(texture, position, scale, rotation, color, origin, new Rectangle(0, 0, texture.Width, texture.Height), effects);
    }

    public unsafe void Render(Texture2D texture, Vector2 position, Vector2 scale, float rotation, ColorF color, Vector2 origin, RectangleF sourceRectangle, TextureRenderEffects effects)
    {
        shader.Use();

        Matrix4x4 modelMatrix = Utilities.CreateModelMatrixFromPosition(position, rotation, origin, scale * new Vector2(sourceRectangle.Width, sourceRectangle.Height));

        shader.SetMatrix4x4("projection", Renderer.Camera.GetProjectionMatrix());
        shader.SetVec4("textureColor", color.R, color.G, color.B, color.A);
        shader.SetInt("image", 0);
        shader.SetFloatArray("uvCoords", GetUVCoordinateData(texture, sourceRectangle, effects));
        shader.SetMatrix4x4("model", modelMatrix);

        glActiveTexture(GL_TEXTURE0);
        GLSM.BindTexture(GL_TEXTURE_2D, texture.ID);

        glBindVertexArray(quadVAO);
        glDrawArrays(GL_TRIANGLES, 0, 6);
        glBindVertexArray(0);
    }

    private float[] GetUVCoordinateData(Texture2D texture, RectangleF rec, TextureRenderEffects effects)
    {
        float sourceX = rec.X / texture.Width;
        float sourceY = rec.Y / texture.Height;
        float sourceWidth = rec.Width / texture.Width;
        float sourceHeight = rec.Height / texture.Height;

        float[] data = { 
            // tex
            sourceX, sourceY + sourceHeight, //downLeft
            sourceX + sourceWidth, sourceY, //topRight
            sourceX, sourceY, //topLeft

            sourceX, sourceY + sourceHeight, //downLeft
            sourceX + sourceWidth, sourceY + sourceHeight, //downRight
            sourceX + sourceWidth, sourceY  //topRight
        };

        if (effects.HasFlag(TextureRenderEffects.FlipHorizontal) && effects.HasFlag(TextureRenderEffects.FlipVertical))
        {
            data = new float[] {
                sourceX + sourceWidth, sourceY, // topRight
                sourceX, sourceY + sourceHeight, // downLeft
                sourceX + sourceWidth, sourceY + sourceHeight, // downRight
                
                sourceX + sourceWidth, sourceY, // topRight
                sourceX, sourceY, // topLeft
                sourceX, sourceY + sourceHeight, // downLeft
            };
        }
        else if (effects.HasFlag(TextureRenderEffects.FlipHorizontal))
        {
            data = new float[] {
                sourceX + sourceWidth, sourceY + sourceHeight, // downRight
                sourceX, sourceY, // topLeft
                sourceX + sourceWidth, sourceY, // topRight

                sourceX + sourceWidth, sourceY + sourceHeight, // downRight
                sourceX, sourceY + sourceHeight, // downLeft
                sourceX, sourceY, // topLeft
            };
        }
        else if (effects.HasFlag(TextureRenderEffects.FlipVertical))
        {
            data = new float[] {
                sourceX, sourceY, // topLeft
                sourceX + sourceWidth, sourceY + sourceHeight, // downRight
                sourceX, sourceY + sourceHeight, // downLeft

                sourceX, sourceY, // topLeft
                sourceX + sourceWidth, sourceY, // topRight
                sourceX + sourceWidth, sourceY + sourceHeight, // downRight
            };
        }

        return data;
    }

    private unsafe void InitRenderData()
    {
        // Configure VAO, VBO
        quadVAO = glGenVertexArray(); // Created vertex array object
        glBindVertexArray(quadVAO);

        quadVBO = glGenBuffer();

        float[] vertices = { 
            // pos     
            0.0f, 1.0f, // down left
            1.0f, 0.0f, // top right
            0.0f, 0.0f, // top left

            0.0f, 1.0f, // down left
            1.0f, 1.0f, // down right
            1.0f, 0.0f, // top right
        };

        glBindBuffer(GL_ARRAY_BUFFER, quadVBO);

        fixed (float* v = &vertices[0])
        {
            glBufferData(GL_ARRAY_BUFFER, sizeof(float) * vertices.Length, v, GL_STATIC_DRAW);
        }

        glEnableVertexAttribArray(0);
        glVertexAttribPointer(0, 2, GL_FLOAT, false, 2 * sizeof(float), (void*)0);

        glBindBuffer(GL_ARRAY_BUFFER, 0);
        glBindVertexArray(0);
    }
}
