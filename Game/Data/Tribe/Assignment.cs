using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using Game.Data.Troop;
using Game.Database;
using Game.Logic;
using Game.Logic.Actions;
using Game.Logic.Formulas;
using Game.Logic.Procedures;
using Game.Setup;
using Game.Util;

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

        public static readonly LargeIdGenerator IdGen = new LargeIdGenerator(long.MaxValue);
        public const string DB_TABLE = "Assignments";

        public int Id { get; set; }
        public Tribe Tribe { get; private set; }
        public DateTime TargetTime { get; private set; }
        public uint X { get; private set; }
        public uint Y { get; private set; }
        public AttackMode AttackMode { get; private set; }
        public uint DispatchCount { get; private set; }
        public int TroopCount { get { return stubs.Count; } }
        private readonly List<AssignmentTroop> stubs = new List<AssignmentTroop>();

        public delegate void OnComplete(Assignment assignment);
        public event OnComplete AssignmentComplete;
        public void InvokeAssignmentComplete() {
            OnComplete handler = AssignmentComplete;
            if (handler != null)
                handler(this);
        }

        public Assignment(int id, Tribe tribe, uint x, uint y, AttackMode mode, DateTime targetTime, uint dispatchCount) {
            Id = id;
            Tribe = tribe;
            TargetTime = targetTime;
            X = x;
            Y = y;
            AttackMode = mode;
            DispatchCount = dispatchCount;
        }

        public Assignment(Tribe tribe, uint x, uint y, AttackMode mode, DateTime targetTime, TroopStub stub) {
            Id = IdGen.GetNext();
            Tribe = tribe;
            TargetTime = targetTime;
            X = x;
            Y = y;
            AttackMode = mode;
            DispatchCount = 0;
            stubs.Add(new AssignmentTroop(stub, DepartureTime(stub)));
            Global.DbManager.Save(this);
            Global.Scheduler.Put(this);
        }

        public string ToNiceString() {
            string result = string.Format("Id[{0}] Time[{1}] x[{2}] y[{3}] mode[{4}] # of stubs[{5}] pispatched[{6}]\n", Id, TargetTime, X, Y, Enum.GetName(typeof(AttackMode), AttackMode), stubs.Count, DispatchCount);
            foreach (var obj in stubs) {
                TroopStub stub = obj.Stub;
                result += string.Format("\tTime[{0}] Player[{1}] City[{2}] Stub[{3}] Upkeep[{4}]\n",
                    obj.DepartureTime, stub.City.Owner.Name, stub.City.Name, stub.TroopId, stub.Upkeep);
            }
            return result;
        }

        public Error Add(TroopStub stub) {
            if (!stubs.Any()) return Error.AssignmentDone;

            stubs.Add(new AssignmentTroop(stub, DepartureTime(stub)));

            if (Global.Scheduler.Remove(this)) {
                Global.Scheduler.Put(this);
            }
            Global.DbManager.Save(this);
            return Error.Ok;

        }

        private DateTime DepartureTime(TroopStub stub) {
            int distance = SimpleGameObject.TileDistance(stub.City.X, stub.City.Y, X, Y);
            return TargetTime.Subtract(new TimeSpan(0, 0, (int)(Formula.MoveTime(Formula.GetTroopSpeed(stub)) * Formula.MoveTimeMod(stub.City, distance, true))));
        }

        private void Dispatch(TroopStub stub) {
            Structure structure = (Structure)Global.World.GetObjects(X, Y).Find(z => z is Structure);
            if (structure == null) {
                throw new Exception("nothing to attack, please add code to handle!");
            }
            // Create troop object
            if (!Procedure.TroopObjectCreate(stub.City, stub)) {
                throw new Exception("fail to create troop object?!?");
            }
            var aa = new AttackChainAction(stub.City.Id, stub.TroopId, structure.City.Id, structure.ObjectId, AttackMode);
            stub.City.Worker.DoPassive(stub.City, aa, true);
        }


        #region ISchedule Members

        public bool IsScheduled { get; set; }

        public DateTime Time {
            get {
                return stubs.Any() ? stubs.Min(x => x.DepartureTime) : TargetTime;
            }
        }

        public void Callback(object custom)
        {
            var now = DateTime.UtcNow;
            using (new CallbackLock(c => stubs.Select(troop => troop.Stub).ToArray(), new object[] { }, Tribe)) {
                foreach (var assignmentTroop in stubs) {
                    if (assignmentTroop.Dispatched || assignmentTroop.DepartureTime > now)
                    {
                        continue;
                    }

                    Dispatch(assignmentTroop.Stub);
                    assignmentTroop.Dispatched = true;                   
                }

                Global.DbManager.Save(this);

                if (stubs.Any(x => !x.Dispatched) || TargetTime.CompareTo(DateTime.UtcNow) > 0) {
                    Global.Scheduler.Put(this);
                } else {
                    InvokeAssignmentComplete();
                    IdGen.Release(Id);
                    Global.DbManager.Delete(this);
                }
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

        }
    }
}
