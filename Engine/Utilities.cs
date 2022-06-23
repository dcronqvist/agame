using System;
using System.Drawing;
using System.IO;
using System.Net;
using System.Numerics;
using System.Reflection;
using System.Text;
using AGame.Engine.ECSys;
using AGame.Engine.Networking;
using AGame.Engine.World;
using GameUDPProtocol;

namespace AGame.Engine
{
    static class Utilities
    {
        static Random RNG;

        static Utilities()
        {

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

        public static Matrix4x4 CreateModelMatrixFromPosition(Vector2 position, Vector2 scale)
        {
            return Matrix4x4.CreateScale(new Vector3(scale, 1f)) * Matrix4x4.CreateTranslation(new Vector3(position, 1f));
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

        public static List<UpdateEntitiesPacket> CreateEntityUpdatePackets(CNType cnType, NDirection direction, params Entity[] entities)
        {
            List<EntityUpdate> updates = new List<EntityUpdate>();

            foreach (Entity e in entities)
            {
                // Send snapshottable components
                Component[] snapshottedComponents = e.GetComponentsWithCNType(cnType, direction);

                // With divisions at most 200, every packet should be able to fit at least 2 entities
                List<Component[]> divisions = Utilities.DivideIPacketables(snapshottedComponents, 200);

                foreach (Component[] division in divisions)
                {
                    updates.Add(new EntityUpdate(e.ID, division));
                }
            }

            List<EntityUpdate[]> entityUpdateDivisions = Utilities.DivideIPacketables(updates.ToArray(), 200);
            List<UpdateEntitiesPacket> packets = new List<UpdateEntitiesPacket>();
            foreach (EntityUpdate[] entityUpdateDivision in entityUpdateDivisions)
            {
                UpdateEntitiesPacket eup = new UpdateEntitiesPacket(entityUpdateDivision);
                packets.Add(eup);
            }

            return packets;
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