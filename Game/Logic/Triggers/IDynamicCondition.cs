using System;

namespace Game.Logic.Conditons
{
    public interface IDynamicCondition
    {
        void SetParameters(string [] parms);
        Type[] EventType { get; }
        bool IsFulfilled(ICityEvent cityEvent);
    }
}
