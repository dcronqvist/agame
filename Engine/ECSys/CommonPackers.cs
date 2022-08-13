using System;
using System.Collections.Generic;
using System.Drawing;
using System.Numerics;
using AGame.Engine.Graphics;
using AGame.Engine.Items;
using AGame.Engine.World;

namespace AGame.Engine.ECSys;

public class IntPacker : ComponentPropertyPacker<int>
{
    public override byte[] Pack(int value)
    {
        return BitConverter.GetBytes(value);
    }

    public override int Unpack(byte[] data, int offset, out int value)
    {
        value = BitConverter.ToInt32(data, offset);
        return sizeof(int);
    }
}

public class FloatPacker : ComponentPropertyPacker<float>
{
    public override byte[] Pack(float value)
    {
        return BitConverter.GetBytes(value);
    }
    public override int Unpack(byte[] data, int offset, out float value)
    {
        value = BitConverter.ToSingle(data, offset);
        return sizeof(float);
    }
}

public class DoublePacker : ComponentPropertyPacker<double>
{
    public override byte[] Pack(double value)
    {
        return BitConverter.GetBytes(value);
    }
    public override int Unpack(byte[] data, int offset, out double value)
    {
        value = BitConverter.ToDouble(data, offset);
        return sizeof(double);
    }
}

public class StringPacker : ComponentPropertyPacker<string>
{
    public override byte[] Pack(string value)
    {
        List<byte> bytes = new List<byte>();
        bytes.AddRange(BitConverter.GetBytes(value.Length));
        bytes.AddRange(System.Text.Encoding.UTF8.GetBytes(value));
        return bytes.ToArray();
    }
    public override int Unpack(byte[] data, int offset, out string value)
    {
        int len = BitConverter.ToInt32(data, offset);
        value = System.Text.Encoding.UTF8.GetString(data, offset + sizeof(int), len);
        return sizeof(int) + len;
    }
}

public class BoolPacker : ComponentPropertyPacker<bool>
{
    public override byte[] Pack(bool value)
    {
        return BitConverter.GetBytes(value);
    }
    public override int Unpack(byte[] data, int offset, out bool value)
    {
        value = BitConverter.ToBoolean(data, offset);
        return sizeof(bool);
    }
}

public class BytePacker : ComponentPropertyPacker<byte>
{
    public override byte[] Pack(byte value)
    {
        return new byte[] { value };
    }
    public override int Unpack(byte[] data, int offset, out byte value)
    {
        value = data[offset];
        return sizeof(byte);
    }
}

public class CharPacker : ComponentPropertyPacker<char>
{
    public override byte[] Pack(char value)
    {
        return BitConverter.GetBytes(value);
    }
    public override int Unpack(byte[] data, int offset, out char value)
    {
        value = BitConverter.ToChar(data, offset);
        return sizeof(char);
    }
}

public class ShortPacker : ComponentPropertyPacker<short>
{
    public override byte[] Pack(short value)
    {
        return BitConverter.GetBytes(value);
    }
    public override int Unpack(byte[] data, int offset, out short value)
    {
        value = BitConverter.ToInt16(data, offset);
        return sizeof(short);
    }
}

public class UShortPacker : ComponentPropertyPacker<ushort>
{
    public override byte[] Pack(ushort value)
    {
        return BitConverter.GetBytes(value);
    }
    public override int Unpack(byte[] data, int offset, out ushort value)
    {
        value = BitConverter.ToUInt16(data, offset);
        return sizeof(ushort);
    }
}

public class LongPacker : ComponentPropertyPacker<long>
{
    public override byte[] Pack(long value)
    {
        return BitConverter.GetBytes(value);
    }
    public override int Unpack(byte[] data, int offset, out long value)
    {
        value = BitConverter.ToInt64(data, offset);
        return sizeof(long);
    }
}

public class ULongPacker : ComponentPropertyPacker<ulong>
{
    public override byte[] Pack(ulong value)
    {
        return BitConverter.GetBytes(value);
    }
    public override int Unpack(byte[] data, int offset, out ulong value)
    {
        value = BitConverter.ToUInt64(data, offset);
        return sizeof(ulong);
    }
}

