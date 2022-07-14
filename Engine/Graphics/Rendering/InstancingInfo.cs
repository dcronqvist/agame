using System.Drawing;
using System.Numerics;
using AGame.Engine.Assets;

namespace AGame.Engine.Graphics.Rendering;

public struct InstancingInfo
{
    public Matrix4x4 ModelMatrix { get; set; }
    public RectangleF SourceRectangle { get; set; }

    private InstancingInfo(Matrix4x4 modelMatrix, RectangleF sourceRectangle)
    {
        this.ModelMatrix = modelMatrix;
        this.SourceRectangle = sourceRectangle;
    }

    public static InstancingInfo Create(Vector2 position, float rotation, Vector2 scale, Vector2 origin, RectangleF sourceRec)
    {
        return new InstancingInfo(Utilities.CreateModelMatrixFromPosition(position, rotation, origin, scale), sourceRec);
    }

    public float[] GetUVCoordinateData(Texture2D texture)
    {
        float sourceX = this.SourceRectangle.X / texture.Width;
        float sourceY = this.SourceRectangle.Y / texture.Height;
        float sourceWidth = this.SourceRectangle.Width / texture.Width;
        float sourceHeight = this.SourceRectangle.Height / texture.Height;

        float[] vertices = { 
            // tex
            sourceX, sourceY + sourceHeight, //downLeft
            sourceX + sourceWidth, sourceY, //topRight
            sourceX, sourceY, //topLeft

            sourceX, sourceY + sourceHeight, //downLeft
            sourceX + sourceWidth, sourceY + sourceHeight, //downRight
            sourceX + sourceWidth, sourceY  //topRight
        };

        return vertices;
    }

    public float[] GetModelMatrixData()
    {
        return Utilities.GetMatrix4x4Values(this.ModelMatrix);
    }

    public override bool Equals(object obj)
    {
        return obj is InstancingInfo info &&
               ModelMatrix.Equals(info.ModelMatrix) &&
               SourceRectangle.Equals(info.SourceRectangle);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(ModelMatrix, SourceRectangle);
    }
}