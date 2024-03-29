using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Numerics;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using AGame.Engine.ECSys;
using AGame.Engine.Items;
using AGame.Engine.Networking;
using AGame.Engine.World;
using GameUDPProtocol;

namespace AGame.Engine
{
    public static class Utilities
    {
        static Random RNG;

        static Utilities()
        {

        }

        public static List<T> NValues<T>(T value, int n)
        {
            List<T> values = new List<T>();
            for (int i = 0; i < n; i++)
            {
                values.Add(value);
            }
            return values;
        }

        public static void InitRNG()
        {
            RNG = new Random();
        }

        public static void InitRNG(int seed)
        {
            RNG = new Random(seed);
        }

        public static string GetExecutableDirectory()
        {
            return Path.GetDirectoryName(Assembly.GetEntryAssembly().Location);
        }

        public static Vector2 Round(this Vector2 vector2)
        {
            return new Vector2(MathF.Round(vector2.X), MathF.Round(vector2.Y));
        }

        public static string Repeat(this string s, int n)
        {
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < n; i++)
            {
                sb.Append(s);
            }
            return sb.ToString();
        }

        public static float Clamp(float min, float max, float value)
        {
            if (value > max)
                return max;
            else if (value < min)
                return min;
            return value;
        }

        public static float Lerp(float min, float max, float value)
        {
            return value * (max - min) + min;
        }

        public static float GetLinearCircularFalloff(int diameter, int x, int y)
        {
            Vector2 middle = new Vector2(diameter / 2f, diameter / 2f);
            float radius = diameter / 2f;

            Vector2 pos = new Vector2(x, y);
            float distToMiddle = Math.Abs((middle - pos).Length());

            float val = Clamp(0, 1f, 1.0f - (distToMiddle / (diameter / 2.0f)));

            return Clamp(0.0f, 1.0f, Lerp(0.0f, 1.0f, val * val));
        }

        public static float[] GetMatrix4x4Values(Matrix4x4 m)
        {
            return new float[]
            {
                m.M11, m.M12, m.M13, m.M14,
                m.M21, m.M22, m.M23, m.M24,
                m.M31, m.M32, m.M33, m.M34,
                m.M41, m.M42, m.M43, m.M44
            };
        }

        public static Matrix4x4 CreateMatrix4x4FromValues(float[] arr)
        {
            return new Matrix4x4(arr[0], arr[1], arr[2], arr[3],
                                arr[4], arr[5], arr[6], arr[7],
                                arr[8], arr[9], arr[10], arr[11],
                                arr[12], arr[13], arr[14], arr[15]);
        }

        public static int GetRandomInt(int min, int max)
        {
            return RNG.Next(min, max);
        }

        public static float GetRandomFloat()
        {
            return GetRandomFloat(0.0f, 1.0f);
        }

        public static float GetRandomFloat(float min, float max)
        {
            return ((float)RNG.NextDouble()) * (max - min) + min;
        }

        public static Vector2 GetRandomVector2(float minX, float maxX, float minY, float maxY)
        {
            return new Vector2(GetRandomFloat(minX, maxX), GetRandomFloat(minY, maxY));
        }

        public static Vector2 GetRandomVector2WithinDistance(Vector2 origin, float minDist, float maxDist)
        {
            float dist = Utilities.GetRandomFloat(minDist, maxDist);
            float rot = Utilities.GetRandomFloat(0f, 2f * MathF.PI);

            Vector2 offset = new Vector2(MathF.Cos(rot), MathF.Sin(rot)) * dist;

            return origin + offset;
        }

        public static RectangleF Offset(this RectangleF r, Vector2 vec)
        {
            return new RectangleF(r.X + vec.X, r.Y + vec.Y, r.Width, r.Height);
        }

        public static Vector2 Floor(this Vector2 vector2)
        {
            return new Vector2((int)vector2.X, (int)vector2.Y);
        }

