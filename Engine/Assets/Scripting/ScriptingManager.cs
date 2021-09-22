using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.CodeDom;
using System.CodeDom.Compiler;
using Microsoft.CSharp;
using Microsoft.CodeAnalysis;

namespace AGame.Engine.Assets.Scripting
{
    static class ScriptingManager
    {
        public static string ScriptDirectory
        {
            get
            {
                return AssetManager.ResourceDirectory + @"/scripts";
            }
        }

        private static Dictionary<string, Script> Scripts { get; set; }
        private static Dictionary<string, Script> TypeToScript { get; set; }

        static ScriptingManager()
        {
            Scripts = new Dictionary<string, Script>();
            TypeToScript = new Dictionary<string, Script>();
        }

        private static bool LoadScript(string filePath, out Script script, out Exception ex)
        {
            // Attempt to load script
            try
            {
                ScriptCompiler sc = new ScriptCompiler();
                byte[] iass = sc.Compile(filePath, out string[] errorMsgs);
                //Assembly assembly = Assembly.LoadFile(filePath);
                Assembly assembly = Assembly.Load(iass);
                ex = null;
                script = new Script(assembly, Path.GetFileNameWithoutExtension(filePath));
                return true;
            }
            catch (Exception e)
            {
                script = null;
                ex = e;
                return false;
            }

        }

        private static void AddScript(string key, Script script)
        {
            Scripts.Add(key, script);
        }

        private static void PointTypesToScript(Type[] types, string key)
        {
            foreach (Type t in types)
            {
                if (!TypeToScript.ContainsKey(t.Name))
                    TypeToScript.Add(t.Name, Scripts[key]);
            }
        }

        public static void LoadScripts()
        {
            string[] coreScripts = Directory.GetFiles(ScriptDirectory, "*.cs", SearchOption.AllDirectories);

            foreach (string script in coreScripts)
            {
                string fileName = Path.GetFileNameWithoutExtension(script);

                Exception ex;
                Script sc;

                if (LoadScript(script, out sc, out ex))
                {
                    // All went well, just add the script to the dictionary of scripts.
                    AddScript(fileName, sc); // Add script to Scripts dictionary
                    PointTypesToScript(sc.GetTypes(), fileName); // Point all types in this script to this script to this script
                    Console.WriteLine($"Loaded script: {fileName}");
                }
                else
                {
                    // Something went wrong, tell user.
                    // TODO: Add some kind of debug log so you can see what goes wrong.
                    throw ex;
                }
            }
        }

        public static Script GetScript(string name)
        {
            return Scripts[name];
        }

        public static Script GetScriptFromType(string type)
        {
            return TypeToScript[type];
        }

        public static T CreateInstance<T>(string type)
        {
            return CreateInstance<T>(type, null);
        }

        public static T CreateInstance<T>(string type, params object[] args)
        {
            Script sc = GetScriptFromType(type);
            return sc.CreateInstance<T>(type, args);
        }
    }
}