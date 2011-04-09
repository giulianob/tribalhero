#region

using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using Game.Database;
using Game.Logic.Actions;
using Game.Logic.Actions.ResourceActions;
using Game.Logic.Formulas;
using Game.Map;
using Game.Setup;
using Game.Util;

#endregion

namespace Game.Data
{
    public class Forest : SimpleGameObject, IPersistableObject, IEnumerable<Structure>, ICityRegionObject
    {
        public const string DB_TABLE = "forests";
        private readonly byte lvl = 1;

        /// <summary>
        ///   The structures currently getting wood from this forest
        /// </summary>
        private readonly List<Structure> structures = new List<Structure>();

        public ForestDepleteAction DepleteAction { get; set; }

        public override uint ObjectId
        {
            get
            {
                return objectId;
            }
            set
            {
                CheckUpdateMode();
                objectId = value;
            }
        }

        public override ushort Type
        {
            get
            {
                return (ushort)Types.Forest;
            }
        }

        public override byte Lvl
        {
            get
            {
                return lvl;
            }
        }

        /// <summary>
        ///   Maximum laborers allowed in this forest
        /// </summary>
        public ushort MaxLabor
        {
            get
            {
                return Formula.GetForestMaxLabor(lvl);
            }
        }

        /// <summary>
        ///   Current amount of laborers in this forest
        /// </summary>
        public int Labor { get; set; }

        /// <summary>
        ///   The lumber availabel at this forest. 
        ///   Notice: The rate is not used, only the upkeep. The rate of the forest is kept in a separate variable.
        /// </summary>
        public AggressiveLazyValue Wood { get; set; }

        /// <summary>
        ///   Base rate at which this forest gives out resources.
        /// </summary>
        public double Rate { get; private set; }

        /// <summary>
        ///   Time until forest is depleted
        ///   Only the db loader should be setting this.
        /// </summary>
        public DateTime DepleteTime { get; set; }

        #region Constructors

        public Forest(byte lvl, int capacity, double rate)
        {
            this.lvl = lvl;

            Wood = new AggressiveLazyValue(capacity) {Limit = capacity};

            Rate = rate;
        }

        #endregion

        #region Methods

        public void AddLumberjack(Structure structure)
        {
            CheckUpdateMode();
            structures.Add(structure);

            // Store the forest_id in the structure so we have a way of getting the forest from the structure
            structure.BeginUpdate();
            structure["forest_id"] = ObjectId;
            structure.EndUpdate();
        }

        /// <summary>
        ///   Removes structure from forest
        /// </summary>
        /// <param name = "structure">Structure to remove</param>
        public void RemoveLumberjack(Structure structure)
        {
            CheckUpdateMode();
            structures.Remove(structure);
        }

        /// <summary>
        ///   Recalculates the rate of the forest and all lumberjack structures around it.
        ///   Must lock all players using the forest and the forest manager.
        /// </summary>
        public void RecalculateForest()
        {
            CheckUpdateMode();

            // Get the number of labors assigned
            int totalLabor = this.Aggregate(0, (current, obj) => current + obj.Stats.Labor);

            // Calculate efficiency
            double playerEfficiency = structures.Count/8d;
            double laborEfficiency = (double)totalLabor/MaxLabor;
            double efficiency = (1 - Math.Abs(playerEfficiency - laborEfficiency))*(structures.Count*0.125);

            float totalRate = 0;
            // Set the appropriate rates
            foreach (var obj in this)
            {
                // Skip structures that are still being built
                if (obj.Lvl == 0)
                    continue;

                // Get the current rate. This will be figure out how much we need to adjust the rate.
                var oldRate = (int)obj["Rate"];

                var newRate = (int)(obj.Stats.Labor*Rate*(1d + efficiency));

                if (newRate != oldRate)
                {
                    // Save the rate in the obj. This is needed so later we can look up how much this object is actually giving.
                    obj.BeginUpdate();
                    obj["Rate"] = newRate;
                    obj.EndUpdate();

                    // Update the cities rate
                    obj.City.BeginUpdate();
                    obj.City.Resource.Wood.Rate += newRate - oldRate;
                    obj.City.EndUpdate();
                }

                totalRate += newRate;
            }

            // Set the forests upkeep
            Wood.Upkeep = (int)totalRate;

            // Set the forests total labor
            Labor = totalLabor;

            SetDepleteAction();

            //Reset the harvest actions
            foreach (var obj in this.Where(obj => obj.Lvl > 0))
            {
                var action = (ForestCampHarvestPassiveAction)obj.City.Worker.FindAction(obj, typeof(ForestCampHarvestPassiveAction));
                if (action != null)
                    action.Reschedule();
                else
                {
                    // Set new harvesting action                             
                    action = new ForestCampHarvestPassiveAction(obj.City.Id, ObjectId);
                    obj.City.Worker.DoPassive(obj, action, true);
                }
            }
        }

