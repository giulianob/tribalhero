using System;
using System.Collections.Generic;
using Game.Data.Troop;

namespace Game.Data.BarbarianTribe
{
    public interface IBarbarianTribeManager
    {
        int Count { get; }

        void DbLoaderAdd(IBarbarianTribe barbarianTribe);

        bool TryGetBarbarianTribe(uint id, out IBarbarianTribe barbarianTribe);

        void Generate(int count);

        void Respawn(IBarbarianTribe barbarianTribe);

        void RelocateIdle(TimeSpan duration);

        IEnumerable<Unit> GenerateNeutralStub(IBarbarianTribe barbarianTribe);
    }
}
