using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text.RegularExpressions;
using Game.Comm;
using Game.Data.Stronghold;
using Game.Data.Troop;
using Game.Logic.Actions;
using Game.Logic.Formulas;
using Game.Logic.Procedures;
using Game.Map;
using Game.Setup;
using Game.Util;
using Game.Util.Locking;
using Persistance;

namespace Game.Data.Tribe
{
    public class Tribe : ITribe
    {
        public const string DB_TABLE = "tribes";

        public const int MEMBERS_PER_LEVEL = 5;

        private readonly IAssignmentFactory assignmentFactory;

        private readonly Dictionary<int, Assignment> assignments = new Dictionary<int, Assignment>();

        private readonly ICityManager cityManager;

        private readonly IDbManager dbManager;

        private readonly Formula formula;

        private readonly Procedure procedure;

        private readonly IStrongholdManager strongholdManager;

        private readonly Dictionary<uint, ITribesman> tribesmen = new Dictionary<uint, ITribesman>();

        private int attackPoint;

        private int defensePoint;

        private string description = string.Empty;

        public Tribe(IPlayer owner,
                     string name,
                     Procedure procedure,
                     IDbManager dbManager,
                     Formula formula,
                     IAssignmentFactory assignmentFactory,
                     ICityManager cityManager,
                     IStrongholdManager strongholdManager)
                : this(
                        owner: owner,
                        name: name,
                        desc: string.Empty,
                        level: 1,
                        victoryPoints: 0,
                        attackPoints: 0,
                        defensePoints: 0,
                        resource: new Resource(),
                        created: SystemClock.Now,
                        procedure: procedure,
                        dbManager: dbManager,
                        formula: formula,
                        assignmentFactory: assignmentFactory,
                        cityManager: cityManager,
                        strongholdManager: strongholdManager)
        {
        }

        public Tribe(IPlayer owner,
                     string name,
                     string desc,
                     byte level,
                     decimal victoryPoints,
                     int attackPoints,
                     int defensePoints,
                     Resource resource,
                     DateTime created,
                     Procedure procedure,
                     IDbManager dbManager,
                     Formula formula,
                     IAssignmentFactory assignmentFactory,
                     ICityManager cityManager,
                     IStrongholdManager strongholdManager)
        {
            this.procedure = procedure;
            this.dbManager = dbManager;
            this.formula = formula;
            this.assignmentFactory = assignmentFactory;
            this.cityManager = cityManager;
            this.strongholdManager = strongholdManager;
            Owner = owner;
            Level = level;
            Resource = resource;
            Description = desc;
            Name = name;
            VictoryPoint = victoryPoints;
            AttackPoint = attackPoints;
            DefensePoint = defensePoints;
            Created = created;
        }

        public event EventHandler<TribesmanRemovedEventArgs> TribesmanRemoved = (sender, args) => { };

        public event EventHandler<EventArgs> Updated = (sender, args) => { };

        public uint Id { set; get; }

        public IPlayer Owner { get; private set; }

        public string Name { get; set; }

        public byte Level { get; set; }

