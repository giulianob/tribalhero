using Game.Logic;
using Game.Logic.Actions;
using Game.Logic.Formulas;
using Game.Setup.DependencyInjection;
using Persistance;

namespace Game.Data.Forest
{
    public class ForestFactory : IForestFactory
    {
        private readonly IKernel kernel;

        public ForestFactory(IKernel kernel)
        {
            this.kernel = kernel;
        }

        public IForest CreateForest(uint id, int capacity, uint x, uint y)
        {
            return new Forest(id, capacity, x, y, kernel.Get<IActionFactory>(), kernel.Get<IScheduler>(), kernel.Get<IDbManager>(), kernel.Get<Formula>());
        }
    }
}