public class Vector2Packer : ComponentPropertyPacker<Vector2>
{
    public override byte[] Pack(Vector2 value)
    {
        List<byte> bytes = new List<byte>();
        bytes.AddRange(BitConverter.GetBytes(value.X));
        bytes.AddRange(BitConverter.GetBytes(value.Y));
        return bytes.ToArray();
    }
    public override int Unpack(byte[] data, int offset, out Vector2 value)
    {
        float x = BitConverter.ToSingle(data, offset);
        float y = BitConverter.ToSingle(data, offset + sizeof(float));
        value = new Vector2(x, y);
        return sizeof(float) * 2;
    }
}

public class CoordinateVectorPacker : ComponentPropertyPacker<CoordinateVector>
{
    public override byte[] Pack(CoordinateVector value)
    {
        List<byte> bytes = new List<byte>();
        bytes.AddRange(BitConverter.GetBytes(value.X));
        bytes.AddRange(BitConverter.GetBytes(value.Y));
        return bytes.ToArray();
    }
    public override int Unpack(byte[] data, int offset, out CoordinateVector value)
    {
        float x = BitConverter.ToSingle(data, offset);
        float y = BitConverter.ToSingle(data, offset + sizeof(float));
        value = new CoordinateVector(x, y);
        return sizeof(float) * 2;
    }
}

public class RectangleFPacker : ComponentPropertyPacker<RectangleF>
{
    public override byte[] Pack(RectangleF value)
    {
        List<byte> bytes = new List<byte>();
        bytes.AddRange(BitConverter.GetBytes(value.X));
        bytes.AddRange(BitConverter.GetBytes(value.Y));
        bytes.AddRange(BitConverter.GetBytes(value.Width));
        bytes.AddRange(BitConverter.GetBytes(value.Height));
        return bytes.ToArray();
    }
    public override int Unpack(byte[] data, int offset, out RectangleF value)
    {
        float x = BitConverter.ToSingle(data, offset);
        float y = BitConverter.ToSingle(data, offset + sizeof(float));
        float width = BitConverter.ToSingle(data, offset + sizeof(float) * 2);
        float height = BitConverter.ToSingle(data, offset + sizeof(float) * 3);
        value = new RectangleF(x, y, width, height);
        return sizeof(float) * 4;
    }
}

public class ItemInstancePacker : ComponentPropertyPacker<ItemInstance>
{
    public override byte[] Pack(ItemInstance value)
    {
        PackedItem pi = new PackedItem(value);
        return pi.ToBytes();
    }

    public override int Unpack(byte[] data, int offset, out ItemInstance value)
    {
        PackedItem pi = new PackedItem();
        int bytesRead = pi.Populate(data, offset);
        value = pi.Instance;
        return bytesRead;
    }
}

public class ArrayPacker<TElem, TElementPacker> : ComponentPropertyPacker<TElem[]> where TElementPacker : IComponentPropertyPacker
{
    public override byte[] Pack(TElem[] value)
    {
        var elemPacker = Activator.CreateInstance<TElementPacker>();
        List<byte> bytes = new List<byte>();
        bytes.AddRange(BitConverter.GetBytes(value.Length));
        foreach (var elem in value)
        {
            bytes.AddRange(elemPacker.Pack(elem));
        }
        return bytes.ToArray();
    }

    public override int Unpack(byte[] data, int offset, out TElem[] value)
    {
        int startOffset = offset;
        var elemPacker = Activator.CreateInstance<TElementPacker>();
        int count = BitConverter.ToInt32(data, offset);
        offset += sizeof(int);

        value = new TElem[count];
        for (int i = 0; i < count; i++)
        {
            object o;
            offset += elemPacker.Unpack(data, offset, out o);
            value[i] = (TElem)o;
        }
        return offset - startOffset;
    }
}

public class ColorFPacker : ComponentPropertyPacker<ColorF>
{
    public override byte[] Pack(ColorF value)
    {
        List<byte> bytes = new List<byte>();
        bytes.AddRange(BitConverter.GetBytes(value.R));
        bytes.AddRange(BitConverter.GetBytes(value.G));
        bytes.AddRange(BitConverter.GetBytes(value.B));
        bytes.AddRange(BitConverter.GetBytes(value.A));
        return bytes.ToArray();
    }
    public override int Unpack(byte[] data, int offset, out ColorF value)
    {
        float r = BitConverter.ToSingle(data, offset);
        float g = BitConverter.ToSingle(data, offset + sizeof(float));
        float b = BitConverter.ToSingle(data, offset + sizeof(float) * 2);
        float a = BitConverter.ToSingle(data, offset + sizeof(float) * 3);
        value = new ColorF(r, g, b, a);
        return sizeof(float) * 4;
    }
}