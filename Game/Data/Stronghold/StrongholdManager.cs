using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Game.Data.Tribe;
using Game.Logic.Formulas;
using Game.Map;
using Game.Module;
using Game.Setup;
using Game.Util;
using Persistance;

namespace Game.Data.Stronghold
{
    class StrongholdManager : IStrongholdManager
    {
        private readonly ConcurrentDictionary<uint, IStronghold> strongholds = new ConcurrentDictionary<uint, IStronghold>();

        private readonly IStrongholdFactory strongholdFactory;
        
        private readonly IStrongholdConfigurator strongholdConfigurator;
        
        private readonly LargeIdGenerator idGenerator = new LargeIdGenerator(10000, 5000);
        
        private readonly IRegionManager regionManager;
        
        private readonly Chat chat;

        private readonly IDbManager dbManager;

        private readonly Formula formula;

        private ILookup<ITribe, IStronghold> tribeIndex;

        private Dictionary<string, IStronghold> nameIndex;

        private bool indexDirty = true;

        public StrongholdManager(IStrongholdConfigurator strongholdConfigurator,
                                    IStrongholdFactory strongholdFactory,
                                    IRegionManager regionManager,
                                    Chat chat,
                                    IDbManager dbManager,
                                    Formula formula)
        {
            this.strongholdConfigurator = strongholdConfigurator;
            this.strongholdFactory = strongholdFactory;
            this.regionManager = regionManager;
            this.chat = chat;
            this.dbManager = dbManager;
            this.formula = formula;
        }

        public int Count
        {
            get
            {
                return strongholds.Count;
            }
        }

        private void Reindex()
        {
            lock (this)
            {
                if (!indexDirty)
                {
                    return;
                }

                indexDirty = false;
                tribeIndex = strongholds.Values.Where(stronghold => stronghold.Tribe != null).ToLookup(stronghold => stronghold.Tribe, stronghold => stronghold);
                nameIndex = strongholds.Values.ToDictionary(stronghold => stronghold.Name.ToLowerInvariant(), stronghold => stronghold);
            }
        }

        private void MarkIndexDirty()
        {
            lock (this)
            {
                indexDirty = true;
            }
        }

        public void Add(IStronghold stronghold)
        {
            strongholds.AddOrUpdate(stronghold.Id, stronghold, (id, old) => stronghold);
            dbManager.Save(stronghold);
            MarkIndexDirty();
        }

        public void DbLoaderAdd(IStronghold stronghold)
        {
            strongholds.AddOrUpdate(stronghold.Id, stronghold, (id, old) => stronghold);
            MarkIndexDirty();
        }

        public bool TryGetStronghold(uint id, out IStronghold stronghold)
        {
            return strongholds.TryGetValue(id, out stronghold);
        }

        public bool TryGetStronghold(string name, out IStronghold stronghold)
        {
            if (indexDirty)
            {
                Reindex();
            }
            
            return nameIndex.TryGetValue(name.ToLowerInvariant(), out stronghold);
        }

        public void Generate(int count)
        {
            while (count-- > 0)
            {
                string name;
                byte level;
                uint x, y;

                if (!strongholdConfigurator.Next(out name, out level, out x, out y))
                {
                    break;
                }

                IStronghold stronghold = strongholdFactory.CreateStronghold((uint)idGenerator.GetNext(), name, level, x, y, formula.GetGateLimit(level));
                using (dbManager.GetThreadTransaction())
                {
                    Add(stronghold);
                }
            }
        }

        public void Activate(IStronghold stronghold)
        {            
            stronghold.BeginUpdate();
            stronghold.StrongholdState = StrongholdState.Neutral;
            regionManager.Add(stronghold);
            stronghold.EndUpdate();
            
            chat.SendSystemChat("STRONGHOLD_ACTIVE", stronghold.Id.ToString(CultureInfo.InvariantCulture), stronghold.Name);
        }

        public void TransferTo(IStronghold stronghold, ITribe tribe)
        {
            if (tribe == null)
            {
                return;
            }

            ITribe oldTribe = stronghold.Tribe;
            stronghold.BeginUpdate();
            stronghold.StrongholdState = StrongholdState.Occupied;
            stronghold.Tribe = tribe;
            stronghold.Gate = formula.GetGateLimit(stronghold.Lvl);
            stronghold.DateOccupied = DateTime.UtcNow;
            stronghold.EndUpdate();
            MarkIndexDirty();

            if (oldTribe != null)
            {
                chat.SendSystemChat("STRONGHOLD_TAKEN_OVER", stronghold.Name, tribe.Name, oldTribe.Name);
            }
            else
            {
                chat.SendSystemChat("STRONGHOLD_NEUTRAL_TAKEN_OVER", stronghold.Name, tribe.Name);
            }
        }

        public IEnumerable<IStronghold> StrongholdsForTribe(ITribe tribe)
        {
            if (indexDirty)
            {
                Reindex();
            }

            return !tribeIndex.Contains(tribe) ? new IStronghold[] { } : tribeIndex[tribe];
        }

        public void RemoveStrongholdsFromTribe(ITribe tribe)
        {
            foreach (var stronghold in StrongholdsForTribe(tribe))
            {
                stronghold.BeginUpdate();
                stronghold.StrongholdState = StrongholdState.Neutral;
                stronghold.Tribe = null;
                stronghold.DateOccupied = DateTime.UtcNow;
                stronghold.EndUpdate();
            }

            MarkIndexDirty();
        }

        public Error RepairGate(IStronghold stronghold)
        {
            if (stronghold.Tribe == null )
            {
                return Error.StrongholdNotOccupied;                
            }

            var diff = formula.GetGateLimit(stronghold.Lvl) - stronghold.Gate;
            if (diff <= 0)
            {
                return Error.StrongholdGateFull;
            }

            var cost = formula.GetGateRepairCost(stronghold.Lvl, diff);
            if (!stronghold.Tribe.Resource.HasEnough(cost))
            {
                return Error.ResourceNotEnough;
            }

            stronghold.Tribe.Resource.Subtract(cost);
            dbManager.Save(stronghold.Tribe);

            stronghold.BeginUpdate();
            stronghold.Gate = formula.GetGateLimit(stronghold.Lvl);
            stronghold.EndUpdate();

            return Error.Ok;
        }

        #region Implementation of IEnumerable

        public IEnumerator<IStronghold> GetEnumerator()
        {
            return strongholds.Values.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        #endregion
    }
}
