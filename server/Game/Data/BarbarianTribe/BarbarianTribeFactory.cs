using Game.Logic;
using Game.Logic.Formulas;
using Game.Map;
using Game.Setup;
using Game.Setup.DependencyInjection;
using Persistance;

namespace Game.Data.BarbarianTribe
{
    class BarbarianTribeFactory : IBarbarianTribeFactory
    {
        private readonly IKernel kernel;

        private readonly Formula formula;

        private readonly IActionWorkerFactory actionWorkerFactory;

        public BarbarianTribeFactory(IKernel kernel, Formula formula, IActionWorkerFactory actionWorkerFactory)
        {
            this.kernel = kernel;
            this.formula = formula;
            this.actionWorkerFactory = actionWorkerFactory;
        }

        public BarbarianTribe CreateBarbarianTribe(uint id, byte level, Position position, int count)
        {
            if (level <= 0 || level > 10)
            {
                return null;
            }

            var worker = actionWorkerFactory.CreateActionWorker(null, new SimpleLocation(LocationType.BarbarianTribe, id));
            var barbarianTribe = new BarbarianTribe(id, level, position.X, position.Y, count, kernel.Get<IDbManager>(), worker);

            var resources = formula.BarbarianTribeResources(barbarianTribe);
            barbarianTribe.Resource.Clear();
            barbarianTribe.Resource.Add(resources);

            worker.LockDelegate = () => barbarianTribe;
            return barbarianTribe;
        }
    }
}