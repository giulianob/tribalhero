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

        public BarbarianTribeFactory(IKernel kernel, Formula formula)
        {
            this.kernel = kernel;
            this.formula = formula;
        }

        public BarbarianTribe CreateBarbarianTribe(uint id, byte level, uint x, uint y, int count)
        {
            return new BarbarianTribe(id, level, x, y, count, formula.BarbarianTribeResources(level), kernel.Get<IDbManager>(), kernel.Get<IActionWorker>());
        }
    }
}