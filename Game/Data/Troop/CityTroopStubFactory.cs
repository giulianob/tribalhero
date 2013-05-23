using System;

namespace Game.Data.Troop
{
    public class CityTroopStubFactory : ITroopStubFactory
    {
        public ICity City { get; set; }

        public ITroopStub CreateTroopStub(ushort troopId)
        {
            if (City == null)
            {
                throw new Exception("City cannot be null");
            }

            return new TroopStub(troopId, City);
        }
    }
}