using System.Collections.Generic;
using Game.Util.Locking;
using Persistance;

namespace Game.Data.Troop
{
    public interface ISimpleStub : IEnumerable<Formation>
    {
        void AddUnit(FormationType formationType, ushort type, ushort count);
        byte FormationCount { get; }
        ushort TotalCount { get; }

        /// <summary>
        /// Returns a list of units for specified formations.
        /// If formation is empty, will return all units.
        /// </summary>
        /// <param name="formations"></param>
        /// <returns></returns>
        List<Unit> ToUnitList(params FormationType[] formations);
    }

    public interface ITroopStub : IPersistableList, ILockable, ISimpleStub
    {
        event TroopStub.StateSwitched OnStateSwitched;

        event TroopStub.Removed OnRemoved;

        event TroopStub.OnUnitUpdate UnitUpdate;

        TroopTemplate Template { get; }

        TroopState State { get; set; }

        ICity City { get; }

        byte StationTroopId { get; set; }

        IStation Station { get; set; }

        ushort StationedRetreatCount { get; set; }

        byte TroopId { get; set; }

        decimal TotalHp { get; }

        byte Speed { get; }

        /// <summary>
        ///   Returns the sum of the upkeep for all units in troop stub
        /// </summary>
        int Value { get; }

        int Upkeep { get; }

        int Carry { get; }

        Formation this[FormationType type] { get; set; }

        void Starve(int percent = 5, bool bypassProtection = false);

        void FireUpdated();

        void BeginUpdate();

        void EndUpdate();

        bool AddFormation(FormationType type);

        void ChangeFormation(FormationType originalFormation, FormationType newFormation);

        void Add(ISimpleStub stub);

        bool HasFormation(FormationType formation);

        void FireRemoved();

        ushort RemoveUnit(FormationType formationType, ushort type, ushort count);

        void RemoveAllUnits(params FormationType[] formations);

        int UpkeepForFormation(FormationType inBattle);
    }
}