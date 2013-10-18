using System.Collections.Generic;
using Game.Data.Stats;
using Persistance;

namespace Game.Data.Troop
{
    public interface IUnitTemplate : IEnumerable<KeyValuePair<ushort, IBaseUnitStats>>, IPersistableList
    {
        event UnitTemplate.UpdateCallback UnitUpdated;

        ICity City { get; }

        int Size { get; }

        IBaseUnitStats this[ushort type] { get; set; }

        void BeginUpdate();

        void EndUpdate();

        void DbLoaderAdd(ushort type, IBaseUnitStats stats);
    }
}