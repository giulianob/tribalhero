#region

using System;
using System.Collections.Generic;
using System.Linq;
using Game.Data;
using Game.Logic.Formulas;
using Game.Map;
using Game.Setup;
using Game.Util;
using Game.Util.Locking;

#endregion

namespace Game.Logic.Actions
{
    public class ForestCampBuildActiveAction : ScheduledActiveAction
    {
        private readonly ushort campType;

        private readonly uint cityId;

        private readonly uint forestId;

        private readonly byte labors;

        private readonly Formula formula;

        private readonly IWorld world;

        private readonly ObjectTypeFactory objectTypeFactory;

        private readonly StructureFactory structureFactory;

        private readonly InitFactory initFactory;

        private readonly ReverseTileLocator reverseTileLocator;

        private readonly ILocker locker;

        private readonly uint lumbermillId;

        private uint campId;

        public ForestCampBuildActiveAction(uint cityId,
                                           uint lumbermillId,
                                           uint forestId,
                                           ushort campType,
                                           byte labors,
                                           Formula formula,
                                           IWorld world,
                                           ObjectTypeFactory objectTypeFactory,
                                           StructureFactory structureFactory,
                                           InitFactory initFactory,
                                           ReverseTileLocator reverseTileLocator,
            ILocker locker)
        {
            this.cityId = cityId;
            this.lumbermillId = lumbermillId;
            this.forestId = forestId;
            this.labors = labors;
            this.formula = formula;
            this.world = world;
            this.objectTypeFactory = objectTypeFactory;
            this.structureFactory = structureFactory;
            this.initFactory = initFactory;
            this.reverseTileLocator = reverseTileLocator;
            this.locker = locker;
            this.campType = campType;
        }

        public ForestCampBuildActiveAction(uint id,
                                           DateTime beginTime,
                                           DateTime nextTime,
                                           DateTime endTime,
                                           int workerType,
                                           byte workerIndex,
                                           ushort actionCount,
                                           Dictionary<string, string> properties,
                                           Formula formula,
                                           IWorld world,
                                           ObjectTypeFactory objectTypeFactory,
                                           StructureFactory structureFactory,
                                           InitFactory initFactory,
                                           ReverseTileLocator reverseTileLocator,
            ILocker locker)
            : base(id, beginTime, nextTime, endTime, workerType, workerIndex, actionCount)
        {
            this.formula = formula;
            this.world = world;
            this.objectTypeFactory = objectTypeFactory;
            this.structureFactory = structureFactory;
            this.initFactory = initFactory;
            this.reverseTileLocator = reverseTileLocator;
            this.locker = locker;
            cityId = uint.Parse(properties["city_id"]);
            lumbermillId = uint.Parse(properties["lumbermill_id"]);
            campId = uint.Parse(properties["camp_id"]);
            labors = byte.Parse(properties["labors"]);
            campType = ushort.Parse(properties["camp_type"]);
            forestId = uint.Parse(properties["forest_id"]);
        }

        public override ConcurrencyType ActionConcurrency
        {
            get
            {
                return ConcurrencyType.Concurrent;
            }
        }

        public override ActionType Type
        {
            get
            {
                return ActionType.ForestCampBuildActive;
            }
        }

        public override Error Execute()
        {
            ICity city;
            IStructure lumbermill;
            Forest forest;

            if (!world.TryGetObjects(cityId, lumbermillId, out city, out lumbermill) || !world.Forests.TryGetValue(forestId, out forest))
            {
                return Error.ObjectNotFound;
            }

            // Count number of camps and verify there's enough space left                
            int campCount = city.Count(s => objectTypeFactory.IsStructureType("ForestCamp", s));
            if (campCount >= formula.GetMaxForestCount(lumbermill.Lvl))
            {
                return Error.ForestCampMaxReached;
            }

            // Make sure some labors are being put in
            if (labors <= 0)
            {
                return Error.LaborNotEnough;
            }

            // Make sure this user is not already milking this forest.
            if (forest.Count(obj => obj.City == city) > 0)
            {
                return Error.AlreadyInForest;
            }

            // Verify user has access to this forest
            if (forest.Lvl > formula.GetMaxForestLevel(lumbermill.Lvl))
            {
                return Error.ForestInaccessible;
            }

            // Cost requirement
            Resource cost = formula.StructureCost(city, campType, 1);

            // Add labor count to the total cost
            cost.Labor += labors;

            if (!city.Resource.HasEnough(cost))
            {
                return Error.ResourceNotEnough;
            }

            // Make sure we can fit this many laborers in the forest and that this user isn't trying to insert more into forest than he can
            if (labors + forest.Labor > forest.MaxLabor || labors > formula.GetForestMaxLaborPerUser(forest))
            {
                return Error.ForestFull;
            }

            // find an open space around the forest
            uint emptyX = 0;
            uint emptyY = 0;
            reverseTileLocator.ForeachObject(forest.X,
                                             forest.Y,
                                             1,
                                             false,
                                             delegate(uint ox, uint oy, uint x, uint y, object custom)
                                                 {
                                                     // Check tile type                
                                                     if (!objectTypeFactory.IsTileType("TileBuildable", world.Regions.GetTileType(x, y)))
                                                     {
                                                         return true;
                                                     }

                                                     // Make sure it's not taken
                                                     if (world[x, y].Count > 0)
                                                     {
                                                         return true;
                                                     }

                                                     emptyX = x;
                                                     emptyY = y;

                                                     return false;
                                                 },
                                             null);

            if (emptyX == 0 || emptyY == 0)
            {
                return Error.MapFull;
            }

            world.Regions.LockRegion(emptyX, emptyY);

            // add structure to the map                    
            IStructure structure = structureFactory.GetNewStructure(campType, 0);
            structure["Rate"] = 0; // Set initial rate for camp
            structure.X = emptyX;
            structure.Y = emptyY;

            structure.BeginUpdate();
            structure.Stats.Labor = labors;
            city.BeginUpdate();
            city.Resource.Subtract(cost);
            city.EndUpdate();

            city.Add(structure);

            if (!world.Regions.Add(structure))
            {
                city.ScheduleRemove(structure, false);
                city.BeginUpdate();
                city.Resource.Add(cost);
                city.EndUpdate();

                world.Regions.UnlockRegion(emptyX, emptyY);
                return Error.MapFull;
            }

            structure.EndUpdate();

            campId = structure.ObjectId;

            forest.BeginUpdate();
            forest.AddLumberjack(structure);
            forest.RecalculateForest();
            forest.EndUpdate();

            // add to queue for completion
            var actionEndTime = formula.BuildTime(structureFactory.GetTime(campType, 1), city, city.Technologies) +
                                lumbermill.TileDistance(forest) * 5;

            endTime = DateTime.UtcNow.AddSeconds(CalculateTime(actionEndTime));
            BeginTime = DateTime.UtcNow;

            city.References.Add(structure, this);

            world.Regions.UnlockRegion(emptyX, emptyY);

            return Error.Ok;
        }

