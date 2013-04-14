using System;
using System.Collections.Generic;
using System.Linq;
using Game.Logic.Conditons;

namespace Game.Logic.Triggers
{
    public class CityTriggerManager
    {
        class Trigger
        {
            public IDynamicCondition DynamicCondition { get; set; }
            public DynamicAction Action { get; set; }
        }

        private readonly Dictionary<Type, List<Trigger>> triggers = new Dictionary<Type, List<Trigger>>();

        #region Implementation of ICityTriggerManager

        public void AddTrigger(IDynamicCondition dynamicCondition, DynamicAction action)
        {
            foreach (var type in dynamicCondition.EventType)
            {
                List<Trigger> list;
                if (!triggers.TryGetValue(type, out list))
                {
                    list = new List<Trigger>();
                    triggers.Add(type, list);
                }
                list.Add(new Trigger {Action = action, DynamicCondition = dynamicCondition});
            }
        }

        public void Process(ICityEvent cityEvent)
        {
            List<Trigger> list;
            if(triggers.TryGetValue(cityEvent.GetType(), out list))
            {
                foreach (var trigger in list.Where(t=> t.DynamicCondition.IsFulfilled(cityEvent)))
                {
                    trigger.Action.Execute(cityEvent.GameObject);
                }
            }
        }

        #endregion
    }
}
