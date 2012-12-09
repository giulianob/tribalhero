namespace Game.Data.Troop
{
    public interface ITroopStubFactory
    {
        TroopStub CreateTroopStub(byte troopId);
    }
}