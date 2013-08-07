using Game.Map;

namespace Game.Data
{
    public interface ICityFactory
    {
        ICity CreateCity(uint id, IPlayer owner, string name, Position position, Resource resource, byte radius, decimal ap);

        ICity CreateCity(uint id, IPlayer owner, string name, Position position, LazyResource resource, byte radius, decimal ap);
    }
}
