using System.Collections.Generic;
using Game.Setup;

namespace Game.Data
{
    public interface ISimpleStubGeneratorFactory
    {
        SimpleStubGenerator CreateSimpleStubGenerator(double[][] ratio, ushort[] type);
    }
}