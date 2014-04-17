using System.Collections.Generic;

namespace Game.Setup
{
    public class SystemVariableManager : ISystemVariableManager
    {
        readonly Dictionary<string, SystemVariable> systemVariables = new Dictionary<string, SystemVariable>();

        public SystemVariable this[string key]
        {
            get
            {
                return systemVariables[key];
            }
            set
            {
                systemVariables[key] = value;
            }
        }

        public bool TryGetValue(string key, out SystemVariable systemVariable)
        {
            return systemVariables.TryGetValue(key, out systemVariable);
        }

        public bool ContainsKey(string key)
        {
            return systemVariables.ContainsKey(key);
        }

        public void Add(string key, SystemVariable systemVariable)
        {
            systemVariables.Add(key, systemVariable);
        }

        public void Clear()
        {
            systemVariables.Clear();
        }
    }
}
