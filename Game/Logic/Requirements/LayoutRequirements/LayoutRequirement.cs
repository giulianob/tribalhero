#region

using System.Collections.Generic;
using Game.Data;

#endregion

namespace Game.Logic {
    enum LayoutComparison : byte {
        NOT_CONTAINS = 0,
        CONTAINS = 1
    }

    abstract class LayoutRequirement {
        public abstract bool validate(IEnumerable<Structure> objects, uint x, uint y);
        public abstract void add(Reqirement req);
    }
}