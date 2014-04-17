#region

using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using Common;
using Game.Logic;
using Game.Logic.Actions;
using Game.Logic.Actions.ResourceActions;
using Game.Logic.Formulas;
using Game.Map;
using Game.Setup;
using Game.Util;
using Game.Util.Locking;
using Persistance;

#endregion

namespace Game.Data.Forest
{
    public class Forest : SimpleGameObject, IForest
    {
        private readonly ILogger logger = LoggerFactory.Current.GetLogger<Forest>();

        public const string DB_TABLE = "forests";

        private readonly byte lvl = 1;

        private readonly IActionFactory actionFactory;

        private readonly IScheduler scheduler;

        private readonly IDbManager dbManager;

        /// <summary>
        ///     The structures currently getting wood from this forest
        /// </summary>
        private readonly List<IStructure> structures = new List<IStructure>();

        private readonly Formula formula;

        public ForestDepleteAction DepleteAction { get; set; }

        public override byte Size
        {
            get
            {
                return 1;
            }
        }

        public override ushort Type
        {
            get
            {
                return (ushort)Types.Forest;
            }
        }

        public override uint GroupId
        {
            get
            {
                return (uint)SystemGroupIds.Forest;
            }
        }

        /// <summary>
        ///     The lumber availabel at this forest.
        ///     Notice: The rate is not used, only the upkeep. The rate of the forest is kept in a separate variable.
        /// </summary>
        public AggressiveLazyValue Wood { get; set; }

        /// <summary>
        ///     Time until forest is depleted
        ///     Only the db loader should be setting this.
        /// </summary>
        public DateTime DepleteTime { get; set; }

        #region Constructors

        public Forest(uint id, int capacity, uint x, uint y, IActionFactory actionFactory, IScheduler scheduler, IDbManager dbManager, Formula formula) 
            : base(id, x, y)
        {
            this.actionFactory = actionFactory;
            this.scheduler = scheduler;
            this.dbManager = dbManager;
            this.formula = formula;

            Wood = new AggressiveLazyValue(capacity) {Limit = capacity};
        }

        #endregion

        #region Methods

        public void AddLumberjack(IStructure structure)
        {
            CheckUpdateMode();
            structures.Add(structure);

            // Store the forest_id in the structure so we have a way of getting the forest from the structure
            structure.BeginUpdate();
            structure["forest_id"] = ObjectId;
            structure.EndUpdate();
        }

        /// <summary>
        ///     Removes structure from forest
        /// </summary>
        /// <param name="structure">Structure to remove</param>
        public void RemoveLumberjack(IStructure structure)
        {
            CheckUpdateMode();
            structures.Remove(structure);
        }

        /// <summary>
        ///     Recalculates the rate of the forest and all lumberjack structures around it.
        ///     Must lock all players using the forest and the forest manager.
        /// </summary>
        public void RecalculateForest()
        {
            CheckUpdateMode();

            float newEfficiency = ((structures.Count-1) * 0.09f);

            // Set the appropriate rates
            foreach (var obj in this)
            {
                // Skip structures that are still being built
                if (obj.Lvl == 0)
                {
                    continue;
                }

                // Get the current rate. This will be figure out how much we need to adjust the rate.
                var oldRate = (int)obj["Rate"];
                var newRate = formula.GetWoodRateForForestCamp(obj, newEfficiency);

                object efficiency;   // if efficiency is not found(old forest camp), it should be set.
                if (!obj.Properties.TryGet("efficiency", out efficiency) || newRate != oldRate)
                {
                    // Save the rate in the obj. This is needed so later we can look up how much this object is actually giving.
                    obj.BeginUpdate();
                    obj["Rate"] = newRate;
                    obj["efficiency"] = newEfficiency;
                    obj.EndUpdate();

                    // Update the cities rate
                    obj.City.BeginUpdate();
                    obj.City.Resource.Wood.Rate += newRate - oldRate;
                    obj.City.EndUpdate();
                }
            }

            // Set the forests upkeep
            Wood.Upkeep = formula.GetForestUpkeep(structures.Count);

            SetDepleteAction();

            // TODO: See if we should fire an event here and let the forest manager handle the harvest action
            //Reset the harvest actions
            foreach (var obj in this.Where(obj => obj.Lvl > 0))
            {
                var action = obj.City.Worker.FindAction<ForestCampHarvestPassiveAction>(obj);
                if (action != null)
                {
                    action.Reschedule();
                }
                else
                {
                    // Set new harvesting action
                    action = actionFactory.CreateForestCampHarvestPassiveAction(obj.City.Id, ObjectId);
                    obj.City.Worker.DoPassive(obj, action, true);
                }
            }
        }

