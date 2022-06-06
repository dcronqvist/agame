using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.CodeDom;
using System.CodeDom.Compiler;
using Microsoft.CSharp;
using Microsoft.CodeAnalysis;
using System.Linq;
using AGame.Engine.DebugTools;

namespace AGame.Engine.Assets.Scripting
{
    static class ScriptingManager
    {
        public static string ScriptDirectory
        {
            get
            {
                return AssetManager.AssetDirectory + @"/scripts";
            }
        }

        private static Dictionary<string, Script> Scripts { get; set; }
        private static Dictionary<string, Script> TypeToScript { get; set; }

        static ScriptingManager()
        {
            Scripts = new Dictionary<string, Script>();
            TypeToScript = new Dictionary<string, Script>();
        }

        private static void AddScript(string key, Script script)
        {
            Scripts.Add(key, script);
        }

        private static void PointTypesToScript(Type[] types, string key)
        {
            foreach (Type t in types)
            {
                if (!TypeToScript.ContainsKey(t.FullName))
                    TypeToScript.Add(t.FullName, Scripts[key]);
            }
        }

        public static Type[] GetAllTypesWithBaseType<T>()
        {
            List<Type> types = new List<Type>();
            foreach (KeyValuePair<string, Script> kvp in Scripts)
            {
                Type[] scriptTypes = kvp.Value.GetTypes();
                types.AddRange(scriptTypes.Where(x => x.IsAssignableTo(typeof(T))));
            }
            return types.ToArray();
        }

        public static void LoadScripts()
        {
            string[] coreScripts = Directory.GetFiles(ScriptDirectory, "*.cs", SearchOption.AllDirectories);

            Script[] scripts = AssetManager.GetAssetsOfType<Script>();

            foreach (Script script in scripts)
            {
                // All went well, just add the script to the dictionary of scripts.
                AddScript(script.Name, script); // Add script to Scripts dictionary
                PointTypesToScript(script.GetTypes(), script.Name); // Point all types in this script to this script
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