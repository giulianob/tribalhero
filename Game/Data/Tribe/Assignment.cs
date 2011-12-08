using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using Game.Data.Troop;
using Game.Logic;
using Game.Logic.Actions;
using Game.Logic.Formulas;
using Game.Logic.Procedures;
using Game.Setup;
using Game.Util;
using Game.Util.Locking;
using Ninject;
using Persistance;

namespace Game.Data.Tribe {

    public class Assignment : ISchedule, IPersistableList, IEnumerable<Assignment.AssignmentTroop>
    {
        public class AssignmentTroop
        {
            public TroopStub Stub { get; set; }
            public DateTime DepartureTime { get; set; }
            public bool Dispatched { get; set; }

            public AssignmentTroop(TroopStub stub, DateTime departureTime, bool dispatched = false)
            {
                Stub = stub;
                DepartureTime = departureTime;
                Dispatched = dispatched;
            }
        }

        private readonly object lck = new object();

        public static readonly LargeIdGenerator IdGen = new LargeIdGenerator(long.MaxValue);
        public const string DB_TABLE = "Assignments";

        public int Id { get; set; }
        public Tribe Tribe { get; private set; }
        public City TargetCity { get; private set; }
        public DateTime TargetTime { get; private set; }
        public uint X { get; private set; }
        public uint Y { get; private set; }
        public AttackMode AttackMode { get; private set; }
        public uint DispatchCount { get; private set; }
        public int TroopCount { get { return stubs.Count; } }
        private readonly List<AssignmentTroop> stubs = new List<AssignmentTroop>();

        public delegate void OnComplete(Assignment assignment);
        public event OnComplete AssignmentComplete = delegate { }; 

