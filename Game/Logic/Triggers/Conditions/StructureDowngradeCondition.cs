using System;
using Game.Logic.Conditons;
using Game.Logic.Triggers.Events;

namespace Game.Logic.Triggers.Conditions
{

    class StructureDowngradeCondition : IDynamicCondition
    {
        private byte level;
        private ushort type;
        
        #region Implementation of IDynamicCondition

        public void SetParameters(string[] parms)
        {
            type = ushort.Parse(parms[0]);
            level = byte.Parse(parms[1]);
        }

        public Type[] EventType
        {
            get
            {
                return new [] {typeof(StructureDowngradeEvent)};
            }
        }

        public bool IsFulfilled(ICityEvent cityEvent)
        {
            return cityEvent.Parameters.level == level && cityEvent.Parameters.type == type;
        }

        #endregion
    }
}