        public static float AbsLength(this Vector2 vector2)
        {
            return MathF.Abs(vector2.Length());
        }

        public static byte[,] EmptyByteGrid(int width, int height)
        {
            byte[,] grid = new byte[width, height];

            return grid;
        }

        public static Matrix4x4 CreateModelMatrixFromPosition(Vector2 position, float rotation, Vector2 origin, Vector2 scale)
        {
            // Rotate around origin in original scale
            Matrix4x4 modelMatrix = Matrix4x4.CreateTranslation(new Vector3(-origin, 0)) * Matrix4x4.CreateRotationZ(rotation) * Matrix4x4.CreateScale(new Vector3(scale, 0)) * Matrix4x4.CreateTranslation(new Vector3(origin, 0));
            // Translate to position
            modelMatrix *= Matrix4x4.CreateTranslation(new Vector3(position, 0));

            return Matrix4x4.CreateScale(new Vector3(scale, 0)) * Matrix4x4.CreateRotationZ(rotation) * Matrix4x4.CreateTranslation(new Vector3(position, 0));
        }

        public static IEnumerable<Type> FindDerivedTypesInAssembly(Assembly assembly, Type baseType)
        {
            return assembly.GetTypes().Where(t => baseType.IsAssignableFrom(t));
        }

        public static IEnumerable<Type> FindDerivedTypes(Type baseType)
        {
            return AppDomain.CurrentDomain.GetAssemblies().SelectMany(ass =>
            {
                return FindDerivedTypesInAssembly(ass, baseType);
            });
        }

        public static bool IsSubclassOfRawGeneric(Type generic, Type toCheck)
        {
            while (toCheck != null && toCheck != typeof(object))
            {
                var cur = toCheck.IsGenericType ? toCheck.GetGenericTypeDefinition() : toCheck;
                if (generic == cur)
                {
                    return true;
                }
                toCheck = toCheck.BaseType;
            }
            return false;
        }

        public static T ChooseUniform<T>(params T[] choices)
        {
            return choices[GetRandomInt(0, choices.Length)];
        }

        // Randomly select one of the choices based on its weight. The sum of all weights sums to 1.
        public static T ChooseWeighted<T>(params Tuple<T, float>[] choices)
        {
            float sum = 0;
            foreach (var choice in choices)
            {
                sum += choice.Item2;
            }
            float r = GetRandomFloat(0, sum);
            float cur = 0;
            foreach (var choice in choices)
            {
                cur += choice.Item2;
                if (r < cur)
                {
                    return choice.Item1;
                }
            }
            return choices[choices.Length - 1].Item1;
        }

        public static int[,] ConvertTileGridNamesToIDs(string[,] tileGridNames)
        {
            int[,] tileGridIDs = new int[tileGridNames.GetLength(0), tileGridNames.GetLength(1)];
            for (int x = 0; x < tileGridNames.GetLength(0); x++)
            {
                for (int y = 0; y < tileGridNames.GetLength(1); y++)
                {
                    tileGridIDs[x, y] = TileManager.GetTileIDFromName(tileGridNames[x, y]);
                }
            }
            return tileGridIDs;
        }

