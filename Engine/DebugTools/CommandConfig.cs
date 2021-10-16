using System.Collections.Generic;
using System.Linq;

namespace AGame.Engine.DebugTools
{
    public class CommandConfig
    {
        public enum ParameterType
        {
            String = 0,
            Integer = 1,
            Float = 2
        }

        public class Parameter
        {
            public ParameterType Type { get; internal set; }
            public string Name { get; internal set; }
            public int OrderNumber { get; internal set; }

            public Parameter(ParameterType type, string name, int orderNumber)
            {
                Type = type;
                Name = name;
                OrderNumber = orderNumber;
            }
        }

        public string Handle { get; internal set; }
        public Dictionary<string, Parameter> PNameToParameter { get; internal set; }
        public string[] ParameterOrder { get; internal set; }

        public CommandConfig()
        {
            this.PNameToParameter = new Dictionary<string, Parameter>();
            this.ParameterOrder = new string[0];
        }

        public string GetUsageMessage()
        {
            string pars = "";

            for (int i = 0; i < this.ParameterOrder.Length; i++)
            {
                string pName = this.ParameterOrder[i];
                Parameter p = this.PNameToParameter[pName];

                pars += $"{pName}:{p.Type.ToString()} ";
            }

            return $"Usage: {this.Handle} {pars}";
        }

        public CommandConfig SetHandle(string handle)
        {
            this.Handle = handle;
            return this;
        }

        public CommandConfig AddParameter(Parameter parameter)
        {
            this.PNameToParameter.Add(parameter.Name, parameter);
            List<string> porder = new List<string>();
            foreach (KeyValuePair<string, Parameter> ps in this.PNameToParameter)
            {
                porder.Insert(ps.Value.OrderNumber, ps.Key);
            }
            this.ParameterOrder = porder.ToArray();
            return this;
        }

        public bool IsValid()
        {
            bool hasHandle = this.Handle != "";
            bool hasPNames = this.PNameToParameter.Count > 0;
            bool hasParamOrder = this.ParameterOrder.Length > 0;
            bool samePNameAndParameterOrder = this.PNameToParameter.Count == this.ParameterOrder.Length;

            return hasHandle && hasPNames && hasParamOrder && samePNameAndParameterOrder;
        }

        public bool TryParseLine(string command, out Dictionary<string, object> dictionary, out string error)
        {
            string[] split = command.Split(char.Parse(" ")).Skip(1).ToArray();

            Dictionary<string, object> d = new Dictionary<string, object>();

            if (split.Length != this.ParameterOrder.Length)
            {
                dictionary = null;
                error = GetUsageMessage();
                return false;
            }

            for (int i = 0; i < this.ParameterOrder.Length; i++)
            {
                string pName = this.ParameterOrder[i];

                // Validate argument at position against parameter type
                switch (this.PNameToParameter[pName].Type)
                {
                    case CommandConfig.ParameterType.String:
                        d.Add(pName, split[i]);
                        break;

                    case ParameterType.Integer:
                        if (int.TryParse(split[i], out int x))
                        {
                            d.Add(pName, x);
                        }
                        else
                        {
                            dictionary = null;
                            error = $"Argument '{split[i]}' in position {i + 1} could not be parsed as an integer.";
                            return false;
                        }
                        break;

                    case ParameterType.Float:
                        if (float.TryParse(split[i], out float y))
                        {
                            d.Add(pName, y);
                        }
                        else
                        {
                            dictionary = null;
                            error = $"Argument '{split[i]}' in position {i + 1} could not be parsed as a float.";
                            return false;
                        }
                        break;
                }
            }

            dictionary = d;
            error = "";
            return true;
        }
    }
}