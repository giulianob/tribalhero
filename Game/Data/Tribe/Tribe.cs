using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text.RegularExpressions;
using Game.Comm;
using Game.Data.Troop;
using Game.Database;
using Game.Logic.Actions;
using Game.Logic.Formulas;
using Game.Setup;
using Game.Util.Locking;
using Ninject;
using Persistance;

namespace Game.Data.Tribe
{
    public class Tribe : ITribe
    {
        public class IncomingListItem
        {
            public ICity City { get; set; }
            public AttackChainAction Action { get; set; }
        }

        public const string DB_TABLE = "tribes";
        public const int MEMBERS_PER_LEVEL = 5;
        public uint Id
        {
            get
            {
                return Owner.PlayerId;
            }
        }
        public IPlayer Owner { get; private set; }
        public string Name { get; set; }
        public byte Level { get; set; }

        private int attackPoint;
        public int AttackPoint
        {
            get
            {
                return attackPoint;
            }
            set
            {
                attackPoint = value;
                if (DbPersisted)
                {
                    DbPersistance.Current.Query(string.Format("UPDATE `{0}` SET `attack_point` = @attack_point WHERE `player_id` = @id LIMIT 1", DB_TABLE),
                                           new[] { new DbColumn("attack_point", attackPoint, DbType.Int32), new DbColumn("id", Id, DbType.UInt32) });
                }
            }
        }

        private int defensePoint;
        public int DefensePoint
        {
            get
            {
                return defensePoint;
            }
            set
            {
                defensePoint = value;

                if (DbPersisted)
                {
                    DbPersistance.Current.Query(string.Format("UPDATE `{0}` SET `defense_point` = @defense_point WHERE `player_id` = @id LIMIT 1", DB_TABLE),
                                           new[] { new DbColumn("defense_point", defensePoint, DbType.Int32), new DbColumn("id", Id, DbType.UInt32) });
                }
            }
        }

        private string description = string.Empty;
        public string Description
        {
            get
            {
                return description;
            }
            set
            {
                description = value;

                if (DbPersisted)
                {
                    DbPersistance.Current.Query(string.Format("UPDATE `{0}` SET `desc` = @desc WHERE `player_id` = @id LIMIT 1", DB_TABLE),
                                           new[] { new DbColumn("desc", description, DbType.String, Player.MAX_DESCRIPTION_LENGTH), new DbColumn("id", Id, DbType.UInt32) });
                }
            }
        }

        readonly Dictionary<uint, ITribesman> tribesmen = new Dictionary<uint, ITribesman>();
        readonly Dictionary<int, Assignment> assignments = new Dictionary<int, Assignment>();

        public Resource Resource { get; private set; }

        public short AssignmentCount
        {
            get
            {
                return (short)assignments.Count;
            }
        }

        public IEnumerable<Assignment> Assignments
        {
            get
            {
                return assignments.Values.AsEnumerable();
            }
        }

        public IEnumerable<ITribesman> Tribesmen
        {
            get
            {
                return tribesmen.Values.AsEnumerable();
            }
        }

        public Tribe(IPlayer owner, string name) :
            this(owner, name, string.Empty, 1, 0, 0, new Resource())
        {

        }

        public Tribe(IPlayer owner, string name, string desc, byte level, int attackPoints, int defensePoints, Resource resource)
        {
            Owner = owner;
            Level = level;
            Resource = resource;
            Description = desc;
            Name = name;
            AttackPoint = attackPoints;
            DefensePoint = defensePoints;
        }

        public bool IsOwner(IPlayer player)
        {
            return player.PlayerId == Id;
        }

        public Error AddTribesman(ITribesman tribesman, bool save = true)
        {
            if (tribesmen.ContainsKey(tribesman.Player.PlayerId))
                return Error.TribesmanAlreadyExists;

            if (tribesmen.Count >= Level * MEMBERS_PER_LEVEL)
                return Error.TribeFull;

            tribesman.Player.Tribesman = tribesman;
            tribesmen.Add(tribesman.Player.PlayerId, tribesman);
            if (save)
            {
                DefaultMultiObjectLock.ThrowExceptionIfNotLocked(tribesman);
                DbPersistance.Current.Save(tribesman);
            }
            return Error.Ok;
        }

        public Error RemoveTribesman(uint playerId)
        {
            ITribesman tribesman;
            if (!tribesmen.TryGetValue(playerId, out tribesman))
                return Error.TribesmanNotFound;

            DefaultMultiObjectLock.ThrowExceptionIfNotLocked(tribesman);
            tribesman.Player.Tribesman = null;
            DbPersistance.Current.Delete(tribesman);
            return !tribesmen.Remove(playerId) ? Error.TribesmanNotFound : Error.Ok;
        }

        public bool TryGetTribesman(uint playerId, out ITribesman tribesman)
        {
            return tribesmen.TryGetValue(playerId, out tribesman);
        }

        public Error SetRank(uint playerId, byte rank)
        {
            ITribesman tribesman;

            if (rank == 0)
                return Error.TribesmanNotAuthorized;

            if (!tribesmen.TryGetValue(playerId, out tribesman))
                return Error.TribesmanNotFound;

            DefaultMultiObjectLock.ThrowExceptionIfNotLocked(tribesman);

            if (IsOwner(tribesman.Player))
                return Error.TribesmanIsOwner;

            tribesman.Rank = rank;
            DbPersistance.Current.Save(tribesman);
            tribesman.Player.TribeUpdate();

            return Error.Ok;
        }

