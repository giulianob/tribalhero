using System;
using System.Collections.Generic;
using System.Text;
using Game.Data;
using System.Reflection;
using Game.Setup;

namespace Game.Logic {
    public class EffectRequirement {
        public string[] parms;
        public MethodInfo method;
    }

    public class EffectRequirementContainer {
        List<EffectRequirement> list = new List<EffectRequirement>();
        uint id;
        public uint ID {
            get { return id; }
            set { id = value; }
        }

        public Error validate(GameObject obj, IEnumerable<Effect> effects) {
            Error error;
            foreach (EffectRequirement req in list) {
                object[] parms = new object[] { obj, effects, req.parms };
                if ((error=(Error)req.method.Invoke(null, parms))!=Error.OK) return error;
            }
            return Error.OK;
        }
        
        public void add(EffectRequirement req) {
            list.Add(req);
        }
    }
}
