using System;
using System.Collections.Generic;
using System.Text;
using Game.Data;

namespace Game.Logic {
    enum LayoutComparison : byte {
        NOT_CONTAINS = 0,
        CONTAINS = 1
    }


    abstract class LayoutRequirement {
        abstract public bool validate(IEnumerable<Structure> objects, uint x, uint y);
        abstract public void add(Reqirement req);
    }
}
