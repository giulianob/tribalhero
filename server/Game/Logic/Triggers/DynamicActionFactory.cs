using System;
using Game.Setup.DependencyInjection;

namespace Game.Logic.Triggers
{
    public class DynamicActionFactory : IDynamicActionFactory
    {
        private readonly IKernel kernel;

        public DynamicActionFactory(IKernel kernel)
        {
            this.kernel = kernel;
        }

        public IDynamicAction CreateDynamicAction(Type actionType, string nlsDescription)
        {
            return new DynamicAction(actionType, nlsDescription, kernel);
        }
    }
}