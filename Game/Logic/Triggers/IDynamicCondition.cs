using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Game.Logic.Conditons
{
    public interface IDynamicCondition
    {
        void SetParameters(string [] parms);
        Type[] EventType { get; }
        bool IsFulfilled(ICityEvent cityEvent);
    }
}