        public static int[,] CreateTileGridWith(string tileName, int width, int height)
        {
            int[,] tileGrid = new int[width, height];
            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    tileGrid[x, y] = TileManager.GetTileIDFromName(tileName);
                }
            }
            return tileGrid;
        }

        public static int[,] CreateTileGridWith(int num, int width, int height)
        {
            int[,] tileGrid = new int[width, height];
            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    tileGrid[x, y] = num;
                }
            }
            return tileGrid;
        }

        public static Vector2i GetChunkCoordsFromTileCoords(int x, int y)
        {
            int cx = (int)MathF.Floor(x / (Chunk.CHUNK_SIZE));
            int cy = (int)MathF.Floor(y / (Chunk.CHUNK_SIZE));

            return new Vector2i(x, y);
        }

        public static List<T[]> DivideIPacketables<T>(T[] all, int maxByteSizePerDivision) where T : IPacketable
        {
            T[] sorted = all.OrderBy(x => x.ToBytes().Length).ToArray();

            List<T[]> divisions = new List<T[]>();
            List<T> currentDivision = new List<T>();
            int currentLength = 0;
            for (int i = 0; i < sorted.Length; i++)
            {
                T c = sorted[i];
                int length = c.ToBytes().Length;

                if (currentLength + length > maxByteSizePerDivision)
                {
                    divisions.Add(currentDivision.ToArray());
                    currentDivision.Clear();
                    currentLength = 0;
                }

                currentDivision.Add(c);
                currentLength += length;
            }

            if (currentDivision.Count > 0)
            {
                divisions.Add(currentDivision.ToArray());
            }

            return divisions;
        }

        public static RectangleF Inflate(this RectangleF rec, float amount)
        {
            RectangleF r = new RectangleF(rec.X - amount, rec.Y - amount, rec.Width + amount * 2, rec.Height + amount * 2);
            return r;
        }

        public static string ResolveIPOrDomain(string ipOrDomain)
        {
            if (IPAddress.TryParse(ipOrDomain, out IPAddress ip))
            {
                return ip.ToString();
            }
            else
            {
                return System.Net.Dns.GetHostAddresses(ipOrDomain, System.Net.Sockets.AddressFamily.InterNetwork).First().ToString();
            }
        }

        public static IWorldGenerator GetGeneratorFromTypeName(string name)
        {
            Type[] types = FindDerivedTypes(typeof(IWorldGenerator)).Where(x => x != typeof(IWorldGenerator)).ToArray();

            foreach (Type type in types)
            {
                if (type.Name == name)
                {
                    return (IWorldGenerator)Activator.CreateInstance(type);
                }
            }

            return null;
        }

        public static CoordinateVector ToCoordinateVector(this Vector2 v)
        {
            return new CoordinateVector(v.X, v.Y) / TileGrid.TILE_SIZE;
        }

        public static bool Contains(this RectangleF rec, Vector2 v)
        {
            return rec.Contains(v.X, v.Y);
        }

        public static Vector2 Max(Vector2 a, Vector2 b)
        {
            return new Vector2(Math.Max(a.X, b.X), Math.Max(a.Y, b.Y));
        }

        public static IEnumerable<ContainerSlot> CreateSlotGrid(int spacing, int columns, int rows, Vector2 offset = default(Vector2))
        {
            for (int y = 0; y < rows; y++)
            {
                for (int x = 0; x < columns; x++)
                {
                    yield return new ContainerSlot(new Vector2(x * (ContainerSlot.WIDTH + spacing) + spacing, y * (ContainerSlot.HEIGHT + spacing) + spacing) + offset);
                }
            }
        }

        public static RectangleF Lerp(this RectangleF rec, RectangleF to, float amount)
        {
            RectangleF r = new RectangleF(
                rec.X + (to.X - rec.X) * amount,
                rec.Y + (to.Y - rec.Y) * amount,
                rec.Width + (to.Width - rec.Width) * amount,
                rec.Height + (to.Height - rec.Height) * amount);
            return r;
        }

        public static byte[] RunLengthEncode(byte[] buffer)
        {
            List<byte> output = new List<byte>();
            byte last = buffer[0];
            int count = 1;
            for (int i = 1; i < buffer.Length; i++)
            {
                if (count == 255)
                {
                    output.Add(last);
                    output.Add((byte)count);
                    count = 1;
                }

                if (buffer[i] == last)
                {
                    count++;
                }
                else
                {
                    output.Add(last);
                    output.Add((byte)count);
                    last = buffer[i];
                    count = 1;
                }
            }
            output.Add(last);
            output.Add((byte)count);
            return output.ToArray();
        }

        public static byte[] RunLengthDecode(byte[] encodedBuffer)
        {
            List<byte> output = new List<byte>();
            for (int i = 0; i < encodedBuffer.Length; i += 2)
            {
                byte b = encodedBuffer[i];
                int count = encodedBuffer[i + 1];
                for (int j = 0; j < count; j++)
                {
                    output.Add(b);
                }
            }
            return output.ToArray();
        }

        public static string GetBytesPerSecondAsString(int bytesPerSecond)
        {
            // Convert bytes per second to appropriate units, lowest unit should be KB/s
            if (bytesPerSecond < 1024 * 1024)
            {
                return MathF.Round((float)bytesPerSecond / 1024, 1) + " KB/s";
            }
            else if (bytesPerSecond < 1024 * 1024 * 1024)
            {
                return MathF.Round((float)bytesPerSecond / (1024 * 1024), 2) + " MB/s";
            }
            else
            {
                return MathF.Round((float)bytesPerSecond / (1024 * 1024 * 1024), 3) + " GB/s";
            }
        }

        public static float CalcQuadratic(float a, float b, float c, float x)
        {
            // f(x) = a*x^2 + b*x + c

            return a * x * x + b * x + c;
        }

        public static ulong Hash(byte[] input)
        {
            using (MD5 md5 = MD5.Create())
            {
                return BitConverter.ToUInt64(md5.ComputeHash(input), 0);
            }
        }

        public static ulong Hash(this int input)
        {
            return Hash(BitConverter.GetBytes(input));
        }

        public static ulong Hash(this string input)
        {
            return Hash(Encoding.UTF8.GetBytes(input));
        }

        public static ulong Hash(this ulong input)
        {
            return Hash(BitConverter.GetBytes(input));
        }

        public static ulong Hash(this long input)
        {
            return Hash(BitConverter.GetBytes(input));
        }

        public static ulong Hash(this uint input)
        {
            return Hash(BitConverter.GetBytes(input));
        }

        public static ulong Hash(this float input)
        {
            return Hash(BitConverter.GetBytes(input));
        }

        public static ulong Hash(this double input)
        {
            return Hash(BitConverter.GetBytes(input));
        }

        public static ulong Hash(this bool input)
        {
            return Hash(BitConverter.GetBytes(input));
        }

        public static ulong Hash(this byte input)
        {
            return Hash(new byte[] { input });
        }

        public static ulong CombineHash(params ulong[] hashes)
        {
            ulong hash = 0;
            foreach (ulong h in hashes)
            {
                hash = Hash(hash ^ h);
            }
            return hash;
        }

        public static bool InRange(this float value, float min, float max)
        {
            return value >= min && value <= max;
        }

        public static Vector2 PixelAlign(this Vector2 v)
        {
            return new Vector2(MathF.Round(v.X), MathF.Round(v.Y));
        }

        public static float EaseInOutQuint(float val)
        {
            return val < 0.5f ? 16 * val * val * val * val * val : 1 - MathF.Pow(-2 * val + 2, 5) / 2;
        }

        public static float GetNegAbsCurve(float val)
        {
            return 1f - MathF.Abs(2 * val - 1f);
        }

        public static Vector2 Offset(this Vector2 v, Vector2 vec)
        {
            return v + vec;
        }
    }

    public struct Vector2i : IPacketable
    {
        public int X;
        public int Y;

        public Vector2i(int x, int y)
        {
            X = x;
            Y = y;
        }

        public override bool Equals(object obj)
        {
            return obj is Vector2i i &&
                   X == i.X &&
                   Y == i.Y;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(X, Y);
        }

        public int Populate(byte[] data, int offset)
        {
            X = BitConverter.ToInt32(data, offset);
            Y = BitConverter.ToInt32(data, offset + 4);
            return 8;
        }

        public byte[] ToBytes()
        {
            List<byte> bytes = new List<byte>();

            bytes.AddRange(BitConverter.GetBytes(X));
            bytes.AddRange(BitConverter.GetBytes(Y));

            return bytes.ToArray();
        }
    }
}