using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;
using Game.Data.Stronghold;
using Game.Data.Troop;
using Game.Logic.Actions;
using Game.Map;
using Game.Setup;
using Game.Util;
using Persistance;

namespace Game.Data.Tribe
{
    class TribeManager : ITribeManager
    {
        private readonly IDbManager dbManager;

        private readonly IStrongholdManager strongholdManager;

        private readonly IActionFactory actionFactory;

        private readonly LargeIdGenerator tribeIdGen = new LargeIdGenerator(200000, 100000);

        private readonly object Lock = new object();

        private ConcurrentDictionary<uint, ITribe> Tribes { get; set; }

        public TribeManager(IDbManager dbManager, IStrongholdManager strongholdManager, IActionFactory actionFactory)
        {
            Tribes = new ConcurrentDictionary<uint, ITribe>();
            this.dbManager = dbManager;
            this.strongholdManager = strongholdManager;
            this.actionFactory = actionFactory;
        }

        public int TribeCount
        {
            get
            {
                return Tribes.Count;
            }
        }

        public void Add(ITribe tribe)
        {
            tribe.Id = (uint)tribeIdGen.GetNext();
            if (!Tribes.TryAdd(tribe.Id, tribe))
            {
                return;
            }

            dbManager.Save(tribe);

            SubscribeEvents(tribe);
        }

        public void DbLoaderAdd(ITribe tribe)
        {
            tribeIdGen.Set(tribe.Id);
            if (!Tribes.TryAdd(tribe.Id, tribe))
            {
                return;
            }

            SubscribeEvents(tribe);
        }

        private void SubscribeEvents(ITribe tribe)
        {
            tribe.TribesmanRemoved += TribeOnTribesmanRemoved;
        }

        private void UnsubscribeEvents(ITribe tribe)
        {
            tribe.TribesmanRemoved += TribeOnTribesmanRemoved;
        }

        private void TribeOnTribesmanRemoved(object sender, TribesmanRemovedEventArgs e)
        {
           foreach (var city in e.Player.GetCityList())
           {
               // Retreat all stationed troops in strongholds that are idle.
               // If they are in battle, then the battle action will take care of removing them. If they are walking to a stronghold, then the attack/reinforce action will walk them back as well.
               foreach (var stub in city.Troops.MyStubs().Where(stub => stub.Station is IStronghold && stub.State == TroopState.Stationed))
               {
                    var retreatAction = actionFactory.CreateRetreatChainAction(stub.City.Id, stub.TroopId);
                    stub.City.Worker.DoPassive(stub.City, retreatAction, true);
               }
           }
        }

        public Error Remove(ITribe tribe)
        {
            if (tribe.AssignmentCount > 0)
            {
                return Error.TribeHasAssignment;
            }

            if (!Tribes.TryRemove(tribe.Id, out tribe))
            {
                return Error.TribeNotFound;
            }

            strongholdManager.RemoveStrongholdsFromTribe(tribe);       

            foreach (var tribesman in new List<ITribesman>(tribe.Tribesmen))
            {
                tribe.RemoveTribesman(tribesman.Player.PlayerId, false, false);
            }            

            UnsubscribeEvents(tribe);

            // Soft delete tribe
            dbManager.Query(
                            String.Format(
                                          "UPDATE `{0}` SET deleted = 1, name = @name WHERE id = @id LIMIT 1",
                                          Tribe.DB_TABLE),
                            new[]
                            {
                                    new DbColumn("id", tribe.Id, DbType.String),
                                    new DbColumn("name", String.Format("{0} (DELETED)", tribe.Name), DbType.String)
                            });

            return Error.Ok;
        }

        public bool TribeNameTaken(string name)
        {
            using (
                    DbDataReader reader = dbManager.ReaderQuery(String.Format("SELECT `id` FROM `{0}` WHERE name = @name LIMIT 1", Tribe.DB_TABLE),
                                                                            new[]
                                                                            {
                                                                                    new DbColumn("name", name, DbType.String)
                                                                            }))
            {
                return reader.HasRows;
            }
        }

        public bool FindTribeId(string name, out uint tribeId)
        {
            tribeId = UInt16.MaxValue;
            using (
                    DbDataReader reader = dbManager.ReaderQuery(String.Format("SELECT `id` FROM `{0}` WHERE name = @name LIMIT 1", Tribe.DB_TABLE),
                                                                            new[]
                                                                            {
                                                                                    new DbColumn("name", name, DbType.String)
                                                                            }))
            {
                if (!reader.HasRows)
                {
                    return false;
                }
                reader.Read();
                tribeId = (uint)reader[0];
                return true;
            }
        }

        public bool TryGetTribe(uint tribeId, out ITribe tribe)
        {
            return Tribes.TryGetValue(tribeId, out tribe);
        }
    }
}