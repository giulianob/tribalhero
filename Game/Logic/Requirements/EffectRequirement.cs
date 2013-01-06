#region

using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using Game.Data;
using Game.Setup;

#endregion

namespace Game.Logic
{
    public class EffectRequirement
    {
        public string Description { get; set; }

        public MethodInfo Method { get; set; }

        public string[] Parms { get; set; }

        public string WebsiteDescription { get; set; }
    }

    public class EffectRequirementContainer : IEnumerable<EffectRequirement>
    {
        private readonly List<EffectRequirement> list = new List<EffectRequirement>();

        public uint Id { get; set; }

        #region IEnumerable<EffectRequirement> Members

        public IEnumerator<EffectRequirement> GetEnumerator()
        {
            return list.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        #endregion

        public Error Validate(IGameObject obj, IEnumerable<Effect> effects)
        {
            foreach (var req in list)
            {
                var parms = new object[] {obj, effects, req.Parms, Id};
                Error error;
                if ((error = (Error)req.Method.Invoke(null, parms)) != Error.Ok)
                {
                    return error;
                }
            }
            return Error.Ok;
        }

        public void Add(EffectRequirement req)
        {
            list.Add(req);
        }
    }
}