        public decimal VictoryPoint { get; set; }

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
                    dbManager.Query(
                                    string.Format(
                                                  "UPDATE `{0}` SET `attack_point` = @attack_point WHERE `id` = @id LIMIT 1",
                                                  DB_TABLE),
                                    new[]
                                    {
                                            new DbColumn("attack_point", attackPoint, DbType.Int32),
                                            new DbColumn("id", Id, DbType.UInt32)
                                    });
                }
            }
        }

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
                    dbManager.Query(
                                    string.Format(
                                                  "UPDATE `{0}` SET `defense_point` = @defense_point WHERE `id` = @id LIMIT 1",
                                                  DB_TABLE),
                                    new[]
                                    {
                                            new DbColumn("defense_point", defensePoint, DbType.Int32),
                                            new DbColumn("id", Id, DbType.UInt32)
                                    });
                }
            }
        }

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
                    dbManager.Query(
                                    string.Format("UPDATE `{0}` SET `desc` = @desc WHERE `id` = @id LIMIT 1", DB_TABLE),
                                    new[]
                                    {
                                            new DbColumn("desc", description, DbType.String, Player.MAX_DESCRIPTION_LENGTH),
                                            new DbColumn("id", Id, DbType.UInt32)
                                    });
                }
            }
        }

        public Resource Resource { get; private set; }

        public DateTime Created { get; private set; }

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

        public bool IsOwner(IPlayer player)
        {
            return player == Owner;
        }

        public Error AddTribesman(ITribesman tribesman, bool save = true)
        {
            if (tribesmen.ContainsKey(tribesman.Player.PlayerId))
            {
                return Error.TribesmanAlreadyExists;
            }

            if (tribesmen.Count >= Level * MEMBERS_PER_LEVEL)
            {
                return Error.TribeFull;
            }

            tribesman.Player.Tribesman = tribesman;
            tribesmen.Add(tribesman.Player.PlayerId, tribesman);

            if (save)
            {
                DefaultMultiObjectLock.ThrowExceptionIfNotLocked(tribesman);
                dbManager.Save(tribesman);
            }

            if (tribesman.Player.Session != null)
            {
                Global.Channel.Subscribe(tribesman.Player.Session, "/TRIBE/" + Id);
            }

            return Error.Ok;
        }

        public Error RemoveTribesman(uint playerId, bool wasKicked, bool checkIfOwner = true)
        {
            ITribesman tribesman;
            if (!tribesmen.TryGetValue(playerId, out tribesman))
            {
                return Error.TribesmanNotFound;
            }

            IPlayer player = tribesman.Player;

            if (IsOwner(player))
            {
                if (checkIfOwner)
                {
                    return Error.TribesmanIsOwner;
                }

                Owner = null;
                dbManager.Save(this);
            }

            DefaultMultiObjectLock.ThrowExceptionIfNotLocked(tribesman);

            player.Tribesman = null;

            dbManager.Delete(tribesman);

            tribesmen.Remove(playerId);

            if (player.Session != null)
            {
                Global.Channel.Unsubscribe(player.Session, "/TRIBE/" + Id);

                if (wasKicked)
                {
                    player.Session.Write(new Packet(Command.TribesmanKicked));
                }
            }

            TribesmanRemoved(this, new TribesmanRemovedEventArgs {Player = player});

            return Error.Ok;
        }

        public Error Transfer(uint newOwnerPlayerId)
        {
            if (Owner.PlayerId == newOwnerPlayerId)
            {
                return Error.TribesmanIsOwner;
            }

            ITribesman newOwnerTribesman;

            if (!tribesmen.TryGetValue(newOwnerPlayerId, out newOwnerTribesman))
            {
                return Error.TribesmanNotFound;
            }

            var previousOwnerTribesman = Owner.Tribesman;

            previousOwnerTribesman.Rank = 1;
            newOwnerTribesman.Rank = 0;
            Owner = newOwnerTribesman.Player;

            dbManager.Save(previousOwnerTribesman, newOwnerTribesman, this);

            previousOwnerTribesman.Player.TribeUpdate();
            newOwnerTribesman.Player.TribeUpdate();

            return Error.Ok;
        }

        public Error SetRank(uint playerId, byte rank)
        {
            ITribesman tribesman;

            if (rank == 0)
            {
                return Error.TribesmanNotAuthorized;
            }

            if (!tribesmen.TryGetValue(playerId, out tribesman))
            {
                return Error.TribesmanNotFound;
            }

            DefaultMultiObjectLock.ThrowExceptionIfNotLocked(tribesman);

            if (IsOwner(tribesman.Player))
            {
                return Error.TribesmanIsOwner;
            }

            tribesman.Rank = rank;
            dbManager.Save(tribesman);
            tribesman.Player.TribeUpdate();

            return Error.Ok;
        }

        public Error Contribute(uint playerId, Resource resource)
        {
            ITribesman tribesman;
            if (!tribesmen.TryGetValue(playerId, out tribesman))
            {
                return Error.TribesmanNotFound;
            }

            DefaultMultiObjectLock.ThrowExceptionIfNotLocked(tribesman);
            tribesman.Contribution += resource;
            Resource += resource;
            dbManager.Save(tribesman, this);

            return Error.Ok;
        }

        public bool HasRight(uint playerId, string action)
        {
            ITribesman tribesman;
            if (!TryGetTribesman(playerId, out tribesman))
            {
                return false;
            }

            switch(action)
            {
                case "Request":
                case "Assignment":
                case "Kick":
                case "Repair":
                    switch(tribesman.Rank)
                    {
                        case 0:
                        case 1:
                            return true;
                    }
                    break;
            }

            return false;
        }

        public Error CreateAssignment(ICity city,
                                      ISimpleStub simpleStub,
                                      uint x,
                                      uint y,
                                      ILocation target,
                                      DateTime time,
                                      AttackMode mode,
                                      string desc,
                                      bool isAttack,
                                      out int id)
        {
            id = 0;

            // Create troop      
            ITroopStub stub;
            if (
                    !procedure.TroopStubCreate(out stub,
                                               city,
                                               simpleStub,
                                               isAttack
                                                       ? TroopState.WaitingInOffensiveAssignment
                                                       : TroopState.WaitingInDefensiveAssignment))
            {
                return Error.TroopChanged;
            }

            // Max of 48 hrs for planning assignments
            if (DateTime.UtcNow.AddDays(2) < time)
            {
                Procedure.Current.TroopStubDelete(city, stub);
                return Error.AssignmentBadTime;
            }

            if (stub.TotalCount == 0)
            {
                Procedure.Current.TroopStubDelete(city, stub);
                return Error.TroopEmpty;
            }

            if (stub.City.Owner.Tribesman == null)
            {
                Procedure.Current.TroopStubDelete(city, stub);
                return Error.TribeNotFound;
            }

            if (target.LocationType == LocationType.City)
            {
                ICity targetCity;
                if (!cityManager.TryGetCity(target.LocationId, out targetCity))
                {
                    Procedure.Current.TroopStubDelete(city, stub);
                    return Error.CityNotFound;
                }

                // Cant attack other tribesman
                if (isAttack && targetCity.Owner.Tribesman != null &&
                    targetCity.Owner.Tribesman.Tribe == stub.City.Owner.Tribesman.Tribe)
                {
                    Procedure.Current.TroopStubDelete(city, stub);
                    return Error.AssignmentCantAttackFriend;
                }

                // Cant defend the same city
                if (targetCity == stub.City)
                {
                    Procedure.Current.TroopStubDelete(city, stub);
                    return Error.DefendSelf;
                }
            }
            else if (target.LocationType == LocationType.Stronghold)
            {
                IStronghold stronghold;
                if (!strongholdManager.TryGetStronghold(target.LocationId, out stronghold))
                {
                    Procedure.Current.TroopStubDelete(city, stub);
                    return Error.StrongholdNotFound;
                }

                // Cant attack your stronghold
                if (isAttack && stronghold.Tribe == stub.City.Owner.Tribesman.Tribe)
                {
                    Procedure.Current.TroopStubDelete(city, stub);
                    return Error.AttackSelf;
                }
                // Cant defend other tribe's stronghold
                if (!isAttack && stronghold.Tribe != stub.City.Owner.Tribesman.Tribe)
                {
                    Procedure.Current.TroopStubDelete(city, stub);
                    return Error.DefendSelf;
                }
            }

            // Player creating the assignment cannot be late (Give a few minutes lead)
            int distance = SimpleGameObject.TileDistance(stub.City.X, stub.City.Y, x, y);
            DateTime reachTime = DateTime.UtcNow.AddSeconds(formula.MoveTimeTotal(stub, distance, true));

            if (reachTime.Subtract(new TimeSpan(0, 1, 0)) > time)
            {
                Procedure.Current.TroopStubDelete(city, stub);
                return Error.AssignmentUnitsTooSlow;
            }

            // Create assignment
            Assignment assignment = assignmentFactory.CreateAssignment(this, x, y, target, mode, time, desc, isAttack);
            id = assignment.Id;
            assignments.Add(assignment.Id, assignment);
            assignment.AssignmentComplete += RemoveAssignment;
            var result = assignment.Add(stub);

            if (result != Error.Ok)
            {
                Procedure.Current.TroopStubDelete(city, stub);
            }

            SendUpdate();
            return result;
        }

        public Error JoinAssignment(int id, ICity city, ISimpleStub simpleStub)
        {
            Assignment assignment;

            if (!assignments.TryGetValue(id, out assignment))
            {
                return Error.AssignmentNotFound;
            }

            if (simpleStub.TotalCount == 0)
            {
                return Error.TroopEmpty;
            }

            if (assignment.Target == city)
            {
                return Error.AssignmentNotEligible;
            }

            ITroopStub stub;
            if (
                    !procedure.TroopStubCreate(out stub,
                                               city,
                                               simpleStub,
                                               assignment.IsAttack
                                                       ? TroopState.WaitingInOffensiveAssignment
                                                       : TroopState.WaitingInDefensiveAssignment,
                                               assignment.IsAttack ? FormationType.Attack : FormationType.Defense))
            {
                return Error.TroopChanged;
            }

            var error = assignment.Add(stub);

            if (error != Error.Ok)
            {
                Procedure.Current.TroopStubDelete(city, stub);
            }

            return error;
        }

        public void DbLoaderAddAssignment(Assignment assignment)
        {
            assignment.AssignmentComplete += RemoveAssignment;
            assignments.Add(assignment.Id, assignment);
        }

        public Error Upgrade()
        {
            if (Level >= 20)
            {
                return Error.TribeMaxLevel;
            }

            Resource cost = formula.GetTribeUpgradeCost(Level);
            if (!Resource.HasEnough(cost))
            {
                return Error.ResourceNotEnough;
            }

            Resource.Subtract(formula.GetTribeUpgradeCost(Level));
            Level++;
            dbManager.Save(this);

            return Error.Ok;
        }

        public void SendUpdate()
        {
            Updated(this, new EventArgs());
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
            get
            {
                return unchecked((int)Owner.PlayerId);
            }
        }

        public object Lock
        {
            get
            {
                return Owner;
            }
        }

        #endregion

        public bool TryGetTribesman(uint playerId, out ITribesman tribesman)
        {
            return tribesmen.TryGetValue(playerId, out tribesman);
        }

        public static bool IsNameValid(string tribeName)
        {
            return tribeName != string.Empty && tribeName.Length >= 3 && tribeName.Length <= 20 &&
                   Regex.IsMatch(tribeName, "^([a-z][a-z0-9\\s].*)$", RegexOptions.IgnoreCase);
        }

        public void RemoveAssignment(Assignment assignment)
        {
            assignment.AssignmentComplete -= RemoveAssignment;
            assignments.Remove(assignment.Id);
            SendUpdate();
        }

        #region IPersistableObject Members

        public bool DbPersisted { get; set; }

        #endregion

        #region IPersistable Members

        public string DbTable
        {
            get
            {
                return DB_TABLE;
            }
        }

        public DbColumn[] DbPrimaryKey
        {
            get
            {
                return new[] {new DbColumn("id", Id, DbType.UInt32)};
            }
        }

        public IEnumerable<DbDependency> DbDependencies
        {
            get
            {
                return new DbDependency[] {};
            }
        }

        public DbColumn[] DbColumns
        {
            get
            {
                return new[]
                {
                        new DbColumn("owner_player_id", Owner != null ? Owner.PlayerId : 0, DbType.UInt32),
                        new DbColumn("name", Name, DbType.String, 20), new DbColumn("level", Level, DbType.Byte),
                        new DbColumn("crop", Resource.Crop, DbType.Int32),
                        new DbColumn("gold", Resource.Gold, DbType.Int32),
                        new DbColumn("iron", Resource.Iron, DbType.Int32),
                        new DbColumn("wood", Resource.Wood, DbType.Int32),
                        new DbColumn("created", Created, DbType.DateTime),
                        new DbColumn("victory_point", VictoryPoint, DbType.Int32),
                };
            }
        }

        #endregion

        public class IncomingListItem
        {
            public ILocation Target { get; set; }

            public ILocation Source { get; set; }

            public DateTime EndTime { get; set; }
        }
    }
}