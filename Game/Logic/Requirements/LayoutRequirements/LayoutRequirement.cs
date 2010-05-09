#region

using System.Collections.Generic;
using Game.Data;

#endregion

namespace Game.Logic {
    enum LayoutComparison : byte {
        NOT_CONTAINS = 0,
        CONTAINS = 1
    }

    public abstract class LayoutRequirement {
        protected List<Reqirement> requirements = new List<Reqirement>();
        public abstract bool Validate(Structure builder, ushort type, uint x, uint y);

        public void Add(Reqirement req) {
            requirements.Add(req);
        }
    }
}