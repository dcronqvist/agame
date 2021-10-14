using System;
using System.Linq;
using System.Reflection;

namespace AGame.Engine
{
    public static class Debug
    {
        public static bool DrawEntityCollisionBoxes = false;
        public static bool DrawEntityBasePositions = false;

        public static bool PropertyExists(string name)
        {
            FieldInfo[] fis = typeof(Debug).GetFields();

            return fis.Any(x => x.Name == name);
        }

        public static Type GetDebugPropertyType(string name)
        {
            FieldInfo fi = typeof(Debug).GetField(name);
            return fi.FieldType;
        }

        public static void SetDebugProperty(string name, object value)
        {
            FieldInfo fi = typeof(Debug).GetField(name);
            fi.SetValue(null, value);
        }
    }
}