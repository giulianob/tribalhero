using Game.Setup;
using Game.Setup.DependencyInjection;

namespace Game.Data
{
    public class SimpleStubGeneratorFactory : ISimpleStubGeneratorFactory
    {
        private readonly IKernel kernel;

        public SimpleStubGeneratorFactory(IKernel kernel)
        {
            this.kernel = kernel;
        }

        public SimpleStubGenerator CreateSimpleStubGenerator(double[][] ratio, ushort[] type)
        {
            return new SimpleStubGenerator(ratio, type, kernel.Get<UnitFactory>());
        }
    }
}