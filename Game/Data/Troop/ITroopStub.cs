using Game.Logic.Actions;
using Game.Util.Locking;
using Persistance;

namespace Game.Data.Troop
{
    public interface ITroopStub : IPersistableList, ILockable, ISimpleStub
    {
        TroopTemplate Template { get; }

        TroopState State { get; set; }

        ICity City { get; }

        ushort StationTroopId { get; set; }

        IStation Station { get; set; }

        ushort InitialCount { get; set; }

        ushort RetreatCount { get; set; }

        AttackMode AttackMode { get; set; }

        ushort TroopId { get; set; }

        decimal Speed { get; }

        int Upkeep { get; }

        int Carry { get; }

        event TroopStub.StateSwitched OnStateSwitched;

        event TroopStub.Removed OnRemoved;

        event TroopStub.OnUpdate Update;

        event TroopStub.OnUpdate UnitUpdate;

        void Starve(int percent = 5, bool bypassProtection = false);

        void FireUpdated();

        void FireUnitUpdated();

        void BeginUpdate();

        void EndUpdate();

        bool AddFormation(FormationType type);

        void ChangeFormation(FormationType originalFormation, FormationType newFormation);

        void Add(ISimpleStub stub);

        void FireRemoved();

        ushort RemoveUnit(FormationType formationType, ushort type, ushort count);

        void RemoveAllUnits(params FormationType[] formations);

        int UpkeepForFormation(FormationType inBattle);

        void AddAllToFormation(FormationType formation, ISimpleStub unitsToAdd);
    }
}