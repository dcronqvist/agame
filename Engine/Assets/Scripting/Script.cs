using System;
using System.Reflection;

namespace AGame.Engine.Assets.Scripting
{
    class Script
    {
        public Assembly Assembly { get; set; }
        public string Name { get; set; }
        private string Namespace { get; set; }

        public Script(Assembly ass, string name)
        {
            this.Assembly = ass;
            this.Name = name;
            this.Namespace = ass.GetName().Name;
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
            if (Namespace == "?")
            {
                return (T)Assembly.CreateInstance(type);
            }
            return (T)Assembly.CreateInstance(Namespace + "." + type);
        }

        public T CreateInstance<T>(string type, params object[] args)
        {
            if (args == null)
                return CreateInstance<T>(type);

            return (T)Assembly.CreateInstance(Namespace + "." + type, false, BindingFlags.Public, null, args, null, null);
        }

        public object CreateInstance(string type)
        {
            return Assembly.CreateInstance(Namespace + "." + type);
        }

        public override string ToString()
        {
            return "Script - " + Name;
        }
    }
}