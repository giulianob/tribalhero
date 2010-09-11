#region

using System.Collections.Generic;
using System.Reflection;
using Game.Data;
using Game.Setup;

#endregion

namespace Game.Logic {
    public class EffectRequirement {
        public string[] parms;
        public MethodInfo method;
    }

    public class EffectRequirementContainer {
        private List<EffectRequirement> list = new List<EffectRequirement>();
        public uint ID { get; set; }

        public Error validate(GameObject obj, IEnumerable<Effect> effects) {
            Error error;
            foreach (EffectRequirement req in list) {
                object[] parms = new object[] {obj, effects, req.parms, ID};
                if ((error = (Error) req.method.Invoke(null, parms)) != Error.OK)
                    return error;
            }
            return Error.OK;
        }

        public void add(EffectRequirement req) {
            list.Add(req);
        }
    }
}