        /// <summary>
        ///     Updates the deplete action to fire when appropriately.
        /// </summary>
        private void SetDepleteAction()
        {
            if (DepleteAction != null)
            {
                scheduler.Remove(DepleteAction);
            }

            double hours = 2 * 24 + Config.Random.NextDouble() * 24;

            if (structures.Count != 0)
            {
                hours = Wood.Value / (Wood.Upkeep / Config.seconds_per_unit);
            }

            logger.Trace(string.Format("DepleteTime in [{0}] hours Wood.Upkeep[{1}] Wood.Value[{2}]",
                                              hours,
                                              Wood.Upkeep,
                                              Wood.Value));

            DepleteTime = DateTime.UtcNow.AddSeconds(hours * 3600);

            DepleteAction = actionFactory.CreateForestDepleteAction(this, DepleteTime);

            scheduler.Put(DepleteAction);
        }

        #endregion

        #region Updates

        protected override void CheckUpdateMode()
        {
            if (!Global.Current.FireEvents || !DbPersisted)
            {
                return;
            }

            if (!Updating)
            {
                throw new Exception("Changed state outside of begin/end update block");
            }

            DefaultMultiObjectLock.ThrowExceptionIfNotLocked(this);
        }

        protected override bool Update()
        {
            var update = base.Update();

            if (update && ObjectId > 0)
            {
                dbManager.Save(this);
            }

            return update;
        }

        #endregion

        #region IEnumerable<Structure> Members

        public IEnumerator<IStructure> GetEnumerator()
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
                IEnumerable<object[]> structuresEnum =
                        structures.Select(structure => new object[] {structure.City.Id, structure.ObjectId});

                return new[]
                {
                        new DbColumn("x", PrimaryPosition.X, DbType.UInt32),
                        new DbColumn("y", PrimaryPosition.Y, DbType.Int32), 

                        new DbColumn("capacity", Wood.Limit, DbType.Int32),
                        new DbColumn("last_realize_time", Wood.LastRealizeTime, DbType.DateTime),
                        new DbColumn("lumber", Wood.RawValue, DbType.Int32),
                        new DbColumn("upkeep", Wood.Upkeep, DbType.Int32),
                        new DbColumn("state", (byte)State.Type, DbType.Boolean),
                        new DbColumn("state_parameters",
                                     XmlSerializer.SerializeList(State.Parameters.ToArray()),
                                     DbType.String),
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

        public IEnumerable<DbDependency> DbDependencies
        {
            get
            {
                return new DbDependency[] {};
            }
        }

        public bool DbPersisted { get; set; }

        #endregion

        public byte MiniMapSize
        {
            get
            {
                return Size;
            }
        }

        public byte[] GetMiniMapObjectBytes()
        {
            using (var ms = new MemoryStream())
            {
                var bw = new BinaryWriter(ms);
                bw.Write(structures.Count);
                bw.Write(UnixDateTime.DateTimeToUnix(DepleteTime.ToUniversalTime()));
                ms.Position = 0;
                return ms.ToArray();
            }
        }

        public MiniMapRegion.ObjectType MiniMapObjectType
        {
            get
            {
                return MiniMapRegion.ObjectType.Forest;
            }
        }

        public uint MiniMapGroupId
        {
            get
            {
                return GroupId;
            }
        }

        public uint MiniMapObjectId
        {
            get
            {
                return ObjectId;
            }
        }

        public override int Hash
        {
            get
            {
                return unchecked((int)ObjectId);
            }
        }

        public override object Lock
        {
            get
            {
                return this;
            }
        }
    }
}