using System.Collections.Generic;

namespace Game.Data.Troop
{
    public interface ITroopManager : IEnumerable<ITroopStub>
    {
        byte Size { get; }

        IStation BaseStation { get; }

        int Upkeep { get; }

        ITroopStub this[byte index] { get; set; }

        event TroopManager.UpdateCallback TroopUpdated;

        event TroopManager.UpdateCallback TroopAdded;

        event TroopManager.UpdateCallback TroopRemoved;

        event TroopManager.UpdateCallback TroopUnitUpdated;

        bool DbLoaderAdd(byte id, ITroopStub stub);

        bool DbLoaderAddStation(ITroopStub stub);

        bool Add(ITroopStub stub, out byte id);

        bool AddStationed(ITroopStub stub);

        bool Add(ITroopStub stub);

        bool RemoveStationed(byte id);

        bool Remove(byte id);

        ITroopStub Create();

        bool TryGetStub(byte id, out ITroopStub stub);

        void Starve(int percent = 5, bool bypassProtection = false);

        void StubUpdateEvent(TroopStub stub);

        IEnumerable<ITroopStub> StationedHere();

        IEnumerable<ITroopStub> MyStubs();
    }
}