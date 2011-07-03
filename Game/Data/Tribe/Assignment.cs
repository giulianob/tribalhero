using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using Game.Data.Troop;
using Game.Database;
using Game.Logic;
using Game.Logic.Actions;
using Game.Logic.Formulas;
using Game.Logic.Procedures;
using Game.Setup;
using Game.Util;

namespace Game.Data.Tribe {

    public class Assignment : ISchedule, IPersistableList
    {
        public static readonly LargeIdGenerator IdGen = new LargeIdGenerator(long.MaxValue);
        public const string DB_TABLE = "Assignments";
        DateTime time;
        uint x, y;
        AttackMode attackMode;
        uint dispatchCount = 0;

        Dictionary<DateTime, TroopStub> stubs= new Dictionary<DateTime, TroopStub>();

        public int Id { get; set; }
        public Tribe Tribe { get; private set; }

        public delegate void OnComplete(Assignment assignment);
        public event OnComplete AssignmentComplete;
        public void InvokeAssignmentComplete()
        {
            OnComplete handler = AssignmentComplete;
            if (handler != null)
                handler(this);
        }

        public Assignment(int id, Tribe tribe, uint x, uint y, AttackMode mode, DateTime time, uint dispatchCount)
        {
            Id = id;
            Tribe = tribe;
            this.time = time;
            this.x = x;
            this.y = y;
            attackMode = mode;
            this.dispatchCount = dispatchCount;
        }

        public Assignment(Tribe tribe, uint x, uint y, AttackMode mode, DateTime time, TroopStub stub)
        {
            Id = IdGen.GetNext();
            Tribe = tribe;
            this.time = time;
            this.x = x;
            this.y = y;
            attackMode = mode;
            stubs.Add(DepartureTime(stub), stub);
            Global.DbManager.Save(this);
            Global.Scheduler.Put(this);
        }

        public string ToNiceString()
        {
            string result = string.Format("Id[{0}] Time[{1}] x[{2}] y[{3}] mode[{4}] # of stubs[{5}] pispatched[{6}]\n",Id, time, x, y, Enum.GetName(typeof(AttackMode), attackMode), stubs.Count,dispatchCount);
            foreach(var kvp in stubs)
            {
                TroopStub stub =kvp.Value;
                result += string.Format("\tTime[{0}] Player[{1}] City[{2}] Stub[{3}] Upkeep[{4}]\n",
                    kvp.Key, stub.City.Owner.Name, stub.City.Name, stub.TroopId, stub.Upkeep);
            }
            return result;
        }

        public Error Add(TroopStub stub)
        {
            lock (this)
            {
                if (!stubs.Any()) return Error.AssignmentDone;
                DateTime t = DepartureTime(stub);
                while(stubs.ContainsKey(t))
                {
                    t = t.AddMilliseconds(1);
                }
                stubs.Add(t, stub);
                if (Global.Scheduler.Remove(this))
                {
                    Global.Scheduler.Put(this);
                }
                Global.DbManager.Save(this);
                return Error.Ok;
            }
        }

        private DateTime DepartureTime(TroopStub stub)
        {
            int distance = SimpleGameObject.TileDistance(stub.City.X,stub.City.Y,x,y);
            return time.Subtract(new TimeSpan(0, 0, (int)(Formula.MoveTime(Formula.GetTroopSpeed(stub)) * Formula.MoveTimeMod(stub.City, distance, true))));
        }

        private void Dispatch(TroopStub stub)
        {
            Structure structure = (Structure)Global.World.GetObjects(x, y).Find(z => z is Structure);
            if(structure==null)
            {
                throw  new Exception("nothing to attack, please add code to handle!");
            }
            // Create troop object
            if (!Procedure.TroopObjectCreate(stub.City, stub))
            {
                throw new Exception("fail to create troop object?!?");
            }
            var aa = new AttackChainAction(stub.City.Id, stub.TroopId, structure.City.Id, structure.ObjectId, attackMode);
            stub.City.Worker.DoPassive(stub.City, aa, true);
        }


        #region ISchedule Members

        public bool IsScheduled { get; set; }

        public DateTime Time {
            get
            {
                return stubs.Any()?stubs.Keys.Min():time;
            }
        }

        public void Callback(object custom) {
            using (new CallbackLock((c)=> ((Dictionary<DateTime, TroopStub>)c[0]).Values.ToArray(),new object[]{stubs},Tribe)) {
                lock (this) {
                    // lock 
                    foreach (var kvp in new List<KeyValuePair<DateTime, TroopStub>>(stubs.Where(z => z.Key <= DateTime.UtcNow))) {
                        Dispatch(kvp.Value);
                        stubs.Remove(kvp.Key);
                        ++dispatchCount;
                    }
                    Global.DbManager.Save(this);
                    if (stubs.Any() || time.CompareTo(DateTime.UtcNow) > 0) {
                        Global.Scheduler.Put(this);
                    } else {
                        InvokeAssignmentComplete();
                        IdGen.Release(Id);
                        Global.DbManager.Delete(this);
                    }
                }
            }
        }

        #endregion

        #region IPersistableList Members

        public DbColumn[] DbListColumns {
            get
            {
                return new[] { new DbColumn("city_id", DbType.UInt32), new DbColumn("stub_id", DbType.Byte) };
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
                              new DbColumn("x", x, DbType.UInt32),
                              new DbColumn("y", y, DbType.UInt32),
                              new DbColumn("mode",Enum.GetName(typeof(AttackMode),attackMode),DbType.String),
                              new DbColumn("attack_time",time,DbType.DateTime), 
                              new DbColumn("dispatch_count", dispatchCount, DbType.UInt32),
                       };
            }
        }
        
        #endregion

        #region IEnumerable<DbColumn[]> Members

        public IEnumerator<DbColumn[]> GetEnumerator() {
            Dictionary<DateTime, TroopStub>.Enumerator itr = stubs.GetEnumerator();
            while (itr.MoveNext())
            {
                yield return
                    new[]
                    {
                        new DbColumn("city_id", itr.Current.Value.City.Id,DbType.UInt32),
                        new DbColumn("stub_id", itr.Current.Value.TroopId,DbType.Byte),
                    };
            }
        }

        #endregion

        #region IEnumerable Members

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() {
            return stubs.GetEnumerator();
        }

        #endregion

        internal void DbLoaderAdd(TroopStub stub) {
            lock (this) {
                DateTime t = DepartureTime(stub);
                while (stubs.ContainsKey(t)) {
                    t = t.AddMilliseconds(1);
                }
                stubs.Add(t, stub);
            }
        }
    }
}
