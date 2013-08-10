using Game.Setup;

namespace Game.Data.Troop
{
    public interface ITroopObjectInitializer
    {
        Error GetTroopObject(out ITroopObject troopObject);

        void DeleteTroopObject();
    }
}
