namespace Game.Data
{
    public interface ICityFactory
    {
        ICity CreateCity(uint id, IPlayer owner, string name, Resource resource, byte radius, decimal ap);

        ICity CreateCity(uint id, IPlayer owner, string name, LazyResource resource, byte radius, decimal ap);
    }
}
