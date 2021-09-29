using System;
using System.IO;
using System.Numerics;
using System.Reflection;
using System.Text;

namespace AGame.Engine
{
    static class Utilities
    {
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
    }
}