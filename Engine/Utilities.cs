using System.IO;
using System.Reflection;

namespace AGame.Engine
{
    static class Utilities
    {
        public static string GetExecutableDirectory()
        {
            return Path.GetDirectoryName(Assembly.GetEntryAssembly().Location);
        }
    }
}