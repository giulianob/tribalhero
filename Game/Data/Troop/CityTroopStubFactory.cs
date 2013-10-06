namespace Game.Data.Troop
{
    public class CityTroopStubFactory : ITroopStubFactory
    {
        public CityTroopStubFactory(ICity city)
        {
            City = city;
        }

        private ICity City { get; set; }

        public TroopStub CreateTroopStub(ushort troopId)
        {
            return new TroopStub(troopId, City);
        }
    }
}