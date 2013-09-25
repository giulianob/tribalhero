using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using Game.Data.Troop;
using Game.Logic;
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
    public partial class Assignment : ISchedule, IPersistableList, IEnumerable<Assignment.AssignmentTroop>
    {
        public delegate void OnComplete(Assignment assignment);

        /// <summary>
        ///     Table name where Assignment gets persisted to
        /// </summary>
        public const string DB_TABLE = "Assignments";

        /// <summary>
        ///     Assignment id generator
        /// </summary>
        private static readonly LargeIdGenerator IdGen = new LargeIdGenerator(int.MaxValue);

        private readonly IActionFactory actionFactory;

        /// <summary>
        ///     Lock for the assignment when adding/removing stubs
        /// </summary>
        private readonly object assignmentLock = new object();

        /// <summary>
        ///     List of stubs in this assignment
        /// </summary>
        private readonly List<AssignmentTroop> assignmentTroops = new List<AssignmentTroop>();

        private readonly IDbManager dbManager;

        private readonly Formula formula;

        private readonly IGameObjectLocator gameObjectLocator;

        private readonly Procedure procedure;

        private readonly IScheduler scheduler;

        private readonly ITileLocator tileLocator;

        private readonly ILocker locker;

        private readonly ITroopObjectInitializerFactory troopObjectInitializerFactory;

        /// <summary>
        ///     Creates a new assignment.
        ///     An id will be assigned and the stub passed in will be added to the assignment. This will not schedule the assignment!
        /// </summary>
        public Assignment(ITribe tribe,
                          uint x,
                          uint y,
                          ILocation target,
                          AttackMode mode,
                          DateTime targetTime,
                          string description,
                          bool isAttack,
                          Formula formula,
                          IDbManager dbManager,
                          IGameObjectLocator gameObjectLocator,
                          IScheduler scheduler,
                          Procedure procedure,
                          ITileLocator tileLocator,
                          IActionFactory actionFactory,
                          ILocker locker,
                          ITroopObjectInitializerFactory troopObjectInitializerFactory)
        {
            this.formula = formula;
            this.dbManager = dbManager;
            this.gameObjectLocator = gameObjectLocator;
            this.scheduler = scheduler;
            this.procedure = procedure;
            this.tileLocator = tileLocator;
            this.actionFactory = actionFactory;
            this.locker = locker;
            this.troopObjectInitializerFactory = troopObjectInitializerFactory;

            Id = (int)IdGen.GetNext();
            Tribe = tribe;
            TargetTime = targetTime;
            Target = target;
            X = x;
            Y = y;
            AttackMode = mode;
            DispatchCount = 0;
            Description = description;
            IsAttack = isAttack;
        }

        /// <summary>
        ///     Creates a new assignment.
        ///     NOTE: This constructor is used by the db loader. Use the other constructor when creating a new assignment from scratch.
        /// </summary>
        public Assignment(int id,
                          ITribe tribe,
                          uint x,
                          uint y,
                          ILocation target,
                          AttackMode mode,
                          DateTime targetTime,
                          uint dispatchCount,
                          string description,
                          bool isAttack,
                          Formula formula,
                          IDbManager dbManager,
                          IGameObjectLocator gameObjectLocator,
                          IScheduler scheduler,
                          Procedure procedure,
                          ITileLocator tileLocator,
                          IActionFactory actionFactory,
                          ILocker locker,
                          ITroopObjectInitializerFactory troopObjectInitializerFactory)
        {
            this.formula = formula;
            this.dbManager = dbManager;
            this.gameObjectLocator = gameObjectLocator;
            this.scheduler = scheduler;
            this.procedure = procedure;
            this.tileLocator = tileLocator;
            this.actionFactory = actionFactory;
            this.locker = locker;
            this.troopObjectInitializerFactory = troopObjectInitializerFactory;

            Id = id;
            Tribe = tribe;
            TargetTime = targetTime;
            Target = target;
            X = x;
            Y = y;
            AttackMode = mode;
            DispatchCount = dispatchCount;
            Description = description;
            IsAttack = isAttack;

            IdGen.Set((uint)id);
        }

        /// <summary>
        ///     Id of the assignment
        /// </summary>
        public int Id { get; private set; }

        /// <summary>
        ///     Tribe assignment belongs to
        /// </summary>
        private ITribe Tribe { get; set; }

        /// <summary>
        ///     City this assignment is targetting
        /// </summary>
        public ILocation Target { get; private set; }

        /// <summary>
        ///     Time that the assignment will end (time it should reach its target)
        /// </summary>
        public DateTime TargetTime { get; private set; }

        /// <summary>
        ///     X coordinate to attack
        /// </summary>
        public uint X { get; private set; }

        /// <summary>
        ///     Y coordinate to attack
        /// </summary>
        public uint Y { get; private set; }

        /// <summary>
        ///     Attack strength of assignment
        /// </summary>
        public AttackMode AttackMode { get; private set; }

        /// <summary>
        ///     Description of assignement
        /// </summary>
        public string Description { get; private set; }

        /// <summary>
        ///     Number of stubs that have already been dispatched
        /// </summary>
        public uint DispatchCount { get; private set; }

        /// <summary>
        ///     Type of assignnment, defensive or offensive
        /// </summary>
        public bool IsAttack { get; private set; }

        /// <summary>
        ///     Number of troops in this assignment (some might have been dispatched already)
        /// </summary>
        public int TroopCount
        {
            get
            {
                return assignmentTroops.Count;
            }
        }

        public IEnumerable<DbColumn[]> DbListValues()
        {
            var itr = assignmentTroops.GetEnumerator();
            while (itr.MoveNext())
            {
                if (itr.Current == null)
                {
                    throw new NullReferenceException("itr.current should not be null");
                }

                yield return
                        new[]
                        {
                                new DbColumn("city_id", itr.Current.Stub.City.Id, DbType.UInt32),
                                new DbColumn("stub_id", itr.Current.Stub.TroopId, DbType.UInt16),
                                new DbColumn("dispatched", itr.Current.Dispatched ? (byte)1 : (byte)0, DbType.Byte)
                        };
            }
        }

        #region IEnumerable Members

        IEnumerator IEnumerable.GetEnumerator()
        {
            return assignmentTroops.GetEnumerator();
        }

        IEnumerator<AssignmentTroop> IEnumerable<AssignmentTroop>.GetEnumerator()
        {
            return assignmentTroops.GetEnumerator();
        }

        #endregion

        /// <summary>
        ///     Event fired when assignment completes (all stubs should be gone by this time)
        /// </summary>
        public event OnComplete AssignmentComplete = delegate { };

        public string ToNiceString()
        {
            StringBuilder stringBuilder = new StringBuilder();

            lock (assignmentLock)
            {
                stringBuilder.Append(
                                     string.Format(
                                                   "Id[{0}] Time[{1}] x[{2}] y[{3}] mode[{4}] # of stubs[{5}] pispatched[{6}]\n",
                                                   Id,
                                                   TargetTime,
                                                   X,
                                                   Y,
                                                   Enum.GetName(typeof(AttackMode), AttackMode),
                                                   assignmentTroops.Count,
                                                   DispatchCount));
                foreach (var obj in assignmentTroops)
                {
                    ITroopStub stub = obj.Stub;
                    stringBuilder.Append(string.Format("\tTime[{0}] Player[{1}] City[{2}] Stub[{3}] Upkeep[{4}]\n",
                                                       obj.DepartureTime,
                                                       stub.City.Owner.Name,
                                                       stub.City.Name,
                                                       stub.TroopId,
                                                       stub.Upkeep));
                }
            }

            return stringBuilder.ToString();
        }

        /// <summary>
        ///     Add a stub and reschedule's the assignment
        /// </summary>
        /// <param name="stub"></param>
        /// <returns></returns>
        public Error Add(ITroopStub stub)
        {
            lock (assignmentLock)
            {
                // Don't allow ppl to join in the last minute
                if (SystemClock.Now.AddMinutes(1) >= TargetTime)
                {
                    return Error.AssignmentDone;
                }

                assignmentTroops.Add(new AssignmentTroop(stub, DepartureTime(stub)));
                stub.OnRemoved += OnStubRemoved;
                stub.OnStateSwitched += StubOnStateSwitched;

                Reschedule();
            }

            return Error.Ok;
        }

        /// <summary>
        ///     Handler when an event switches state. This is how we know to remove a stub from the assignment once it joins the battle.
        /// </summary>
        /// <param name="stub"></param>
        /// <param name="newState"></param>
        private void StubOnStateSwitched(ITroopStub stub, TroopState newState)
        {
            lock (assignmentLock)
            {
                switch(newState)
                {
                    case TroopState.Battle:
                        RemoveStub(stub);
                        Reschedule();
                        break;
                }
            }
        }

        /// <summary>
        ///     Called when a stub is deleted for good
        /// </summary>
        /// <param name="stub"></param>
        private void OnStubRemoved(ITroopStub stub)
        {
            RemoveStub(stub);
            Reschedule();
        }

        /// <summary>
        ///     Removes a stub from the assignment
        /// </summary>
        /// <param name="stub"></param>
        private void RemoveStub(ITroopStub stub)
        {
            lock (assignmentLock)
            {
                var assignmentTroop = assignmentTroops.FirstOrDefault(x => x.Stub == stub);

                if (assignmentTroop == null)
                {
                    return;
                }

                stub.OnRemoved -= OnStubRemoved;
                stub.OnStateSwitched -= StubOnStateSwitched;
                assignmentTroops.Remove(assignmentTroop);
            }
        }

        /// <summary>
        ///     Calculates the departure time for a given troop stub
        /// </summary>
        /// <param name="stub"></param>
        /// <returns></returns>
        private DateTime DepartureTime(ITroopStub stub)
        {
            int distance = tileLocator.TileDistance(stub.City.PrimaryPosition, 1, new Position(X, Y), 1);
            return TargetTime.Subtract(TimeSpan.FromSeconds(formula.MoveTimeTotal(stub, distance, IsAttack)));
        }

        /// <summary>
        ///     Called to dispatch a unit
        /// </summary>
        /// <param name="stub"></param>
        /// <returns></returns>
        private bool Dispatch(ITroopStub stub)
        {
            // Create troop object
            ITroopObject troopObject;
            procedure.TroopObjectCreate(stub.City, stub, out troopObject);

            PassiveAction action;
            if (Target.LocationType == LocationType.City)
            {
                IStructure structure = (IStructure)gameObjectLocator.Regions.GetObjectsInTile(X, Y).FirstOrDefault(z => z is IStructure);
                if (structure == null)
                {
                    procedure.TroopObjectDelete(troopObject, true);
                    stub.City.Owner.SendSystemMessage(null,
                                                      "Assignment Failed",
                                                      string.Format(
                                                                    @"Assigned target({0},{1}) has already been destroyed. The reserved troops have been returned to the city.",
                                                                    X,
                                                                    Y));
                    return false;
                }

                if (IsAttack)
                {
                    var troopObjectInitializer = troopObjectInitializerFactory.CreateAssignmentTroopObjectInitializer(troopObject, TroopBattleGroup.Attack, AttackMode);
                    action = actionFactory.CreateCityAttackChainAction(stub.City.Id,
                                                                       troopObjectInitializer,
                                                                       structure.City.Id,
                                                                       new Position(X, Y));
                }
                else
                {
                    var troopObjectInitializer = troopObjectInitializerFactory.CreateAssignmentTroopObjectInitializer(troopObject, TroopBattleGroup.Defense, AttackMode);

                    action = actionFactory.CreateCityDefenseChainAction(stub.City.Id,
                                                                        troopObjectInitializer,
                                                                        structure.City.Id);
                }
            }
            else if (Target.LocationType == LocationType.Stronghold)
            {
                if (IsAttack)
                {
                    var troopObjectInitializer = troopObjectInitializerFactory.CreateAssignmentTroopObjectInitializer(troopObject, TroopBattleGroup.Attack, AttackMode);

                    action = actionFactory.CreateStrongholdAttackChainAction(stub.City.Id,
                                                                             troopObjectInitializer,
                                                                             Target.LocationId,
                                                                             forceAttack: true);
                }
                else
                {
                    var troopObjectInitializer = troopObjectInitializerFactory.CreateAssignmentTroopObjectInitializer(troopObject, TroopBattleGroup.Defense, AttackMode);
                    action = actionFactory.CreateStrongholdDefenseChainAction(stub.City.Id,
                                                                              troopObjectInitializer,
                                                                              Target.LocationId);
                }
            }
            else
            {
                procedure.TroopObjectDelete(troopObject, true);
                return false;
            }

            if (stub.City.Worker.DoPassive(stub.City, action, true) != Error.Ok)
            {
                procedure.TroopObjectDelete(troopObject, true);
                return false;
            }

            return true;
        }

        internal void DbLoaderAdd(ITroopStub stub, bool dispatched)
        {
            assignmentTroops.Add(new AssignmentTroop(stub, DepartureTime(stub), dispatched));
            stub.OnRemoved += OnStubRemoved;
            stub.OnStateSwitched += StubOnStateSwitched;
        }

        #region ISchedule Members

        public bool IsScheduled { get; set; }

        public DateTime Time { get; private set; }

        public void Callback(object custom)
        {
            var now = SystemClock.Now;
            using (
                    locker.Lock(c => assignmentTroops.Select(troop => troop.Stub).ToArray<ILockable>(),
                                             new object[] {},
                                             Tribe))
            {
                lock (assignmentLock)
                {
                    var troopToDispatch = assignmentTroops.FirstOrDefault(x => !x.Dispatched && x.DepartureTime <= now);

                    if (troopToDispatch != null)
                    {
                        // Make sure troop time is still valid. If not we need to put it back on the scheduler.
                        // The time may change if the user has finished upgrading a tech/unit that improves speed since adding
                        // it to the assignment.
                        var departureTime = DepartureTime(troopToDispatch.Stub);
                        if (departureTime > now)
                        {
                            troopToDispatch.DepartureTime = departureTime;
                        }
                        // If a troop dispatches, then we set the troop to dispatched.
                        else if (Dispatch(troopToDispatch.Stub))
                        {
                            troopToDispatch.Dispatched = true;
                        }
                        // Otherwise, if dispatch fails, then we remove it.
                        else
                        {
                            RemoveStub(troopToDispatch.Stub);
                        }
                    }

                    Reschedule();
                }
            }
        }

        /// <summary>
        ///     Reschedules the assignment on the actual scheduler.
        ///     If there are no troops left, it will remove it.
        /// </summary>
        public void Reschedule()
        {
            // If this has been scheduled then remove
            if (IsScheduled)
            {
                scheduler.Remove(this);
            }

            // Take the quickest stub or the final target time.
            Time = assignmentTroops.Any(s => !s.Dispatched)
                           ? assignmentTroops.Where(s => !s.Dispatched).Min(x => x.DepartureTime)
                           : TargetTime;

            // If there are stubs that have not been dispatched or we haven't reached the time that the assignment should be over then we just reschedule it.
            if (assignmentTroops.Any(x => !x.Dispatched) || TargetTime.CompareTo(SystemClock.Now) > 0)
            {
                dbManager.Save(this);
                scheduler.Put(this);
            }
            else
            {
                // Remove all stubs from the assignment before removing it completely                
                var stubs = assignmentTroops.Select(x => x.Stub).ToList();

                foreach (var stub in stubs)
                {
                    RemoveStub(stub);
                }

                // Call assignment complete
                AssignmentComplete(this);

                // Delete the obj
                if (DbPersisted)
                {
                    dbManager.Delete(this);
                }
            }
        }

        #endregion

        #region IPersistableList Members

        public IEnumerable<DbColumn> DbListColumns
        {
            get
            {
                return new[]
                {
                        new DbColumn("city_id", DbType.UInt32), 
                        new DbColumn("stub_id", DbType.UInt16),
                        new DbColumn("dispatched", DbType.Byte)
                };
            }
        }

        #endregion

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
                return new[] {new DbColumn("id", Id, DbType.Int32)};
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
                        new DbColumn("tribe_id", Tribe.Id, DbType.UInt32),
                        new DbColumn("location_type", Target.LocationType.ToString(), DbType.String),
                        new DbColumn("location_id", Target.LocationId, DbType.UInt32),
                        new DbColumn("x", X, DbType.UInt32), new DbColumn("y", Y, DbType.UInt32),
                        new DbColumn("mode", Enum.GetName(typeof(AttackMode), AttackMode), DbType.String),
                        new DbColumn("attack_time", TargetTime, DbType.DateTime),
                        new DbColumn("dispatch_count", DispatchCount, DbType.UInt32),
                        new DbColumn("description", Description, DbType.String, 250),
                        new DbColumn("is_attack", IsAttack, DbType.Boolean)
                };
            }
        }

        #endregion
    }
}