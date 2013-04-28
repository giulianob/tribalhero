using System;
using Game.Data;
using Ninject;

namespace Game.Logic.Triggers
{
    public class DynamicAction : IDynamicAction
    {
        private readonly IKernel kernel;

        private Type ActionType { get; set; }

        private string NlsDescription { get; set; }

        public string[] Parms { get; private set; }

        public DynamicAction(Type actionType, string nlsDescription, IKernel kernel)
        {
            ActionType = actionType;
            this.NlsDescription = nlsDescription;
            this.kernel = kernel;
            Parms = new string[5];

            try
            {
                kernel.Get(ActionType);
            }
            catch(Exception e)
            {
                throw new Exception(string.Format("Cannot resolve dynamic action {0}", actionType.FullName), e);
            }
        }

        public void Execute(IGameObject obj)
        {
            var action = (IScriptable)kernel.Get(ActionType);

            // TODO: This needs to be more generic to support more types of actions. Since this is init and all init actions are passive, then this is okay for now.
            if (action is ScheduledPassiveAction)
            {
                ((ScheduledPassiveAction)action).NlsDescription = NlsDescription;
            }

            action.ScriptInit(obj, Parms);
        }
    }
}
