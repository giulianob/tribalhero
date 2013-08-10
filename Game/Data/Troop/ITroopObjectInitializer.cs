namespace Game.Data.Troop
{
    public interface ITroopObjectInitializer
    {
        bool GetTroopObject(out ITroopObject troopObject);

        void DeleteTroopObject(ITroopObject troopObject);
    }
}
