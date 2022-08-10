using System;
using System.Reflection;

namespace AGame.Engine.Assets.Scripting;

public class Script : Asset
{
    public Assembly Assembly { get; set; }

    public Script(Assembly ass)
    {
        this.Assembly = ass;
    }

    public Type[] GetTypes()
    {
        return Assembly.GetTypes();
    }

    public bool HasType(string type)
    {
        foreach (Type t in GetTypes())
        {
            if (t.Name == type)
                return true;
        }

        return false;
    }

    public T CreateInstance<T>(string type)
    {
        return (T)Assembly.CreateInstance(type);
    }

    public T CreateInstance<T>(string type, params object[] args)
    {
        if (args == null)
            return CreateInstance<T>(type);

        return (T)Assembly.CreateInstance(type, false, BindingFlags.Public, null, args, null, null);
    }

    public object CreateInstance(string type)
    {
        return Assembly.CreateInstance(type);
    }

    public object CreateInstance(string type, params object[] args)
    {
        if (args == null)
            return CreateInstance(type);

        return Assembly.CreateInstance(type, false, BindingFlags.Public, null, args, null, null);
    }

    public override bool InitOpenGL()
    {
        // Scripts have nothing to do with OpenGL, so no need to do anything.
        return true;
    }
}
[AttributeUsage(AttributeTargets.Class)]
public class ScriptClassAttribute : Attribute
{
    public string Name { get; set; }
}