        public Error Contribute(uint playerId, Resource resource)
        {
            ITribesman tribesman;
            if (!tribesmen.TryGetValue(playerId, out tribesman))
                return Error.TribesmanNotFound;

            DefaultMultiObjectLock.ThrowExceptionIfNotLocked(tribesman);
            tribesman.Contribution += resource;
            Resource += resource;
            DbPersistance.Current.Save(tribesman, this);

            return Error.Ok;
        }

        public IEnumerable<IncomingListItem> GetIncomingList()
        {
            return from tribesmen in Tribesmen
                   from city in tribesmen.Player.GetCityList()
                   from notification in city.Worker.Notifications
                   where notification.Action is AttackChainAction && notification.Action.WorkerObject.City != city && notification.Subscriptions.Count > 0
                   orderby ((ChainAction)notification.Action).EndTime ascending
                   select new IncomingListItem { Action = (AttackChainAction)notification.Action };
        }

        public bool HasRight(uint playerId, string action)
        {
            ITribesman tribesman;
            if (!TryGetTribesman(playerId, out tribesman))
            {
                return false;
            }

            switch (action)
            {
                case "Request":
                case "Assignment":
                case "Kick":
                    switch (tribesman.Rank)
                    {
                        case 0:
                        case 1:
                            return true;
                    }
                    break;
            }

            return false;
        }

        #region Properties
        public int Count
        {
            get
            {
                return tribesmen.Count;
            }
        }
        #endregion


        #region ILockable Members

        public int Hash
        {
            get { return unchecked((int)Owner.PlayerId); }
        }

        public object Lock
        {
            get { return Owner; }
        }

        #endregion

        public static bool IsNameValid(string tribeName)
        {
            return tribeName != string.Empty && tribeName.Length >= 3 && tribeName.Length <= 20 &&
                   Regex.IsMatch(tribeName, "^([a-z][a-z0-9\\s].*)$", RegexOptions.IgnoreCase);
        }

        #region IPersistableObject Members

        public bool DbPersisted { get; set; }

        #endregion

        #region IPersistable Members

        public string DbTable
        {
            get { return DB_TABLE; }
        }

        public DbColumn[] DbPrimaryKey
        {
            get
            {
                return new[] { new DbColumn("player_id", Id, DbType.UInt32) };
            }
        }

        public DbDependency[] DbDependencies
        {
            get
            {
                return new DbDependency[] { };
            }
        }

        public DbColumn[] DbColumns
        {
            get
            {
                return new[]
                       {
                               new DbColumn("name", Name, DbType.String, 20), new DbColumn("level", Level, DbType.Byte),
                               new DbColumn("crop", Resource.Crop, DbType.Int32), new DbColumn("gold", Resource.Gold, DbType.Int32),
                               new DbColumn("iron", Resource.Iron, DbType.Int32), new DbColumn("wood", Resource.Wood, DbType.Int32),
                       };
            }
        }

        #endregion

        public Error CreateAssignment(ITroopStub stub, uint x, uint y, ICity targetCity, DateTime time, AttackMode mode, out int id)
        {
            id = 0;

            // Max of 48 hrs for planning assignments
            if (DateTime.UtcNow.AddDays(2) < time)
            {
                return Error.AssignmentBadTime;
            }

            if (stub.TotalCount == 0)
            {
                return Error.TroopEmpty;
            }

            if (stub.City.Owner.Tribesman == null)
            {
                return Error.TribeNotFound;
            }

            // Cant attack other tribesman
            if (targetCity.Owner.Tribesman != null && targetCity.Owner.Tribesman.Tribe == stub.City.Owner.Tribesman.Tribe)
            {
                return Error.AssignmentCantAttackFriend;
            }

            // Player creating the assignment cannot be late (Give a few minutes lead)
            int distance = SimpleGameObject.TileDistance(stub.City.X, stub.City.Y, x, y);
            DateTime reachTime = DateTime.UtcNow.AddSeconds(Formula.Current.MoveTimeTotal(stub, distance, true));

            if (reachTime.Subtract(new TimeSpan(0, 1, 0)) > time)
            {
                return Error.AssignmentUnitsTooSlow;
            }

            // Create assignment
            Assignment assignment = Ioc.Kernel.Get<IAssignmentFactory>().CreateAssignment(this, x, y, targetCity, mode, time, stub);
            id = assignment.Id;
            assignments.Add(assignment.Id, assignment);
            assignment.AssignmentComplete += RemoveAssignment;
            assignment.Reschedule();

            SendUpdate();
            return Error.Ok;
        }

        public Error JoinAssignment(int id, ITroopStub stub)
        {
            Assignment assignment;
            if (assignments.TryGetValue(id, out assignment))
            {
                Error error = assignment.Add(stub);
                if (error != Error.Ok)
                {
                    return error;
                }
            }
            else
            {
                return Error.AssignmentNotFound;
            }
            return Error.Ok;
        }

        public void RemoveAssignment(Assignment assignment)
        {
            assignment.AssignmentComplete -= RemoveAssignment;
            assignments.Remove(assignment.Id);
            SendUpdate();
        }

        public void DbLoaderAddAssignment(Assignment assignment)
        {
            assignment.AssignmentComplete += RemoveAssignment;
            assignments.Add(assignment.Id, assignment);
        }

        public void Upgrade()
        {
            if (Level >= 20)
                return;

            Resource.Subtract(Formula.Current.GetTribeUpgradeCost(Level));
            Level++;
            DbPersistance.Current.Save(this);
        }

        public void SendUpdate()
        {
            Packet packet = new Packet(Command.TribeChannelNotification);
            packet.AddInt32(GetIncomingList().Count());
            packet.AddInt16(AssignmentCount);
            Global.Channel.Post("/TRIBE/" + Id, packet);
        }
    }
}
