using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Game.Logic.Conditons;
using Game.Logic.Triggers.Events;

namespace Game.Logic.Triggers.Conditions
{
    public class TechnologyChangeCondition : IDynamicCondition
    {
        private int type;

        #region Implementation of IDynamicCondition

        public void SetParameters(string[] parms)
        {
            type = int.Parse(parms[0]);
        }

        public Type[] EventType
        {
            get
            {
                return new[] {typeof(TechnologyDeleteEvent), typeof(TechnologyUpgradeEvent)};
            }
        }

        public bool IsFulfilled(ICityEvent cityEvent)
        {
            return cityEvent.Parameters.type == type;
        }

        #endregion
    }
}
