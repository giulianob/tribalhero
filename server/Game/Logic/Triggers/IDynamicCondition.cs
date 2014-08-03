using System;
using System.Collections.Generic;
using Game.Logic.Conditons;

namespace Game.Logic.Triggers
{
    public interface IDynamicCondition
    {
        void SetParameters(string [] parms);
        
        IEnumerable<Type> EventType { get; }
        
        bool IsFulfilled(ICityEvent cityEvent);
    }
}
