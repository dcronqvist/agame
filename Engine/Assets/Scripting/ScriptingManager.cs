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
    public static class ScriptingManager
    {
        private static Dictionary<string, Script> Scripts { get; set; }
        private static Dictionary<string, Script> TypeToScript { get; set; }
        private static Dictionary<string, (Script, string)> ScriptClassAttributeNameToScript { get; set; }

        private static List<ModOverwriteDefinition> _overwritesToPerform { get; set; }

        static ScriptingManager()
        {
            Scripts = new Dictionary<string, Script>();
            TypeToScript = new Dictionary<string, Script>();
            ScriptClassAttributeNameToScript = new Dictionary<string, (Script, string)>();
            _overwritesToPerform = new List<ModOverwriteDefinition>();
        }

        private static void AddScript(string key, Script script)
        {
            Scripts.Add(key, script);
        }

        public static string GetScriptClassNameFromRealType(Type type)
        {
            var script = GetScriptFromRealType(type);

            var className = ScriptClassAttributeNameToScript.Where(kvp => kvp.Value.Item1 == script && kvp.Value.Item2 == type.FullName).FirstOrDefault().Key;

            if (_overwritesToPerform.Any(x => x.New == className))
            {
                className = _overwritesToPerform.Where(x => x.New == className).FirstOrDefault().Original;
            }

            return className;
        }

        private static void PointTypesToScript(Type[] types, Script script)
        {
            foreach (Type t in types)
            {
                if (!TypeToScript.ContainsKey(t.FullName))
                {
                    TypeToScript.Add(t.FullName, script);
                }
            }
        }

        public static Type[] GetAllTypesWithBaseType<T>()
        {
            List<Type> types = new List<Type>();
            foreach (KeyValuePair<string, Script> kvp in Scripts)
            {
                Type[] scriptTypes = kvp.Value.GetTypes();
                types.AddRange(scriptTypes.Where(x => x.IsAssignableTo(typeof(T)) && TypeToScript.ContainsKey(x.FullName)));
            }
            return types.ToArray();
        }

        public static void LoadScripts()
        {
            Script[] scripts = ModManager.GetAssetsOfType<Script>();

            foreach (Script script in scripts)
            {
                if (!Scripts.ContainsValue(script))
                {


                    // // All went well, just add the script to the dictionary of scripts.
                    // AddScript(script.Name, script); // Add script to Scripts dictionary
                    // Logging.Log(LogLevel.Info, $"Registered script {script.Name}");
                    // PointTypesToScript(script.GetTypes(), script.Name); // Point all types in this script to this script
                    Type[] types = script.GetTypes();

                    PointTypesToScript(types, script);

                    foreach (Type t in types)
                    {
                        if (t.GetCustomAttribute<ScriptClassAttribute>() != null)
                        {
                            ScriptClassAttribute attr = t.GetCustomAttribute<ScriptClassAttribute>();
                            var modName = script.Mod;
                            var finalName = $"{modName}.script_class.{attr.Name}";

                            if (!ScriptClassAttributeNameToScript.ContainsKey(finalName))
                            {
                                ScriptClassAttributeNameToScript.Add(finalName, (script, t.FullName));
                            }
                        }

                        if (!TypeToScript.ContainsKey(t.FullName))
                        {
                            TypeToScript.Add(t.FullName, script);
                        }
                    }

                    Scripts.Add(script.Name, script);
                }
            }
        }

        public static void AddOverwrite(ModOverwriteDefinition definition)
        {
            _overwritesToPerform.Add(definition);
        }

        public static Script GetScript(string name)
        {
            return Scripts[name];
        }

        public static Script GetScriptFromRealType(Type type)
        {
            return TypeToScript[type.FullName];
        }

        public static (Script, string) GetScriptFromType(string type)
        {
            if (_overwritesToPerform.Any(x => x.Original == type))
            {
                var def = _overwritesToPerform.Where(x => x.Original == type).FirstOrDefault();
                return ScriptClassAttributeNameToScript[def.New];
            }

            return ScriptClassAttributeNameToScript[type];
        }

        public static T CreateInstanceFromRealType<T>(string type)
        {
            Script sc = TypeToScript[type];
            return sc.CreateInstance<T>(type);
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