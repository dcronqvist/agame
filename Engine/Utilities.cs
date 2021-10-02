using System;
using System.IO;
using System.Numerics;
using System.Reflection;
using System.Text;

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
    }
}