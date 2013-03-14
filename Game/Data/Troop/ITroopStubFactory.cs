namespace Game.Data.Troop
{
    public interface ITroopStubFactory
    {
        ITroopStub CreateTroopStub(byte troopId);
    }
}