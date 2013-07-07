using System.Collections.Generic;
using Game.Util;

namespace Game.Data.Troop
{
    public interface ITroopManager : IEnumerable<ITroopStub>
    {
        ushort Size { get; }

        int Upkeep { get; }

        ITroopStub this[ushort index] { get; set; }

        event TroopManager.UpdateCallback TroopUpdated;

        event TroopManager.UpdateCallback TroopAdded;

        event TroopManager.UpdateCallback TroopRemoved;

        event TroopManager.UpdateCallback TroopUnitUpdated;

        void Add(ITroopStub stub);

        void DbLoaderAddStation(ITroopStub stub);

        bool AddStationed(ITroopStub stub);

        bool RemoveStationed(ushort id);

        void Remove(ushort id);

        bool TryGetStub(ushort id, out ITroopStub stub);

        void Starve(int percent = 5, bool bypassProtection = false);

        SmallIdGenerator IdGen { get; }

        IStation BaseStation { set; }

        IEnumerable<ITroopStub> StationedHere();

        IEnumerable<ITroopStub> MyStubs();
    }
}