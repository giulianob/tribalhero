using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text.RegularExpressions;
using Game.Comm;
using Game.Data.Stronghold;
using Game.Data.Tribe.EventArguments;
using Game.Data.Troop;
using Game.Logic.Actions;
using Game.Logic.Formulas;
using Game.Logic.Procedures;
using Game.Map;
using Game.Setup;
using Game.Util;
using Game.Util.Locking;
using Newtonsoft.Json;
using Persistance;

namespace Game.Data.Tribe
{
    public class Tribe : ITribe
    {
        public const string DB_TABLE = "tribes";

        public const int MEMBERS_PER_LEVEL = 5;

        public const int DAYS_BEFORE_REJOIN_ALLOWED = 2;

        public const int HOURS_BEFORE_SLOT_REOPENS = 8;

        public const int ASSIGNMENT_MIN_UPKEEP = 40;

        private readonly IAssignmentFactory assignmentFactory;

        private readonly Dictionary<int, Assignment> assignments = new Dictionary<int, Assignment>();

        private readonly ICityManager cityManager;

        private readonly IDbManager dbManager;

        private readonly Formula formula;

        private readonly Procedure procedure;

        private readonly IStrongholdManager strongholdManager;

        private readonly Dictionary<uint, ITribesman> tribesmen = new Dictionary<uint, ITribesman>();

        private readonly SortedDictionary<byte, ITribeRank> ranks = new SortedDictionary<byte, ITribeRank>();

        public List<LeavingTribesmate> LeavingTribesmates { get; private set; }

        private int attackPoint;

        private int defensePoint;

        private string description = string.Empty;

        private string publicDescription = string.Empty;

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
            LeavingTribesmates = new List<LeavingTribesmate>();

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

        public event EventHandler<EventArgs> RanksUpdated = (sender, args) => { };
        public event EventHandler<TribesmanEventArgs> TribesmanJoined = (sender, args) => { };
        public event EventHandler<TribesmanEventArgs> TribesmanLeft = (sender, args) => { };
        public event EventHandler<TribesmanKickedEventArgs> TribesmanKicked = (sender, args) => { };
        public event EventHandler<TribesmanContributedEventArgs> TribesmanContributed = (sender, args) => { };
        public event EventHandler<TribesmanEventArgs> TribesmanRankChanged = (sender, args) => { };
        public event EventHandler<StrongholdGainedEventArgs> StrongholdGained = (sender, args) => { };
        public event EventHandler<StrongholdLostEventArgs> StrongholdLost = (sender, args) => { };
        public event EventHandler<EventArgs> Upgraded = (sender, args) => { };

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

        public string PublicDescription
        {
            get
            {
                return publicDescription;
            }
            set
            {
                publicDescription = value;

                if (DbPersisted)
                {
                    dbManager.Query(
                                    string.Format("UPDATE `{0}` SET `public_desc` = @desc WHERE `id` = @id LIMIT 1", DB_TABLE),
                                    new[]
                                    {
                                            new DbColumn("desc", publicDescription, DbType.String, Player.MAX_DESCRIPTION_LENGTH),
                                            new DbColumn("id", Id, DbType.UInt32)
                                    });
                }
            }
        }

        public ITribeRank DefaultRank
        {
            get
            {
                return ranks.Values.Last();
            }
        }

