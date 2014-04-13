using Game.Setup;
using Game.Setup.DependencyInjection;

namespace Game.Data.Troop
{
    public class UnitTemplateFactory : IUnitTemplateFactory
    {
        private readonly IKernel kernel;

        public UnitTemplateFactory(IKernel kernel)
        {
            this.kernel = kernel;
        }

        public IUnitTemplate CreateUnitTemplate(uint cityId)
        {
            return new UnitTemplate(kernel.Get<UnitFactory>(), cityId);
        }
    }
}