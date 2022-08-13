using System.Drawing;
using System.Numerics;
using AGame.Engine.Graphics;
using AGame.Engine.Items;
using AGame.Engine.World;

namespace AGame.Engine.ECSys;

public class IntInterpolator : ComponentPropertyInterpolator<int>
{
    public override object Interpolate(int a, int b, float t)
    {
        return a + (int)((b - a) * t);
    }
}

public class FloatInterpolator : ComponentPropertyInterpolator<float>
{
    public override object Interpolate(float a, float b, float t)
    {
        return a + (b - a) * t;
    }
}

public class DoubleInterpolator : ComponentPropertyInterpolator<double>
{
    public override object Interpolate(double a, double b, float t)
    {
        return a + (b - a) * t;
    }
}

public class StringInterpolator : ComponentPropertyInterpolator<string>
{
    public override object Interpolate(string a, string b, float t)
    {
        return a;
    }
}

public class BoolInterpolator : ComponentPropertyInterpolator<bool>
{
    public override object Interpolate(bool a, bool b, float t)
    {
        return a;
    }
}

public class ByteInterpolator : ComponentPropertyInterpolator<byte>
{
    public override object Interpolate(byte a, byte b, float t)
    {
        return (byte)(a + (byte)((b - a) * t));
    }
}

public class CharInterpolator : ComponentPropertyInterpolator<char>
{
    public override object Interpolate(char a, char b, float t)
    {
        return a + (char)((b - a) * t);
    }
}

public class ShortInterpolator : ComponentPropertyInterpolator<short>
{
    public override object Interpolate(short a, short b, float t)
    {
        return (short)(a + (short)((b - a) * t));
    }
}

public class UShortInterpolator : ComponentPropertyInterpolator<ushort>
{
    public override object Interpolate(ushort a, ushort b, float t)
    {
        return (ushort)(a + (ushort)((b - a) * t));
    }
}

public class LongInterpolator : ComponentPropertyInterpolator<long>
{
    public override object Interpolate(long a, long b, float t)
    {
        return (long)(a + (long)((b - a) * t));
    }
}

public class ULongInterpolator : ComponentPropertyInterpolator<ulong>
{
    public override object Interpolate(ulong a, ulong b, float t)
    {
        return (ulong)(a + (ulong)((b - a) * t));
    }
}

public class Vector2Interpolator : ComponentPropertyInterpolator<Vector2>
{
    public override object Interpolate(Vector2 a, Vector2 b, float t)
    {
        return a + (b - a) * t;
    }
}

public class CoordinateVectorInterpolator : ComponentPropertyInterpolator<CoordinateVector>
{
    public override object Interpolate(CoordinateVector a, CoordinateVector b, float t)
    {
        return a + (b - a) * t;
    }
}

public class RectangleFInterpolator : ComponentPropertyInterpolator<RectangleF>
{
    public override object Interpolate(RectangleF a, RectangleF b, float t)
    {
        float x = a.X + (b.X - a.X) * t;
        float y = a.Y + (b.Y - a.Y) * t;
        float w = a.Width + (b.Width - a.Width) * t;
        float h = a.Height + (b.Height - a.Height) * t;

        return new RectangleF(x, y, w, h);
    }
}

public class ItemInstanceInterpolator : ComponentPropertyInterpolator<ItemInstance>
{
    public override object Interpolate(ItemInstance a, ItemInstance b, float t)
    {
        return a;
    }
}

public class ArrayInterpolator : IComponentPropertyInterpolator
{
    public object Interpolate(InterpolationType type, object a, object b, float t)
    {
        return a;
    }
}

public class ColorFInterpolator : ComponentPropertyInterpolator<ColorF>
{
    public override object Interpolate(ColorF a, ColorF b, float t)
    {
        return ColorF.Lerp(a, b, t);
    }
}