        public ITribeRank ChiefRank
        {
            get
            {
                return ranks[0];
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

        public IEnumerable<ITribeRank> Ranks
        {
            get
            {
                return ranks.Values.AsEnumerable();
            }
        }

        public bool IsOwner(IPlayer player)
        {
            return player == Owner;
        }

        public void DbLoaderAdd(ITribesman tribesman)
        {
            tribesman.Player.Tribesman = tribesman;
            tribesmen.Add(tribesman.Player.PlayerId, tribesman);
        }

        public Error AddTribesman(ITribesman tribesman, bool ignoreRequirements = false)
        {
            if (tribesmen.ContainsKey(tribesman.Player.PlayerId))
            {
                return Error.TribesmanAlreadyInTribe;
            }

            if (!ignoreRequirements)
            {
                if (LeavingTribesmates.Any(p =>
                                            p.PlayerId == tribesman.Player.PlayerId &&
                                            SystemClock.Now.Subtract(p.TimeLeft).TotalDays < DAYS_BEFORE_REJOIN_ALLOWED))
                {
                    return Error.TribeCannotRejoinYet;
                }

                var totalSlots = Level * MEMBERS_PER_LEVEL -
                                 LeavingTribesmates.Count(p => SystemClock.Now.Subtract(p.TimeLeft).TotalHours < HOURS_BEFORE_SLOT_REOPENS);

                if (tribesmen.Count >= totalSlots)
                {
                    return Error.TribeFull;
                }
            }

            DbLoaderAdd(tribesman);

            DefaultMultiObjectLock.ThrowExceptionIfNotLocked(tribesman);
            dbManager.Save(tribesman);            

            if (tribesman.Player.Session != null)
            {
                Global.Channel.Subscribe(tribesman.Player.Session, "/TRIBE/" + Id);
            }

            TribesmanJoined(this, new TribesmanEventArgs {Player = tribesman.Player});
            return Error.Ok;
        }

        public Error KickTribesman(IPlayer player, IPlayer kicker)
        {
            Error error = RemoveTribesman(player.PlayerId, true);
            if (error == Error.Ok)
            {
                TribesmanKicked(this, new TribesmanKickedEventArgs {Kickee = player, Kicker = kicker});
            }
            return error;
        }

        public Error LeaveTribesman(IPlayer player)
        {
            Error error = RemoveTribesman(player.PlayerId, false);
            if (error == Error.Ok)
            {
                TribesmanLeft(this, new TribesmanEventArgs {Player = player});
            }
            return error;
        }

        public Error RemoveTribesman(uint playerId, bool wasKicked, bool doNotRemoveIfOwner = true)
        {
            ITribesman tribesman;
            if (!tribesmen.TryGetValue(playerId, out tribesman))
            {
                return Error.TribesmanNotFound;
            }

            IPlayer player = tribesman.Player;

            if (IsOwner(player))
            {
                if (doNotRemoveIfOwner)
                {
                    return Error.TribesmanIsOwner;
                }

                Owner = null;
            }

            DefaultMultiObjectLock.ThrowExceptionIfNotLocked(tribesman);

            player.Tribesman = null;
            dbManager.Delete(tribesman);
            tribesmen.Remove(playerId);
            
            // Keep logs of who entered/left tribe. First clean up the list of no longer needed records though.
            LeavingTribesmates.RemoveAll(p => p.PlayerId == player.PlayerId || SystemClock.Now.Subtract(p.TimeLeft).TotalDays > DAYS_BEFORE_REJOIN_ALLOWED);
            LeavingTribesmates.Add(new LeavingTribesmate {PlayerId = player.PlayerId, TimeLeft = SystemClock.Now});

            // Save to save owner and leaving tribesmates
            dbManager.Save(this);

            // TODO: Move event out
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

            previousOwnerTribesman.Rank = newOwnerTribesman.Rank;
            newOwnerTribesman.Rank = ChiefRank;
            Owner = newOwnerTribesman.Player;

            dbManager.Save(previousOwnerTribesman, newOwnerTribesman, this);

            previousOwnerTribesman.Player.TribeUpdate();
            newOwnerTribesman.Player.TribeUpdate();

            return Error.Ok;
        }

        public void CreateRank(byte id, string name, TribePermission permission)
        {
            var rank = new TribeRank(id) {Name = name, Permission = permission};
            ranks[rank.Id] = rank;
        }

        public Error SetRank(uint playerId, byte rank)
        {
            ITribesman tribesman;
            ITribeRank tribeRank;

            if (rank == 0)
            {
                return Error.TribesmanNotAuthorized;
            }

            if (!tribesmen.TryGetValue(playerId, out tribesman))
            {
                return Error.TribesmanNotFound;
            }

            if (!ranks.TryGetValue(rank, out tribeRank))
            {
                return Error.TribeRankNotFound;
            }

            DefaultMultiObjectLock.ThrowExceptionIfNotLocked(tribesman);

            if (IsOwner(tribesman.Player))
            {
                return Error.TribesmanIsOwner;
            }

            tribesman.Rank = tribeRank;
            dbManager.Save(tribesman);
            tribesman.Player.TribeUpdate();

            TribesmanRankChanged(this, new TribesmanEventArgs {Player = tribesman.Player});
            return Error.Ok;
        }

        public Error UpdateRank(byte rank, string name, TribePermission permission)
        {
            ITribeRank tribeRank;
            if (!ranks.TryGetValue(rank, out tribeRank))
            {
                return Error.TribeRankNotFound;
            }

            if (!TribeRank.IsNameValid(name))
            {
                return Error.TribeRankInvalidName;
            }

            tribeRank.Name = name;
            if (tribeRank != ChiefRank)
            {
                tribeRank.Permission = permission;
            }
            dbManager.Save(this);
            SendRanksUpdate();
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

            TribesmanContributed(this, new TribesmanContributedEventArgs {Player = tribesman.Player, Resource = resource});
            return Error.Ok;
        }

        public bool HasRight(uint playerId, TribePermission permission)
        {
            ITribesman tribesman;
            if (!TryGetTribesman(playerId, out tribesman))
            {
                return false;
            }
            return tribesman.Rank.Permission.HasFlag(TribePermission.All) ||
                   tribesman.Rank.Permission.HasFlag(permission);
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

            if (assignments.Count > 20)
            {
                return Error.AssignmentTooManyInProgress;                
            }

            // Create troop
            ITroopStub stub;
            if (!procedure.TroopStubCreate(out stub,
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
            
            if (stub.Upkeep < ASSIGNMENT_MIN_UPKEEP)
            {
                Procedure.Current.TroopStubDelete(city, stub);
                return Error.AssignmentTooFewTroops;
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
            else
            {
                Procedure.Current.TroopStubDelete(city, stub);
                return Error.ObjectNotAttackable;                
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

            if (assignment.Target == city)
            {
                return Error.AssignmentNotEligible;
            }

            ITroopStub stub;
            if (!procedure.TroopStubCreate(out stub,
                                           city,
                                           simpleStub,
                                           assignment.IsAttack 
                                                   ? TroopState.WaitingInOffensiveAssignment
                                                   : TroopState.WaitingInDefensiveAssignment,
                                           assignment.IsAttack ? FormationType.Attack : FormationType.Defense))
            {
                return Error.TroopChanged;
            }

            if (stub.Upkeep < ASSIGNMENT_MIN_UPKEEP)
            {
                Procedure.Current.TroopStubDelete(city, stub);
                return Error.AssignmentTooFewTroops;
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

            Upgraded(this, new EventArgs());
            return Error.Ok;
        }

        public void SendUpdate()
        {
            Updated(this, new EventArgs());
        }

        public void SendRanksUpdate()
        {
            RanksUpdated(this, new EventArgs());
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
                   Regex.IsMatch(tribeName, Global.ALPHANUMERIC_NAME, RegexOptions.IgnoreCase);
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
                        new DbColumn("ranks", JsonConvert.SerializeObject(Ranks), DbType.String),
                        new DbColumn("leaving_tribesmates", JsonConvert.SerializeObject(LeavingTribesmates), DbType.String)
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