namespace Game.Map
{
    public interface IRegionFactory
    {
        IRegion CreateRegion(byte[] map);
    }
}