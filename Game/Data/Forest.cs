#region

using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using Game.Data.Stats;
using Game.Data.Troop;
using Game.Database;
using Game.Logic;
using Game.Logic.Actions.ResourceActions;
using Game.Setup;
using Game.Util;

#endregion

namespace Game.Data {
    public class Forest : SimpleGameObject, IPersistableObject, IEnumerable<Structure> {

        public ForestDepleteAction DepleteAction { get; set; }

        public override uint ObjectId {
            get { return objectId; }
            set {
                CheckUpdateMode();
                objectId = value;
            }
        }

        public override ushort Type {
            get { return (ushort) Types.FOREST; }
        }

        readonly byte lvl = 1;
        public override byte Lvl {
            get {
                return lvl;
            }
        }

        /// <summary>
        /// The structures currently getting wood from this forest
        /// </summary>
        List<Structure> structures = new List<Structure>();       

        /// <summary>
        /// Maximum laborers allowed in this forest
        /// </summary>
        public ushort MaxLabor {
            get {
                return Formula.GetForestMaxLabor(lvl);
            }
        }

        /// <summary>
        /// Current amount of laborers in this forest
        /// </summary>
        public int Labor { get; set; }

        /// <summary>
        /// The lumber availabel at this forest. 
        /// Notice: The rate is not used, only the upkeep. The rate of the forest is kept in a separate variable.
        /// </summary>
        public AggressiveLazyValue Wood { get; set; }

        /// <summary>
        /// Base rate at which this forest gives out resources.
        /// </summary>
        public int Rate { get; private set; }

        /// <summary>
        /// Time until forest is depleted
        /// </summary>
        public DateTime DepleteTime { get; private set; }

        #region Constructors

        public Forest(byte lvl, int capacity, int rate) {
            this.lvl = lvl;

            Wood = new AggressiveLazyValue(capacity) {
                Limit = capacity
            };

            Rate = rate;
        }

        #endregion

        #region Methods
        public void AddLumberjack(Structure structure) {
            CheckUpdateMode();
            structures.Add(structure);
        }

        public void RemoveLumberjack(Structure structure) {
            CheckUpdateMode();
            structures.Remove(structure);
        }

        /// <summary>
        /// Recalculates the rate of the forest and all lumberjack structures around it.
        /// Must lock all players using the forest and the forest manager.
        /// </summary>
        public void RecalculateForest() {
            CheckUpdateMode();

            // Get the number of labors assigned
            int totalLabor = this.Aggregate(0, (current, obj) => current + obj.Stats.Labor);

            // Calculate efficiency           
            double playerEfficiency = structures.Count / 8d;
            double laborEfficiency = (double)totalLabor / MaxLabor;
            double efficiency = (1 - Math.Abs(playerEfficiency - laborEfficiency)) * (structures.Count * 0.125);

            int totalRate = 0;
            // Set the appropriate rates
            foreach (Structure obj in this) {

                // Skip structures that are still being built
                if (obj.Lvl == 0) continue;

                // Get the current rate. This will be figure out how much we need to adjust the rate.
                int oldRate = (int)obj["Rate"];

                int newRate = (int)((double)obj.Stats.Labor * Rate * (1d + efficiency));

                if (newRate != oldRate) {
                    // Save the rate in the obj. This is needed so later we can look up how much this object is actually giving.
                    obj.BeginUpdate();
                    obj["Rate"] = newRate;
                    obj.EndUpdate();

                    // Update the cities rate
                    obj.City.BeginUpdate();
                    obj.City.Resource.Wood.Rate += (newRate - oldRate);
                    obj.City.EndUpdate();
                }

                totalRate += newRate;
            }

            // Set the forests upkeep
            Wood.Upkeep = totalRate;

            // Set the forests total labor
            Labor = totalLabor;

            SetDepleteAction();
        }

        /// <summary>
        /// Updates the deplete action to fire when appropriately.
        /// </summary>
        public void SetDepleteAction() {            
            if (DepleteAction != null)
                Global.Scheduler.Del(DepleteAction);

            DateTime depleteTime = DateTime.Now.AddDays(2).AddMinutes(Config.Random.Next(360));
            if (Wood.Upkeep != 0)
                depleteTime = DateTime.Now.AddHours(Wood.Value / (Wood.Upkeep / Config.seconds_per_unit));

            DepleteAction = new ForestDepleteAction(this, depleteTime);

            Global.Scheduler.Put(DepleteAction);

            DepleteTime = depleteTime;
        }
        #endregion

        #region Updates

        public override void CheckUpdateMode() {
            if (!Global.FireEvents || objectId == 0)
                return;

            if (!updating)
                throw new Exception("Changed state outside of begin/end update block");

            MultiObjectLock.ThrowExceptionIfNotLocked(Global.Forests);
        }

        public override void EndUpdate() {
            if (!updating)
                throw new Exception("Called an endupdate without first calling a beginupdate");

            updating = false;

            Update();
        }

        protected new void Update() {
            base.Update();

            if (!Global.FireEvents)
                return;

            if (updating)
                return;

            if (objectId > 0)
                Global.DbManager.Save(this);
        }

        #endregion

        #region IPersistable Members

        public const string DB_TABLE = "forests";

        public string DbTable {
            get { return DB_TABLE; }
        }

        public DbColumn[] DbColumns {
            get {                                
                return new[] {
                                new DbColumn("labor", Labor, DbType.UInt16),
                                new DbColumn("x", X, DbType.UInt32), 
                                new DbColumn("y", Y, DbType.Int32),            
                                new DbColumn("level", Lvl, DbType.Byte),    
                                new DbColumn("rate", Rate, DbType.UInt32), 
                                new DbColumn("capacity", Wood.Limit, DbType.Int32), 
                                new DbColumn("last_realize_time", Wood.LastRealizeTime, DbType.DateTime),
                                new DbColumn("lumber", Wood.RawValue, DbType.Int32),
                                new DbColumn("upkeep", Wood.Upkeep, DbType.Int32),                                
                                new DbColumn("state", (byte) State.Type, DbType.Boolean),
                                new DbColumn("state_parameters", XMLSerializer.SerializeList(State.Parameters.ToArray()), DbType.String),
                                new DbColumn("structures", XMLSerializer.SerializeComplexList(structures.Select(structure => new object[] {structure.City.Id, structure.ObjectId})), DbType.String)
                };
            }
        }

        public DbColumn[] DbPrimaryKey {
            get { return new[] {new DbColumn("id", ObjectId, DbType.UInt32)}; }
        }

        public DbDependency[] DbDependencies {
            get { return new DbDependency[] {}; }
        }

        public bool DbPersisted { get; set; }

        #endregion

        public IEnumerator<Structure> GetEnumerator() {
            return structures.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator() {
            return GetEnumerator();
        }
    }
}