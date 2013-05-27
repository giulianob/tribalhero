using Game.Logic;
using Game.Logic.Formulas;
using Ninject;
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

        public BarbarianTribe CreateBarbarianTribe(uint id, byte level, uint x, uint y, int count)
        {
            var worker = actionWorkerFactory.CreateActionWorker(null, new SimpleLocation(LocationType.BarbarianTribe, id));
            var barbarianTribe = new BarbarianTribe(id, level, x, y, count, formula.BarbarianTribeResources(level), kernel.Get<IDbManager>(), worker);
            worker.LockDelegate = () => barbarianTribe;
            return barbarianTribe;
        }
    }
}