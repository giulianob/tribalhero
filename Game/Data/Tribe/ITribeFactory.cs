namespace Game.Data.Tribe
{
    public interface ITribeFactory
    {
        ITribe CreateTribe(IPlayer owner, string name, string desc, byte level, int victoryPoints, int attackPoints, int defensePoints, Resource resource);

        ITribe CreateTribe(IPlayer owner, string name);
    }
}
