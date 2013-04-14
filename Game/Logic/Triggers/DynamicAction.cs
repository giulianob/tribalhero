using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Game.Data;

namespace Game.Logic.Triggers
{
    public class DynamicAction
    {
        public Type Type { get; set; }

        public string[] Parms { get; set; }

        public string NlsDescription { get; set; }

        public void Execute(IGameObject obj)
        {
            var action = (IScriptable)Activator.CreateInstance(Type, new object[] {});

            // TODO: This needs to be more generic to support more types of actions. Since this is init and all init actions are passive, then this is okay for now.
            if (action is ScheduledPassiveAction)
            {
                ((ScheduledPassiveAction)action).NlsDescription = NlsDescription;
            }

            action.ScriptInit(obj, Parms);
        }
    }
}
