using Game.Data;
using Game.Logic.Procedures;

namespace Game.Map
{
    public interface ICityManager
    {
        int Count { get; }

        uint GetNextCityId();

        bool TryGetCity(uint cityId, out ICity city);

        void AfterDbLoaded(Procedure procedure);

        void Remove(ICity city);

        void Add(ICity city);

        void DbLoaderAdd(ICity city);

        bool FindCityId(string name, out uint cityId);
    }
}