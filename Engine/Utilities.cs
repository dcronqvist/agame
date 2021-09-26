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
    }
}