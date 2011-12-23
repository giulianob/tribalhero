using System.Collections.Generic;
using Game.Util.Locking;
using Persistance;

namespace Game.Data.Troop
{
    public interface ITroopStub : IEnumerable<Formation>, IPersistableList, ILockable
    {
        event TroopStub.StateSwitched OnStateSwitched;

        event TroopStub.Removed OnRemoved;

        event TroopStub.OnUnitUpdate UnitUpdate;

        TroopTemplate Template { get; }

        TroopManager TroopManager { get; set; }

        TroopState State { get; set; }

        ICity City { get; }

        byte StationedTroopId { get; set; }

        ICity StationedCity { get; set; }

        TroopObject TroopObject { get; set; }

        byte TroopId { get; set; }

        byte FormationCount { get; }

        ushort TotalCount { get; }

        int TotalHp { get; }

        byte Speed { get; }

        /// <summary>
        ///   Returns the sum of the upkeep for all units in troop stub
        /// </summary>
        int Value { get; }

        int Upkeep { get; }

        int Carry { get; }

        Formation this[FormationType type] { get; set; }

        bool IsDefault();

        void Starve(int percent = 5, bool bypassProtection = false);

        void FireUpdated();

        void BeginUpdate();

        void EndUpdate();

        bool AddFormation(FormationType type);

        bool Add(TroopStub stub);

        bool AddUnit(FormationType formationType, ushort type, ushort count);

        bool Remove(TroopStub troop);

        bool HasFormation(FormationType formation);

        void FireRemoved();

        ushort RemoveUnit(FormationType formationType, ushort type, ushort count);

        void RemoveAllUnits(params FormationType[] formations);

        bool HasEnough(TroopStub troop);

        void Print();

        bool TryGetValue(FormationType formationType, out Formation formation);

        ushort GetFormationBits();

        void KeepFormations(params FormationType[] formations);

        /// <summary>
        /// Returns a list of units for specified formations.
        /// If formation is empty, will return all units.
        /// </summary>
        /// <param name="formations"></param>
        /// <returns></returns>
        List<Unit> ToUnitList(params FormationType[] formations);
    }
}