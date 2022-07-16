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
using AGame.Engine.Configuration;

namespace AGame.Engine.Assets.Scripting
{
    static class ScriptingManager
    {
        private static Dictionary<string, Script> Scripts { get; set; }
        private static Dictionary<string, Script> TypeToScript { get; set; }
        private static Dictionary<string, (Script, string)> ScriptClassAttributeNameToScript { get; set; }

        static ScriptingManager()
        {
            Scripts = new Dictionary<string, Script>();
            TypeToScript = new Dictionary<string, Script>();
            ScriptClassAttributeNameToScript = new Dictionary<string, (Script, string)>();
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
            Script[] scripts = ModManager.GetAssetsOfType<Script>();

            foreach (Script script in scripts)
            {
                // // All went well, just add the script to the dictionary of scripts.
                // AddScript(script.Name, script); // Add script to Scripts dictionary
                // Logging.Log(LogLevel.Info, $"Registered script {script.Name}");
                // PointTypesToScript(script.GetTypes(), script.Name); // Point all types in this script to this script
                Type[] types = script.GetTypes();

                foreach (Type t in types)
                {
                    if (t.GetCustomAttribute<ScriptClassAttribute>() != null)
                    {
                        var scriptAssetNameNoEnd = script.Name.Substring(0, script.Name.LastIndexOf(".") + 1);
                        ScriptClassAttribute attr = t.GetCustomAttribute<ScriptClassAttribute>();

                        if (!ScriptClassAttributeNameToScript.ContainsKey(attr.Name))
                        {
                            ScriptClassAttributeNameToScript.Add(scriptAssetNameNoEnd + attr.Name, (script, t.FullName));

                        }
                    }
                }
            }
        }

        public static Script GetScript(string name)
        {
            return Scripts[name];
        }

        public static (Script, string) GetScriptFromType(string type)
        {
            return ScriptClassAttributeNameToScript[type];
        }

        public static T CreateInstance<T>(string type)
        {
            return CreateInstance<T>(type, null);
        }

        public static T CreateInstance<T>(string type, params object[] args)
        {
            (Script sc, string realType) = GetScriptFromType(type);
            return sc.CreateInstance<T>(realType, args);
        }

        public static object CreateInstance(string type, params object[] args)
        {
            (Script sc, string realType) = GetScriptFromType(type);
            return sc.CreateInstance(realType, args);
        }

        public static object CreateInstance(string type)
        {
            (Script sc, string realType) = GetScriptFromType(type);
            return sc.CreateInstance(realType, null);
        }
    }
}