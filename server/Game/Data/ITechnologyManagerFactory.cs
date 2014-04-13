namespace Game.Data
{
    public interface ITechnologyManagerFactory
    {
        ITechnologyManager CreateTechnologyManager(EffectLocation location, uint cityId, uint ownerId);
    }
}