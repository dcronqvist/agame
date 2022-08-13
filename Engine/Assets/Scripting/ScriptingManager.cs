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

namespace AGame.Engine.Assets.Scripting;

public class ScriptType
{
    private string ScriptTypeName { get; set; }
    public string OverwrittenBy { get; set; }
    public Type RealType { get; set; }
    public Script ContainedInScript { get; set; }

    public ScriptType(string scriptClassName, Type realType, Script containedInScript)
    {
        ScriptTypeName = scriptClassName;
        RealType = realType;
        ContainedInScript = containedInScript;
        OverwrittenBy = null;
    }

    public string GetScriptTypeName()
    {
        return OverwrittenBy ?? ScriptTypeName;
    }

    public T CreateInstance<T>()
    {
        return (T)Activator.CreateInstance(RealType);
    }

    public bool HasBaseType<T>()
    {
        return RealType.IsAssignableTo(typeof(T));
    }
}

public class ScriptTypeAttribute : Attribute
{
    public string Name { get; set; }
}

public static class ScriptingManager
{
    private static List<ScriptType> _scriptTypes = new List<ScriptType>();
    private static Dictionary<string, ScriptType> _scriptTypesByName = new Dictionary<string, ScriptType>();

    private static List<ModOverwriteDefinition> _overwritesToPerform { get; set; }

    static ScriptingManager()
    {
        _overwritesToPerform = new List<ModOverwriteDefinition>();
    }

    public static void Initialize()
    {
        var scripts = ModManager.GetAssetsOfType<Script>();

        foreach (var script in scripts)
        {
            // All types in the script that have a ScriptTypeAttribute
            var types = script.GetTypes().Where(t => t.GetCustomAttribute(typeof(ScriptTypeAttribute)) != null);

            foreach (var type in types)
            {
                var attr = (ScriptTypeAttribute)type.GetCustomAttribute(typeof(ScriptTypeAttribute));
                var name = $"{script.Mod}.script_type.{attr.Name}";
                var scriptType = new ScriptType(name, type, script);

                _scriptTypes.Add(scriptType);
                _scriptTypesByName.Add(scriptType.GetScriptTypeName(), scriptType);
            }
        }

        foreach (var overwrite in _overwritesToPerform)
        {
            var original = _scriptTypesByName[overwrite.Original];
            var newType = _scriptTypesByName[overwrite.New];
            original.OverwrittenBy = newType.GetScriptTypeName();
        }
    }

    public static ScriptType GetScriptType(string scriptTypeName)
    {
        foreach (var type in _scriptTypes)
        {
            if (type.GetScriptTypeName() == scriptTypeName)
            {
                return type;
            }
        }
        return null;
    }

    public static ScriptType[] GetAllScriptTypesWithBaseType<T>()
    {
        return _scriptTypes.Where(t => t.HasBaseType<T>()).ToArray();
    }

    public static T CreateInstance<T>(string scriptTypeName)
    {
        var scriptType = GetScriptType(scriptTypeName);
        return scriptType.CreateInstance<T>();
    }

    public static ScriptType GetScriptTypeNameFromRealType(Type type)
    {
        foreach (var scriptType in _scriptTypes)
        {
            if (scriptType.RealType == type)
            {
                return scriptType;
            }
        }
        return null;
    }

    public static void AddOverwrite(ModOverwriteDefinition definition)
    {
        _overwritesToPerform.Add(definition);
    }
}