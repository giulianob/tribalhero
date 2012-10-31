namespace Game.Map
{
    public interface ICityRegionManagerFactory
    {
        ICityRegionManager CreateCityRegionManager(int regionCount);
    }
}