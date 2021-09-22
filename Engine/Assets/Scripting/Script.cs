using System;
using System.Reflection;

namespace AGame.Engine.Assets.Scripting
{
    class Script
    {
        public Assembly Assembly { get; set; }
        public string Name { get; set; }

        public Script(Assembly ass, string name)
        {
            this.Assembly = ass;
            this.Name = name;
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
    }
}