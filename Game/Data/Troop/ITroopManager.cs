using System.Collections.Generic;
using Game.Util;

namespace Game.Data.Troop
{
    public interface ITroopManager : IEnumerable<ITroopStub>
    {
        byte Size { get; }

        int Upkeep { get; }

        ITroopStub this[byte index] { get; set; }

        event TroopManager.UpdateCallback TroopUpdated;

        event TroopManager.UpdateCallback TroopAdded;

        event TroopManager.UpdateCallback TroopRemoved;

        event TroopManager.UpdateCallback TroopUnitUpdated;

        void Add(ITroopStub stub);

        void DbLoaderAdd(byte id, ITroopStub stub);

        void DbLoaderAddStation(ITroopStub stub);

        bool AddStationed(ITroopStub stub);

        bool RemoveStationed(byte id);

        void Remove(byte id);

        bool TryGetStub(byte id, out ITroopStub stub);

        void Starve(int percent = 5, bool bypassProtection = false);

        IEnumerable<ITroopStub> StationedHere();

        IEnumerable<ITroopStub> MyStubs();

        SmallIdGenerator IdGen { get; }

        IStation BaseStation { set; }
    }
}