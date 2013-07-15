using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;
using Game.Comm;
using Game.Data.Stronghold;
using Game.Data.Tribe.EventArguments;
using Game.Data.Troop;
using Game.Logic;
using Game.Logic.Actions;
using Game.Map;
using Game.Setup;
using Game.Util;
using Persistance;

namespace Game.Data.Tribe
{
    public class TribeManager : ITribeManager
    {
        private readonly IActionFactory actionFactory;

        private readonly ITribeFactory tribeFactory;

        private readonly ITribeLogger tribeLogger;

        private readonly IDbManager dbManager;

        private readonly IStrongholdManager strongholdManager;

        private readonly LargeIdGenerator tribeIdGen = new LargeIdGenerator(Config.tribe_id_max, Config.tribe_id_min);

        public TribeManager(IDbManager dbManager, IStrongholdManager strongholdManager, IActionFactory actionFactory, ITribeFactory tribeFactory, ITribeLogger tribeLogger)
        {
            Tribes = new ConcurrentDictionary<uint, ITribe>();
            this.dbManager = dbManager;
            this.strongholdManager = strongholdManager;
            this.actionFactory = actionFactory;
            this.tribeFactory = tribeFactory;
            this.tribeLogger = tribeLogger;
        }

        private ConcurrentDictionary<uint, ITribe> Tribes { get; set; }

        public int TribeCount
        {
            get
            {
                return Tribes.Count;
            }
        }

        public void Add(ITribe tribe)
        {
            tribe.Id = tribeIdGen.GetNext();
            if (!Tribes.TryAdd(tribe.Id, tribe))
            {
                return;
            }

            dbManager.Save(tribe);

            SubscribeEvents(tribe);
        }