        public Assignment(int id, Tribe tribe, uint x, uint y, City targetCity, AttackMode mode, DateTime targetTime, uint dispatchCount) {
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

        public Assignment(Tribe tribe, uint x, uint y, City targetCity, AttackMode mode, DateTime targetTime, TroopStub stub) {
            Id = IdGen.GetNext();
            Tribe = tribe;
            TargetTime = targetTime;
            TargetCity = targetCity;
            X = x;
            Y = y;
            AttackMode = mode;
            DispatchCount = 0;
            stubs.Add(new AssignmentTroop(stub, DepartureTime(stub)));
            Ioc.Kernel.Get<IDbManager>().Save(this);
            Global.Scheduler.Put(this);
        }

        public string ToNiceString() {
            lock (lck)
            {
                string result = string.Format("Id[{0}] Time[{1}] x[{2}] y[{3}] mode[{4}] # of stubs[{5}] pispatched[{6}]\n",
                                              Id,
                                              TargetTime,
                                              X,
                                              Y,
                                              Enum.GetName(typeof(AttackMode), AttackMode),
                                              stubs.Count,
                                              DispatchCount);
                foreach (var obj in stubs)
                {
                    TroopStub stub = obj.Stub;
                    result += string.Format("\tTime[{0}] Player[{1}] City[{2}] Stub[{3}] Upkeep[{4}]\n",
                                            obj.DepartureTime,
                                            stub.City.Owner.Name,
                                            stub.City.Name,
                                            stub.TroopId,
                                            stub.Upkeep);
                }

                return result;
            }            
        }

        public Error Add(TroopStub stub) {
            lock (lck)
            {
                if (stubs.All(s => s.Dispatched))
                    return Error.AssignmentDone;

                stubs.Add(new AssignmentTroop(stub, DepartureTime(stub)));
                stub.OnRemoved += RemoveStub;
                stub.OnStateSwitched += StubOnStateSwitched;

                if (Global.Scheduler.Remove(this))
                {
                    Global.Scheduler.Put(this);
                }

                Ioc.Kernel.Get<IDbManager>().Save(this);
            }

            return Error.Ok;
        }

        private void StubOnStateSwitched(TroopStub stub, TroopState newState)
        {
            if (newState == TroopState.Battle)
            {
                RemoveStub(stub);
            }
        }

        private void RemoveStub(TroopStub stub)
        {
            lock (lck)
            {
                foreach (var assignmentTroop in stubs.Where(assignmentTroop => assignmentTroop.Stub == stub))
                {
                    stub.OnRemoved -= RemoveStub;
                    stub.OnStateSwitched -= StubOnStateSwitched;
                    stubs.Remove(assignmentTroop);

                    Reschedule();
                    break;
                }
            }
        }

        private DateTime DepartureTime(TroopStub stub) {
            int distance = SimpleGameObject.TileDistance(stub.City.X, stub.City.Y, X, Y);
            return TargetTime.Subtract(new TimeSpan(0, 0, Formula.MoveTimeTotal(stub.Speed, distance, true, new List<Effect>(stub.City.Technologies.GetAllEffects()))));
        }

        private bool Dispatch(TroopStub stub) {
            Structure structure = (Structure)Global.World.GetObjects(X, Y).Find(z => z is Structure);
            if (structure == null) {
                Procedure.TroopStubDelete(stub.City, stub);
                stub.City.Owner.SendSystemMessage(null, "Assignment Failed", string.Format(@"Assigned target({0},{1}) has already been destroyed. The reserved troops have been returned to the city.", X, Y));
                return false;
            }

            // Create troop object
            Procedure.TroopObjectCreate(stub.City, stub);

            var action = new AttackChainAction(stub.City.Id, stub.TroopId, structure.City.Id, structure.ObjectId, AttackMode);
            if (stub.City.Worker.DoPassive(stub.City, action, true) != Error.Ok)
            {
                Procedure.TroopObjectDelete(stub.TroopObject, true);
                return false;
            }

            return true;
        }

        #region ISchedule Members

        public bool IsScheduled { get; set; }

        public DateTime Time { get; private set; }

        public void Callback(object custom)
        {
            var now = DateTime.UtcNow;
            using (Ioc.Kernel.Get<CallbackLock>().Lock(c => stubs.Select(troop => troop.Stub).ToArray(), new object[] { }, Tribe)) {
                lock (lck)
                {
                    for (int i = stubs.Count - 1; i >= 0; i--)
                    {
                        var assignmentTroop = stubs[i];
                        if (assignmentTroop.Dispatched || assignmentTroop.DepartureTime > now)
                            continue;

                        if (Dispatch(assignmentTroop.Stub))
                            assignmentTroop.Dispatched = true;
                        else
                            stubs.RemoveAt(i);
                    }
                    
                    Reschedule();
                }
            }
        }

        /// <summary>
        /// Recalculates the action time. This doesn't reschedule the action.
        /// </summary>
        public void ResetNextTime()
        {
            Time = stubs.Any(s => !s.Dispatched) ? stubs.Where(s => !s.Dispatched).Min(x => x.DepartureTime) : TargetTime;
        }

        /// <summary>
        /// Reschedules the assignment on the actual scheduler.
        /// If there are no troops left, it will remove it.
        /// </summary>
        private void Reschedule()
        {
            if (IsScheduled)
                Global.Scheduler.Remove(this);

            ResetNextTime();

            if (stubs.Count > 0 && (stubs.Any(x => !x.Dispatched) || TargetTime.CompareTo(DateTime.UtcNow) > 0))
            {
                Ioc.Kernel.Get<IDbManager>().Save(this);
                Global.Scheduler.Put(this);
            }
            else
            {                
                AssignmentComplete(this);
                IdGen.Release(Id);
                Ioc.Kernel.Get<IDbManager>().Delete(this);
            }
        }

        #endregion

        #region IPersistableList Members

        public DbColumn[] DbListColumns {
            get {
                return new[] { new DbColumn("city_id", DbType.UInt32), new DbColumn("stub_id", DbType.Byte), new DbColumn("dispatched", DbType.Byte) };
            }
        }

        #endregion

        #region IPersistableObject Members

        public bool DbPersisted { get; set; }

        #endregion

        #region IPersistable Members

        public string DbTable {
            get { return DB_TABLE; }
        }

        public DbColumn[] DbPrimaryKey {
            get {
                return new[] { new DbColumn("id", Id, DbType.Int32) };
            }
        }

        public DbDependency[] DbDependencies {
            get {
                return new DbDependency[] { };
            }
        }

        public DbColumn[] DbColumns {
            get {
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

        #region IEnumerable<DbColumn[]> Members

        public IEnumerator<DbColumn[]> GetEnumerator() {
            var itr = stubs.GetEnumerator();
            while (itr.MoveNext()) {
                yield return
                    new[]
                    {
                        new DbColumn("city_id", itr.Current.Stub.City.Id, DbType.UInt32),
                        new DbColumn("stub_id", itr.Current.Stub.TroopId, DbType.Byte),
                        new DbColumn("dispatched", itr.Current.Dispatched ? (byte)1 : (byte)0, DbType.Byte)
                    };
            }
        }

        #endregion

        #region IEnumerable Members

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() {
            return stubs.GetEnumerator();
        }

        IEnumerator<AssignmentTroop> IEnumerable<AssignmentTroop>.GetEnumerator()
        {
            return stubs.GetEnumerator();
        }

        #endregion

        internal void DbLoaderAdd(TroopStub stub, bool dispatched) {
            stubs.Add(new AssignmentTroop(stub, DepartureTime(stub), dispatched));
            stub.OnRemoved += RemoveStub;
            stub.OnStateSwitched += StubOnStateSwitched;            
        }
    }
}
