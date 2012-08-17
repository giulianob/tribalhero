using Game.Data;

namespace Game.Map
{
    public interface ICityManager
    {
        int Count { get; }

        bool TryGetCity(uint cityId, out ICity city);

        void AfterDbLoaded();

        void Remove(ICity city);

        void Add(ICity city);

        void DbLoaderAdd(uint id, ICity city);

        bool FindCityId(string name, out uint cityId);
    }
}