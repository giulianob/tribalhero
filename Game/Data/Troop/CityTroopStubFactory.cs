using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Game.Data.Troop
{
    public class CityTroopStubFactory : ITroopStubFactory
    {
        private ICity City { get; set; }

        public CityTroopStubFactory(ICity city)
        {
            City = city;
        }

        public TroopStub CreateTroopStub(byte troopId)
        {
            return new TroopStub(troopId, City);
        }
    }
}
