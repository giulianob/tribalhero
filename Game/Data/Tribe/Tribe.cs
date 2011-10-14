using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text.RegularExpressions;
using Game.Data.Troop;
using Game.Logic.Actions;
using Game.Logic.Formulas;
using Game.Setup;
using Game.Util;
using Ninject;
using Persistance;

namespace Game.Data.Tribe
{
    public class Tribe : ILockable, IEnumerable<Tribesman>, IPersistableObject, IEnumerable<Assignment>
    {
        public class IncomingListItem
        {
            public City City { get; set; }
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
        public Player Owner { get; private set; }
        public string Name { get; set; }
        public byte Level { get; set; }

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
                    Ioc.Kernel.Get<IDbManager>().Query(string.Format("UPDATE `{0}` SET `desc` = @desc WHERE `player_id` = @id LIMIT 1", DB_TABLE),
                                           new[] { new DbColumn("desc", description, DbType.String, Player.MAX_DESCRIPTION_LENGTH), new DbColumn("id", Id, DbType.UInt32) });
                }
            }
        }

        readonly Dictionary<uint, Tribesman> tribesmen = new Dictionary<uint, Tribesman>();
        readonly Dictionary<int, Assignment> assignments = new Dictionary<int, Assignment>();

        public Resource Resource { get; private set; }
        public short AssignmentCount { get { return (short)assignments.Count; } }

        public Tribe(Player owner, string name) :
            this(owner, name, string.Empty, 1, new Resource())
        {

        }

        public Tribe(Player owner, string name, string desc, byte level, Resource resource)
        {
            Owner = owner;
            Level = level;
            Resource = resource;
            Description = desc;
            Name = name;
        }

        public bool IsOwner(Player player)
        {
            return player.PlayerId == Id;
        }

        public Error AddTribesman(Tribesman tribesman, bool save = true)
        {
            if (tribesmen.ContainsKey(tribesman.Player.PlayerId))
                return Error.TribesmanAlreadyExists;

            if (tribesmen.Count >= Level*MEMBERS_PER_LEVEL)
                return Error.TribeFull;

            tribesman.Player.Tribesman = tribesman;
            tribesmen.Add(tribesman.Player.PlayerId, tribesman);
            if (save)
            {
                MultiObjectLock.ThrowExceptionIfNotLocked(tribesman);
                Ioc.Kernel.Get<IDbManager>().Save(tribesman);
            }
            return Error.Ok;
        }

        public Error RemoveTribesman(uint playerId)
        {
            Tribesman tribesman;
            if (!tribesmen.TryGetValue(playerId, out tribesman))
                return Error.TribesmanNotFound;

            MultiObjectLock.ThrowExceptionIfNotLocked(tribesman);
            tribesman.Player.Tribesman = null;
            Ioc.Kernel.Get<IDbManager>().Delete(tribesman);
            return !tribesmen.Remove(playerId) ? Error.TribesmanNotFound : Error.Ok;
        }

        public bool TryGetTribesman(uint playerId, out Tribesman tribesman)
        {
            return tribesmen.TryGetValue(playerId, out tribesman);
        }

        public Error SetRank(uint playerId, byte rank)
        {
            Tribesman tribesman;
            
            if (rank == 0)
                return Error.TribesmanNotAuthorized;
            
            if (!tribesmen.TryGetValue(playerId, out tribesman))
                return Error.TribesmanNotFound;
            
            MultiObjectLock.ThrowExceptionIfNotLocked(tribesman);
            
            if (IsOwner(tribesman.Player))
                return Error.TribesmanIsOwner;

            tribesman.Rank = rank;
            Ioc.Kernel.Get<IDbManager>().Save(tribesman);
            tribesman.Player.TribeUpdate();

            return Error.Ok;
        }

        public Error Contribute(uint playerId, Resource resource)
        {
            Tribesman tribesman;
            if (!tribesmen.TryGetValue(playerId, out tribesman)) 
                return Error.TribesmanNotFound;
            
            MultiObjectLock.ThrowExceptionIfNotLocked(tribesman);
            tribesman.Contribution += resource;
            Resource += resource;
            Ioc.Kernel.Get<IDbManager>().Save(tribesman, this);

            return Error.Ok;
        }

        public IEnumerable<IncomingListItem> GetIncomingList()
        {
            return from tribesmen in ((IEnumerable<Tribesman>)this)
                   from city in tribesmen.Player.GetCityList()
                   from notification in city.Worker.Notifications
                   where notification.Action is AttackChainAction && notification.Action.WorkerObject.City != city && notification.Subscriptions.Count > 0
                   orderby ((ChainAction)notification.Action).EndTime ascending
                   select new IncomingListItem { Action = (AttackChainAction)notification.Action };
        }

        public bool HasRight(uint playerId, string action)
        {
            Tribesman tribesman;
            if (!TryGetTribesman(playerId, out tribesman)) return false;

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


        #region IEnumerable<Tribesman> Members

        IEnumerator<Assignment> IEnumerable<Assignment>.GetEnumerator()
        {
            return assignments.Values.GetEnumerator();
        }

        public IEnumerator<Tribesman> GetEnumerator()
        {
            return tribesmen.Values.GetEnumerator();
        }

        #endregion

        #region IEnumerable Members

        IEnumerator IEnumerable.GetEnumerator()
        {
            return tribesmen.Values.GetEnumerator();
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

        public Error CreateAssignment(TroopStub stub, uint x, uint y, City targetCity, DateTime time, AttackMode mode, out int id)
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
            DateTime reachTime =
                    DateTime.UtcNow.AddSeconds((int)(Formula.MoveTime(Formula.GetTroopSpeed(stub))*Formula.MoveTimeMod(stub.City, distance, true))*distance*
                                               Config.seconds_per_unit);

            if (reachTime.Subtract(new TimeSpan(0, 1, 0)) > time)
            {
                return Error.AssignmentUnitsTooSlow;
            }

            // Create assignment
            Assignment assignment = new Assignment(this, x, y, targetCity, mode, time, stub);
            id = assignment.Id;
            assignments.Add(assignment.Id, assignment);
            assignment.AssignmentComplete += RemoveAssignment;

            return Error.Ok;
        }

        internal Error JoinAssignment(int id, TroopStub stub) {
            Assignment assignment;
            if(assignments.TryGetValue(id,out assignment))
            {
                Error error = assignment.Add(stub);
                if(error!=Error.Ok)
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

        internal void RemoveAssignment(Assignment assignment)
        {
            assignments.Remove(assignment.Id);
        }

        internal void DbLoaderAddAssignment(Assignment assignment) {
            assignment.AssignmentComplete += RemoveAssignment;
            assignments.Add(assignment.Id,assignment);
        }

        public void Upgrade()
        {
            if (Level >= 20)
                return;

            Resource.Subtract(Formula.GetTribeUpgradeCost(Level));
            Level++;
            Ioc.Kernel.Get<IDbManager>().Save(this);
        }
    }
}
