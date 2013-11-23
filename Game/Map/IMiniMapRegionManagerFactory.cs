namespace Game.Map
{
    public interface IMiniMapRegionManagerFactory
    {
        IMiniMapRegionManager CreateMiniMapRegionManager(int regionCount);
    }
}