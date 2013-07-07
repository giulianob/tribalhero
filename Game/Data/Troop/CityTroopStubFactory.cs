using System;
using Game.Logic.Formulas;
using Ninject;
using Persistance;

namespace Game.Data.Troop
{
    public class CityTroopStubFactory : ITroopStubFactory
    {
        private readonly IKernel kernel;

        public ICity City { get; set; }

        public CityTroopStubFactory(IKernel kernel)
        {
            this.kernel = kernel;
        }

        public ITroopStub CreateTroopStub(ushort troopId)
        {
            if (City == null)
            {
                throw new Exception("City cannot be null");
            }

            return new TroopStub(troopId, City, kernel.Get<Formula>(), kernel.Get<IDbManager>());
        }
    }
}