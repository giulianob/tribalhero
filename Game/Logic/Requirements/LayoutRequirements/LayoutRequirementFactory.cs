using Game.Logic.Formulas;
using Game.Map;
using Game.Setup.DependencyInjection;

namespace Game.Logic.Requirements.LayoutRequirements
{
    public class LayoutRequirementFactory : ILayoutRequirementFactory
    {
        private readonly IKernel kernel;

        public LayoutRequirementFactory(IKernel kernel)
        {
            this.kernel = kernel;
        }

        public AwayFromLayout CreateAwayFromLayout()
        {
            return new AwayFromLayout(kernel.Get<Formula>(), kernel.Get<ITileLocator>());
        }

        public SimpleLayout CreateSimpleLayout()
        {
            return new SimpleLayout(kernel.Get<ITileLocator>());
        }
    }
}