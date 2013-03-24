namespace Game.Map
{
    public interface IRegionFactory
    {
        Region CreateRegion(byte[] map);
    }
}