using System.Collections.Generic;

namespace Game.Data.Troop
{
    public interface ITroopManager : IEnumerable<ITroopStub>
    {
        ushort Size { get; }

        IStation BaseStation { get; }

        int Upkeep { get; }

        ITroopStub this[ushort index] { get; set; }

        event TroopManager.UpdateCallback TroopUpdated;

        event TroopManager.UpdateCallback TroopAdded;

        event TroopManager.UpdateCallback TroopRemoved;

        event TroopManager.UpdateCallback TroopUnitUpdated;

        bool DbLoaderAdd(ushort id, ITroopStub stub);

        bool DbLoaderAddStation(ITroopStub stub);

        bool Add(ITroopStub stub, out ushort id);

        bool AddStationed(ITroopStub stub);

        bool Add(ITroopStub stub);

        bool RemoveStationed(ushort id);

        bool Remove(ushort id);

        ITroopStub Create();

        bool TryGetStub(ushort id, out ITroopStub stub);

        void Starve(int percent = 5, bool bypassProtection = false);

        void StubUpdateEvent(TroopStub stub);

        IEnumerable<ITroopStub> StationedHere();

        IEnumerable<ITroopStub> MyStubs();
    }
}