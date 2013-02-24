﻿using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using Game.Data.Tribe;
using Game.Data.Troop;
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
        private readonly Chat chat;

        private readonly IDbManager dbManager;

        private readonly Formula formula;

        private readonly LargeIdGenerator idGenerator = new LargeIdGenerator(9999, 5000);

        private readonly IRegionManager regionManager;

        private readonly SimpleStubGenerator simpleStubGenerator;

        private readonly IStrongholdConfigurator strongholdConfigurator;

        private readonly IStrongholdFactory strongholdFactory;

        private readonly ConcurrentDictionary<uint, IStronghold> strongholds =
                new ConcurrentDictionary<uint, IStronghold>();

        private ILookup<ITribe, IStronghold> gateOpenToIndex;

        private bool indexDirty = true;

        private Dictionary<string, IStronghold> nameIndex;

        private ILookup<ITribe, IStronghold> tribeIndex;

        public StrongholdManager(IStrongholdConfigurator strongholdConfigurator,
                                 IStrongholdFactory strongholdFactory,
                                 IRegionManager regionManager,
                                 Chat chat,
                                 IDbManager dbManager,
                                 ISimpleStubGeneratorFactory simpleStubGeneratorFactory,
                                 Formula formula,
                                 ICityManager cityManager)
        {
            this.strongholdConfigurator = strongholdConfigurator;
            this.strongholdFactory = strongholdFactory;
            this.regionManager = regionManager;
            this.chat = chat;
            this.dbManager = dbManager;            
            this.formula = formula;

            cityManager.CityAdded += CityManagerCityAdded;
            simpleStubGenerator = simpleStubGeneratorFactory.CreateSimpleStubGenerator(formula.StrongholdUnitRatio(), formula.StrongholdUnitType());
        }

        void CityManagerCityAdded(object sender, EventArgs e)
        {
            ICity city = sender as ICity;
            foreach (var stronghold in strongholds.Where(x => x.Value.StrongholdState == StrongholdState.Inactive && x.Value.TileDistance(city.X, city.Y) < Config.stronghold_radius_base + Config.stronghold_radius_per_level * x.Value.Lvl))
            {
                stronghold.Value.BeginUpdate();
                ++stronghold.Value.NearbyCitiesCount;
                stronghold.Value.EndUpdate();
            }
        }

        public int Count
        {
            get
            {
                return strongholds.Count;
            }
        }

        public void DbLoaderAdd(IStronghold stronghold)
        {
            strongholds.AddOrUpdate(stronghold.Id, stronghold, (id, old) => stronghold);
            RegisterEvents(stronghold);
            MarkIndexDirty();

            if (stronghold.StrongholdState != StrongholdState.Inactive)
            {
                stronghold.InWorld = true;
                regionManager.DbLoaderAdd(stronghold);
            }
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
            for (var i = 0; i < count; ++i)
            {
                string name;
                byte level;
                uint x, y;

                if (!strongholdConfigurator.Next(i, count, out name, out level, out x, out y))
                {
                    break;
                }

                IStronghold stronghold = strongholdFactory.CreateStronghold((uint)idGenerator.GetNext(),
                                                                            name,
                                                                            level,
                                                                            x,
                                                                            y,
                                                                            formula.StrongholdGateLimit(level));
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
            
            chat.SendSystemChat("STRONGHOLD_ACTIVE", stronghold.Name);
        }

        public void TransferTo(IStronghold stronghold, ITribe tribe)
        {
            if (tribe == null)
            {
                return;
            }

            ITribe oldTribe = stronghold.Tribe;
            stronghold.BeginUpdate();
            if (stronghold.StrongholdState == StrongholdState.Occupied)
                stronghold.BonusDays = ((decimal)SystemClock.Now.Subtract(stronghold.DateOccupied).TotalDays + stronghold.BonusDays) * .75m;
            stronghold.StrongholdState = StrongholdState.Occupied;
            stronghold.Tribe = tribe;
            stronghold.GateOpenTo = null;
            stronghold.Gate = formula.StrongholdGateLimit(stronghold.Lvl);
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

        public IEnumerable<Unit> GenerateNeutralStub(IStronghold stronghold)
        {
            ISimpleStub simpleStub;
            int upkeep;
            byte unitLevel;
            formula.StrongholdUpkeep(stronghold.Lvl, out upkeep, out unitLevel);
            simpleStubGenerator.Generate(stronghold.Lvl,
                                         upkeep,
                                         unitLevel,
                                         Config.stronghold_npc_randomness,
                                         (int)stronghold.Id,
                                         out simpleStub);
            return simpleStub.ToUnitList(FormationType.Normal);
        }

        public IEnumerable<IStronghold> StrongholdsForTribe(ITribe tribe)
        {
            if (indexDirty)
            {
                Reindex();
            }

            return !tribeIndex.Contains(tribe) ? new IStronghold[] {} : tribeIndex[tribe];
        }

        public IEnumerable<IStronghold> OpenStrongholdsForTribe(ITribe tribe)
        {
            if (indexDirty)
            {
                Reindex();
            }

            return !gateOpenToIndex.Contains(tribe) ? new IStronghold[] {} : gateOpenToIndex[tribe];
        }

        public void RemoveStrongholdsFromTribe(ITribe tribe)
        {
            foreach (var stronghold in StrongholdsForTribe(tribe))
            {
                stronghold.BeginUpdate();
                stronghold.StrongholdState = StrongholdState.Neutral;
                stronghold.Tribe = null;
                stronghold.DateOccupied = DateTime.UtcNow;
                stronghold.BonusDays = 0;
                stronghold.EndUpdate();
            }

            MarkIndexDirty();
        }

        public Error RepairGate(IStronghold stronghold)
        {
            if (stronghold.Tribe == null)
            {
                return Error.StrongholdNotOccupied;
            }
            
            if (stronghold.MainBattle != null || stronghold.GateBattle != null)
            {
                return Error.StrongholdNotRepairableInBattle;
            }

            var diff = formula.StrongholdGateLimit(stronghold.Lvl) - stronghold.Gate;
            if (diff <= 0)
            {
                return Error.StrongholdGateFull;
            }

            var cost = formula.StrongholdGateRepairCost(stronghold.Lvl, diff);
            if (!stronghold.Tribe.Resource.HasEnough(cost))
            {
                return Error.ResourceNotEnough;
            }

            stronghold.Tribe.Resource.Subtract(cost);
            dbManager.Save(stronghold.Tribe);

            stronghold.BeginUpdate();
            stronghold.Gate = formula.StrongholdGateLimit(stronghold.Lvl);
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

        private void Reindex()
        {
            lock (this)
            {
                if (!indexDirty)
                {
                    return;
                }

                indexDirty = false;
                tribeIndex =
                        strongholds.Values.Where(stronghold => stronghold.Tribe != null)
                                   .ToLookup(stronghold => stronghold.Tribe, stronghold => stronghold);
                gateOpenToIndex =
                        strongholds.Values.Where(stronghold => stronghold.GateOpenTo != null)
                                   .ToLookup(stronghold => stronghold.GateOpenTo, stronghold => stronghold);
                nameIndex = strongholds.Values.ToDictionary(stronghold => stronghold.Name.ToLowerInvariant(),
                                                            stronghold => stronghold);
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
            RegisterEvents(stronghold);
            dbManager.Save(stronghold);
            MarkIndexDirty();
        }

        private void RegisterEvents(IStronghold stronghold)
        {
            stronghold.GateStatusChanged += StrongholdOnGateStatusChanged;
        }

        private void StrongholdOnGateStatusChanged(object sender, EventArgs eventArgs)
        {
            MarkIndexDirty();

            var stronghold = (Stronghold)sender;

            if (stronghold.GateOpenTo != null)
            {
                chat.SendSystemChat("STRONGHOLD_GATE_BROKEN", stronghold.Name, stronghold.GateOpenTo.Name);
            }
        }

        public void Probe(out int neutralStrongholds, out int capturedStrongholds)
        {
            neutralStrongholds = 0;
            capturedStrongholds = 0;
            
            foreach (var stronghold in strongholds)
            {
                if (stronghold.Value.StrongholdState == StrongholdState.Occupied)
                {
                    capturedStrongholds++;
                }

                if (stronghold.Value.StrongholdState == StrongholdState.Neutral)
                {
                    neutralStrongholds++;
                }
            }
        }
    }
}