        public override void Callback(object custom)
        {
            ICity city;
            if (!world.TryGetObjects(cityId, out city))
            {
                return;
            }

            using (
                    locker.Lock(world.Forests.CallbackLockHandler,
                                             new object[] { forestId },
                                             city,
                                             world.Forests))
            {
                if (!IsValid())
                {
                    return;
                }

                IStructure structure;
                if (!city.TryGetStructure(campId, out structure))
                {
                    // Give back the labors to the city
                    city.BeginUpdate();
                    city.Resource.Labor.Add(labors);
                    city.EndUpdate();

                    StateChange(ActionState.Failed);
                    return;
                }

                city.References.Remove(structure, this);

                // Get forest. If it doesn't exist, we need to delete the structure.
                Forest forest;
                if (!world.Forests.TryGetValue(forestId, out forest))
                {
                    // Remove the camp
                    structure.BeginUpdate();
                    world.Regions.Remove(structure);
                    city.ScheduleRemove(structure, false);
                    structure.EndUpdate();

                    StateChange(ActionState.Failed);
                    return;
                }

                // Upgrade the camp
                structure.BeginUpdate();
                structure.Technologies.Parent = structure.City.Technologies;
                structureFactory.GetUpgradedStructure(structure, structure.Type, 1);
                initFactory.InitGameObject(InitCondition.OnInit, structure, structure.Type, structure.Lvl);
                structure.EndUpdate();

                // Recalculate the forest
                forest.BeginUpdate();
                forest.RecalculateForest();
                forest.EndUpdate();

                StateChange(ActionState.Completed);
            }
        }

        public override Error Validate(string[] parms)
        {
            if (ushort.Parse(parms[0]) == campType)
            {
                return Error.Ok;
            }

            return Error.ActionInvalid;
        }

        private void InterruptCatchAll()
        {
            ICity city;
            if (!world.TryGetObjects(cityId, out city))
            {
                throw new Exception("City is missing");
            }

            using (locker.Lock(world.Forests.CallbackLockHandler, new object[] { forestId }, city, world.Forests))
            {
                if (!IsValid())
                {
                    return;
                }

                // Give laborers back
                city.BeginUpdate();
                city.Resource.Add(formula.GetActionCancelResource(BeginTime, formula.StructureCost(city, campType, 1)));
                city.EndUpdate();

                // Get camp
                IStructure structure;
                if (!city.TryGetStructure(campId, out structure))
                {
                    StateChange(ActionState.Failed);
                    return;
                }

                city.References.Remove(structure, this);

                // Remove camp from forest and recalculate forest
                Forest forest;
                if (world.Forests.TryGetValue(forestId, out forest))
                {
                    forest.BeginUpdate();
                    forest.RemoveLumberjack(structure);
                    forest.RecalculateForest();
                    forest.EndUpdate();
                }

                // Remove the camp                        
                structure.BeginUpdate();
                world.Regions.Remove(structure);
                city.ScheduleRemove(structure, false);
                structure.EndUpdate();

                StateChange(ActionState.Failed);
            }
        }

        public override void UserCancelled()
        {
            InterruptCatchAll();
        }

        public override void WorkerRemoved(bool wasKilled)
        {
            InterruptCatchAll();
        }

        #region IPersistable

        public override string Properties
        {
            get
            {
                return
                        XmlSerializer.Serialize(new[]
                        {
                                new XmlKvPair("city_id", cityId), new XmlKvPair("lumbermill_id", lumbermillId),
                                new XmlKvPair("camp_id", campId), new XmlKvPair("labors", labors),
                                new XmlKvPair("camp_type", campType), new XmlKvPair("forest_id", forestId)
                        });
            }
        }

        #endregion
    }
}