﻿using System;
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

        private readonly Formula formula;
        private readonly IDbManager dbManager;
        private readonly IGameObjectLocator gameObjectLocator;
        private readonly IScheduler scheduler;
        private readonly Procedure procedure;
        private readonly TileLocator tileLocator;

        private readonly IActionFactory actionFactory;

        /// <summary>
        /// List of stubs in this assignment
        /// </summary>
        private readonly List<AssignmentTroop> assignmentTroops = new List<AssignmentTroop>();

        /// <summary>
        /// Lock for the assignment when adding/removing stubs
        /// </summary>
        private readonly object assignmentLock = new object();

        /// <summary>
        /// Assignment id generator
        /// </summary>
        public static readonly LargeIdGenerator IdGen = new LargeIdGenerator(long.MaxValue);

        /// <summary>
        /// Table name where Assignment gets persisted to
        /// </summary>
        public const string DB_TABLE = "Assignments";

        /// <summary>
        /// Id of the assignment
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Tribe assignment belongs to
        /// </summary>
        public Tribe Tribe { get; private set; }

        /// <summary>
        /// City this assignment is targetting
        /// </summary>
        public ICity TargetCity { get; private set; }

        /// <summary>
        /// Time that the assignment will end (time it should reach its target)
        /// </summary>
        public DateTime TargetTime { get; private set; }

        /// <summary>
        /// X coordinate to attack
        /// </summary>
        public uint X { get; private set; }

        /// <summary>
        /// Y coordinate to attack
        /// </summary>
        public uint Y { get; private set; }

        /// <summary>
        /// Attack strength of assignment
        /// </summary>
        public AttackMode AttackMode { get; private set; }

        /// <summary>
        /// Number of stubs that have already been dispatched
        /// </summary>
        public uint DispatchCount { get; private set; }

        /// <summary>
        /// Number of troops in this assignment (some might have been dispatched already)
        /// </summary>
        public int TroopCount { get { return assignmentTroops.Count; } }

        /// <summary>
        /// Event fired when assignment completes (all stubs should be gone by this time)
        /// </summary>
        public event OnComplete AssignmentComplete = delegate { };

        /// <summary>
        /// Creates a new assignment. 
        /// NOTE: This constructor is used by the db loader. Use the other constructor when creating a new assignment from scratch.
        /// </summary>
        /// <param name="id"></param>
        /// <param name="tribe"></param>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="targetCity"></param>
        /// <param name="mode"></param>
        /// <param name="targetTime"></param>
        /// <param name="dispatchCount"></param>
        /// <param name="formula"></param>
        /// <param name="dbManager"> </param>
        /// <param name="gameObjectLocator"> </param>
        /// <param name="scheduler"> </param>
        /// <param name="procedure"> </param>
        /// <param name="tileLocator"> </param>
        /// <param name="actionFactory"> </param>
        public Assignment(int id, Tribe tribe, uint x, uint y, ICity targetCity, AttackMode mode, DateTime targetTime, uint dispatchCount, Formula formula, IDbManager dbManager, IGameObjectLocator gameObjectLocator, IScheduler scheduler, Procedure procedure, TileLocator tileLocator, IActionFactory actionFactory)
        {
            this.formula = formula;
            this.dbManager = dbManager;
            this.gameObjectLocator = gameObjectLocator;
            this.scheduler = scheduler;
            this.procedure = procedure;
            this.tileLocator = tileLocator;
            this.actionFactory = actionFactory;

            Id = id;
            Tribe = tribe;
            TargetTime = targetTime;
            TargetCity = targetCity;
            X = x;
            Y = y;
            AttackMode = mode;
            DispatchCount = dispatchCount;

            IdGen.Set(id);
        }

        /// <summary>
        /// Creates a new assignment.
        /// An id will be assigned and the stub passed in will be added to the assignment. This will not schedule the assignment!
        /// </summary>
        /// <param name="tribe"></param>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="targetCity"></param>
        /// <param name="mode"></param>
        /// <param name="targetTime"></param>
        /// <param name="stub"></param>
        /// <param name="formula"> </param>
        /// <param name="dbManager"> </param>
        /// <param name="gameObjectLocator"> </param>
        /// <param name="scheduler"> </param>
        /// <param name="procedure"> </param>
        /// <param name="tileLocator"> </param>
        /// <param name="actionFactory"> </param>
        public Assignment(Tribe tribe, uint x, uint y, ICity targetCity, AttackMode mode, DateTime targetTime, ITroopStub stub, Formula formula, IDbManager dbManager, IGameObjectLocator gameObjectLocator, IScheduler scheduler, Procedure procedure, TileLocator tileLocator, IActionFactory actionFactory)
        {
            this.formula = formula;
            this.dbManager = dbManager;
            this.gameObjectLocator = gameObjectLocator;
            this.scheduler = scheduler;
            this.procedure = procedure;
            this.tileLocator = tileLocator;
            this.actionFactory = actionFactory;

            Id = IdGen.GetNext();
            Tribe = tribe;
            TargetTime = targetTime;
            TargetCity = targetCity;
            X = x;
            Y = y;
            AttackMode = mode;
            DispatchCount = 0;

            assignmentTroops.Add(new AssignmentTroop(stub, DepartureTime(stub)));
        }

        public override string ToString()
        {
            StringBuilder stringBuilder = new StringBuilder();

            lock (assignmentLock)
            {
                stringBuilder.Append(string.Format("Id[{0}] Time[{1}] x[{2}] y[{3}] mode[{4}] # of stubs[{5}] pispatched[{6}]\n",
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
        /// Add a stub and reschedule's the assignment
        /// </summary>
        /// <param name="stub"></param>
        /// <returns></returns>
        public Error Add(TroopStub stub)
        {
            lock (assignmentLock)
            {
                // Don't allow ppl to join in the last minute
                if (SystemClock.Now.AddMinutes(1) >= TargetTime)
                    return Error.AssignmentDone;

                assignmentTroops.Add(new AssignmentTroop(stub, DepartureTime(stub)));
                stub.OnRemoved += RemoveStub;
                stub.OnStateSwitched += StubOnStateSwitched;

                Reschedule();
            }

            return Error.Ok;
        }

        /// <summary>
        /// Handler when an event switches state. This is how we know to remove a stub from the assignment once it joins the battle.
        /// </summary>
        /// <param name="stub"></param>
        /// <param name="newState"></param>
        private void StubOnStateSwitched(ITroopStub stub, TroopState newState)
        {
            lock (assignmentLock)
            {
                if (newState == TroopState.Battle)
                {
                    RemoveStub(stub);
                    Reschedule();
                }
            }
        }

        /// <summary>
        /// Removes a stub from the assignment
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

                stub.OnRemoved -= RemoveStub;
                stub.OnStateSwitched -= StubOnStateSwitched;
                assignmentTroops.Remove(assignmentTroop);
            }
        }

        /// <summary>
        /// Calculates the departure time for a given troop stub
        /// </summary>
        /// <param name="stub"></param>
        /// <returns></returns>
        private DateTime DepartureTime(ITroopStub stub)
        {
            int distance = tileLocator.TileDistance(stub.City.X, stub.City.Y, X, Y);
            return TargetTime.Subtract(new TimeSpan(0, 0, formula.MoveTimeTotal(stub.Speed, distance, true, new List<Effect>(stub.City.Technologies.GetAllEffects()))));
        }

        /// <summary>
        /// Called to dispatch a unit
        /// </summary>
        /// <param name="stub"></param>
        /// <returns></returns>
        private bool Dispatch(ITroopStub stub)
        {
            Structure structure = (Structure)gameObjectLocator.GetObjects(X, Y).Find(z => z is Structure);
            if (structure == null)
            {
                procedure.TroopStubDelete(stub.City, stub);
                stub.City.Owner.SendSystemMessage(null, "Assignment Failed", string.Format(@"Assigned target({0},{1}) has already been destroyed. The reserved troops have been returned to the city.", X, Y));
                return false;
            }

            // Create troop object
            procedure.TroopObjectCreate(stub.City, stub);

            var action = actionFactory.CreateAttackChainAction(stub.City.Id, stub.TroopId, structure.City.Id, structure.ObjectId, AttackMode);
            if (stub.City.Worker.DoPassive(stub.City, action, true) != Error.Ok)
            {
                procedure.TroopObjectDelete(stub.TroopObject, true);
                return false;
            }

            return true;
        }

        #region ISchedule Members

        public bool IsScheduled { get; set; }

        public DateTime Time { get; private set; }

        public void Callback(object custom)
        {
            var now = SystemClock.Now;
            using (Concurrency.Current.Lock(c => assignmentTroops.Select(troop => troop.Stub).ToArray(), new object[] { }, Tribe))
            {
                lock (assignmentLock)
                {
                    var troopToDispatch = assignmentTroops.FirstOrDefault(x => !x.Dispatched && x.DepartureTime <= now);

                    if (troopToDispatch != null)
                    {
                        // If a troop dispatches, then we set the troop to dispatched.
                        if (Dispatch(troopToDispatch.Stub))
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
        /// Reschedules the assignment on the actual scheduler.
        /// If there are no troops left, it will remove it.
        /// </summary>
        public void Reschedule()
        {
            // If this has been scheduled then remove
            if (IsScheduled)
                scheduler.Remove(this);

            // Take the quickest stub or the final target time.
            Time = assignmentTroops.Any(s => !s.Dispatched) ? assignmentTroops.Where(s => !s.Dispatched).Min(x => x.DepartureTime) : TargetTime;

            // If there are stubs that have not been dispatched or we haven't reached the time that the assignment should be over then we just reschedule it.
            if (assignmentTroops.Count > 0 && (assignmentTroops.Any(x => !x.Dispatched) || TargetTime.CompareTo(SystemClock.Now) > 0))
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

        public DbColumn[] DbListColumns
        {
            get
            {
                return new[] { new DbColumn("city_id", DbType.UInt32), new DbColumn("stub_id", DbType.Byte), new DbColumn("dispatched", DbType.Byte) };
            }
        }

        #endregion

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
                return new[] { new DbColumn("id", Id, DbType.Int32) };
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
                              new DbColumn("tribe_id", Tribe.Id, DbType.UInt32),
                              new DbColumn("city_id", TargetCity.Id, DbType.UInt32),
                              new DbColumn("x", X, DbType.UInt32),
                              new DbColumn("y", Y, DbType.UInt32),
                              new DbColumn("mode",Enum.GetName(typeof(AttackMode),AttackMode),DbType.String),
                              new DbColumn("attack_time",TargetTime,DbType.DateTime), 
                              new DbColumn("dispatch_count", DispatchCount, DbType.UInt32),
                       };
            }
        }

        #endregion

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
                        new DbColumn("stub_id", itr.Current.Stub.TroopId, DbType.Byte),
                        new DbColumn("dispatched", itr.Current.Dispatched ? (byte)1 : (byte)0, DbType.Byte)
                    };
            }
        }

        #region IEnumerable Members

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return assignmentTroops.GetEnumerator();
        }

        IEnumerator<AssignmentTroop> IEnumerable<AssignmentTroop>.GetEnumerator()
        {
            return assignmentTroops.GetEnumerator();
        }

        #endregion

        internal void DbLoaderAdd(ITroopStub stub, bool dispatched)
        {
            assignmentTroops.Add(new AssignmentTroop(stub, DepartureTime(stub), dispatched));
            stub.OnRemoved += RemoveStub;
            stub.OnStateSwitched += StubOnStateSwitched;
        }
    }
}
