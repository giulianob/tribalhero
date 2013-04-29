namespace Game.Data.Troop
{
    public interface ITroopStubFactory
    {
        TroopStub CreateTroopStub(ushort troopId);
    }
}