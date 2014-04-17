using System;
using System.Collections.Generic;
using Game.Logic.Conditons;
using Game.Logic.Triggers.Events;

namespace Game.Logic.Triggers.Conditions
{
    class StructureDowngradeOrRemoveCondition : IDynamicCondition
    {
        private ushort type;

        public void SetParameters(string[] parms)
        {
            type = ushort.Parse(parms[0]);
        }

        public IEnumerable<Type> EventType
        {
            get
            {
                return new[] {typeof(StructureDowngradeEvent), typeof(StructureRemoveEvent)};
            }
        }

        public bool IsFulfilled(ICityEvent cityEvent)
        {
            return cityEvent.Parameters.type == type;
        }
    }
}
