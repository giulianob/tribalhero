using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Game.Data.Troop;

namespace Game.Data.Settlement
{
    public interface ISettlementManager
    {
        int Count { get; }

        void DbLoaderAdd(ISettlement settlement);

        bool TryGetSettlement(uint id, out ISettlement settlement);

        void Generate(int count);

        void Respawn(ISettlement settlement);

        void RelocateIdle(TimeSpan duration);  // Generate settlement if needed, Move settlement if idle for too long

        IEnumerable<Unit> GenerateNeutralStub(ISettlement settlement);
    }
}