        /// <summary>
        ///  Updates the deplete action to fire when appropriately.
        /// </summary>
        private void SetDepleteAction()
        {
            if (DepleteAction != null)
                Global.Scheduler.Remove(DepleteAction);

            double hours = 2*24 + Config.Random.NextDouble()*5;

            if (Wood.Upkeep != 0)
                hours = Wood.Value/(Wood.Upkeep/Config.seconds_per_unit);
            
            Global.Logger.Debug(string.Format("DepleteTime in [{0}] hours Wood.Upkeep[{1}] Wood.Value[{2}]", hours, Wood.Upkeep, Wood.Value));

            DepleteTime = DateTime.UtcNow.AddHours(hours);

            DepleteAction = new ForestDepleteAction(this, DepleteTime);

            Global.Scheduler.Put(DepleteAction);
        }

        #endregion

        #region Updates

        public override void CheckUpdateMode()
        {
            if (!Global.FireEvents || objectId == 0)
                return;

            if (!updating)
                throw new Exception("Changed state outside of begin/end update block");

            MultiObjectLock.ThrowExceptionIfNotLocked(Global.World.Forests);
        }

        public override void EndUpdate()
        {
            if (!updating)
                throw new Exception("Called an endupdate without first calling a beginupdate");

            updating = false;

            Update();
        }

        protected new void Update()
        {
            base.Update();

            if (!Global.FireEvents)
                return;

            if (updating)
                return;

            if (objectId > 0)
                Global.DbManager.Save(this);
        }

        #endregion

        #region IEnumerable<Structure> Members

        public IEnumerator<Structure> GetEnumerator()
        {
            return structures.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        #endregion

        #region IPersistableObject Members

        public string DbTable
        {
            get
            {
                return DB_TABLE;
            }
        }

        public DbColumn[] DbColumns
        {
            get
            {
                IEnumerable<object[]> structuresEnum = structures.Select(structure => new object[] {structure.City.Id, structure.ObjectId});

                return new[]
                       {
                               new DbColumn("labor", Labor, DbType.UInt16), new DbColumn("x", X, DbType.UInt32), new DbColumn("y", Y, DbType.Int32),
                               new DbColumn("level", Lvl, DbType.Byte), new DbColumn("rate", Rate, DbType.Single), new DbColumn("capacity", Wood.Limit, DbType.Int32),
                               new DbColumn("last_realize_time", Wood.LastRealizeTime, DbType.DateTime), new DbColumn("lumber", Wood.RawValue, DbType.Int32),
                               new DbColumn("upkeep", Wood.Upkeep, DbType.Int32), new DbColumn("state", (byte)State.Type, DbType.Boolean),
                               new DbColumn("state_parameters", XmlSerializer.SerializeList(State.Parameters.ToArray()), DbType.String),
                               new DbColumn("deplete_time", DepleteTime, DbType.DateTime),
                               new DbColumn("structures", XmlSerializer.SerializeComplexList(structuresEnum), DbType.String),
                               new DbColumn("in_world", InWorld, DbType.Boolean)
                       };
            }
        }

        public DbColumn[] DbPrimaryKey
        {
            get
            {
                return new[] {new DbColumn("id", ObjectId, DbType.UInt32)};
            }
        }

        public DbDependency[] DbDependencies
        {
            get
            {
                return new DbDependency[] {};
            }
        }

        public bool DbPersisted { get; set; }

        #endregion

        #region Implementation of ICityRegionObject

        public Location GetCityRegionLocation()
        {
            return new Location(X, Y);
        }

        public byte[] GetCityRegionObjectBytes()
        {
            using (var ms = new MemoryStream())
            {
                var bw = new BinaryWriter(ms);
                bw.Write(Lvl);
                bw.Write(objectId);
                bw.Write((ushort)(CityRegionRelX));
                bw.Write((ushort)(CityRegionRelY));
                ms.Position = 0;
                return ms.ToArray();
            }
        }

        public CityRegion.ObjectType GetCityRegionType()
        {
            return CityRegion.ObjectType.Forest;
        }

        #endregion
    }
}