        public void DbLoaderSetIdUsed(uint id)
        {
            tribeIdGen.Set(id);
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

            tribe.Owner.LastDeletedTribe = SystemClock.Now;
            dbManager.Save(tribe.Owner);

            foreach (var tribesman in new List<ITribesman>(tribe.Tribesmen))
            {
                tribe.RemoveTribesman(tribesman.Player.PlayerId, wasKicked: false, doNotRemoveIfOwner: false);
            }

            UnsubscribeEvents(tribe);

            // Soft delete tribe
            dbManager.Query(
                            String.Format("UPDATE `{0}` SET deleted = 1, name = @name WHERE id = @id LIMIT 1",
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
                    DbDataReader reader =
                            dbManager.ReaderQuery(
                                                  String.Format("SELECT `id` FROM `{0}` WHERE name = @name LIMIT 1",
                                                                Tribe.DB_TABLE),
                                                  new[] {new DbColumn("name", name, DbType.String)}))
            {
                return reader.HasRows;
            }
        }

        public bool FindTribeId(string name, out uint tribeId)
        {
            tribeId = UInt16.MaxValue;
            using (
                    DbDataReader reader =
                            dbManager.ReaderQuery(
                                                  String.Format("SELECT `id` FROM `{0}` WHERE name = @name LIMIT 1",
                                                                Tribe.DB_TABLE),
                                                  new[] {new DbColumn("name", name, DbType.String)}))
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

        public IEnumerable<Tribe.IncomingListItem> GetIncomingList(ITribe tribe)
        {
            var incomingTroops = (from tribesmen in tribe.Tribesmen
                                  from city in tribesmen.Player.GetCityList()
                                  from notification in city.Notifications
                                  where
                                          notification.Action is IActionTime &&
                                          notification.Action.Category == ActionCategory.Attack &&
                                          notification.GameObject.City != city && notification.Subscriptions.Count > 0
                                  select
                                          new Tribe.IncomingListItem
                                          {
                                                  Source = notification.GameObject.City,
                                                  Target = city,
                                                  EndTime = ((IActionTime)notification.Action).EndTime
                                          }).ToList();

            incomingTroops.AddRange(from stronghold in strongholdManager.StrongholdsForTribe(tribe)
                                    from notification in stronghold.Notifications
                                    where
                                            notification.Action is IActionTime &&
                                            notification.Action.Category == ActionCategory.Attack &&
                                            notification.Subscriptions.Count > 0 &&
                                            (!notification.GameObject.City.Owner.IsInTribe ||
                                             notification.GameObject.City.Owner.Tribesman.Tribe != tribe)
                                    select
                                            new Tribe.IncomingListItem
                                            {
                                                    Source = notification.GameObject.City,
                                                    Target = stronghold,
                                                    EndTime = ((IActionTime)notification.Action).EndTime
                                            });

            return incomingTroops.OrderBy(i => i.EndTime);
        }

        public Error CreateTribe(IPlayer player, string name, out ITribe tribe)
        {
            tribe = null;

            if (player.Tribesman != null)
            {
                return Error.TribesmanAlreadyInTribe;
            }

            if (SystemClock.Now.Subtract(player.LastDeletedTribe).TotalDays < 1)
            {
                return Error.TribeCannotCreateYet;                
            }

            if (TribeNameTaken(name))
            {
                return Error.TribeAlreadyExists;
            }

            if (!Tribe.IsNameValid(name))
            {
                return Error.TribeNameInvalid;
            }            

            tribe = tribeFactory.CreateTribe(player, name);

            tribe.CreateRank(0, "Chief", TribePermission.All);
            tribe.CreateRank(1, "Elder", TribePermission.Invite | TribePermission.Kick | TribePermission.Repair | TribePermission.AssignmentCreate);
            tribe.CreateRank(2, "Protector", TribePermission.Repair | TribePermission.AssignmentCreate);
            tribe.CreateRank(3, "Aggressor", TribePermission.AssignmentCreate);
            tribe.CreateRank(4, "Tribesmen", TribePermission.None);

            Add(tribe);

            var tribesman = new Tribesman(tribe, player, tribe.ChiefRank);
            tribe.AddTribesman(tribesman);

            return Error.Ok;
        }

        private void SubscribeEvents(ITribe tribe)
        {
            tribe.TribesmanRemoved += TribeOnTribesmanRemoved;
            tribe.Updated += TribeOnUpdated;
            tribe.RanksUpdated += TribeOnRanksUpdated;
            tribeLogger.Listen(tribe);
        }

        private void TribeOnRanksUpdated(object sender, TribeEventArgs e)
        {
            Packet packet = new Packet(Command.TribeChannelRanksUpdate);
            PacketHelper.AddTribeRanksToPacket(e.Tribe, packet);

            Global.Current.Channel.Post("/TRIBE/" + e.Tribe.Id, packet);
        }

        private void TribeOnUpdated(object sender, TribeEventArgs e)
        {            
            Packet packet = new Packet(Command.TribeChannelNotification);
            packet.AddInt32(GetIncomingList(e.Tribe).Count());
            packet.AddInt16(e.Tribe.AssignmentCount);
            
            Global.Current.Channel.Post("/TRIBE/" + e.Tribe.Id, packet);
        }

        private void UnsubscribeEvents(ITribe tribe)
        {
            tribe.TribesmanRemoved -= TribeOnTribesmanRemoved;
            tribe.Updated -= TribeOnUpdated;
            tribe.RanksUpdated += TribeOnRanksUpdated;
            tribeLogger.Unlisten(tribe);
        }

        private void TribeOnTribesmanRemoved(object sender, TribesmanRemovedEventArgs e)
        {
            foreach (var city in e.Player.GetCityList())
            {
                // Retreat all stationed troops in strongholds that are idle.
                // If they are in battle, then the battle action will take care of removing them. If they are walking to a stronghold, then the attack/reinforce action will walk them back as well.
                foreach (var stub in city.Troops.MyStubs().Where(stub =>
                                                                 stub.Station is IStronghold &&
                                                                 ((IStronghold)stub.Station).MainBattle == null &&
                                                                 stub.State == TroopState.Stationed))
                {
                    var retreatAction = actionFactory.CreateRetreatChainAction(stub.City.Id, stub.TroopId);
                    stub.City.Worker.DoPassive(stub.City, retreatAction, true);
                }
            }
        }
    }
}