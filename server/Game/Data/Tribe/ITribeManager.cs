using System.Collections.Generic;
using Game.Data;
using Game.Data.Tribe;
using Game.Setup;

namespace Game.Map
{
    public interface ITribeManager
    {
        int TribeCount { get; }

        IEnumerable<ITribe> AllTribes { get; }

        bool TryGetTribe(uint tribeId, out ITribe tribe);

        void Add(ITribe tribe);

        void DbLoaderAdd(ITribe tribe);

        void DbLoaderSetIdUsed(uint id);

        Error Remove(ITribe tribe);

        bool TribeNameTaken(string name);

        bool FindTribeId(string name, out uint tribeId);

        IEnumerable<Tribe.IncomingListItem> GetIncomingList(ITribe tribe);

        Error CreateTribe(IPlayer player, string name, out ITribe tribe);
    }
}