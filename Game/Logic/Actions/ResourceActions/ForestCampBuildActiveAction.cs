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
using Ninject;

#endregion

namespace Game.Logic.Actions
{
    public class ForestCampBuildActiveAction : ScheduledActiveAction
    {
        private readonly ushort campType;

        private readonly uint cityId;

        private readonly uint forestId;

        private readonly byte labors;

        private readonly uint lumbermillId;

        private uint campId;

        public ForestCampBuildActiveAction(uint cityId, uint lumbermillId, uint forestId, ushort campType, byte labors)
        {
            this.cityId = cityId;
            this.lumbermillId = lumbermillId;
            this.forestId = forestId;
            this.labors = labors;
            this.campType = campType;
        }

        public ForestCampBuildActiveAction(uint id,
                                           DateTime beginTime,
                                           DateTime nextTime,
                                           DateTime endTime,
                                           int workerType,
                                           byte workerIndex,
                                           ushort actionCount,
                                           Dictionary<string, string> properties)
                : base(id, beginTime, nextTime, endTime, workerType, workerIndex, actionCount)
        {
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

            if (!World.Current.TryGetObjects(cityId, lumbermillId, out city, out lumbermill) ||
                !World.Current.Forests.TryGetValue(forestId, out forest))
            {
                return Error.ObjectNotFound;
            }

            // Count number of camps and verify there's enough space left                
            int campCount = city.Count(s => Ioc.Kernel.Get<ObjectTypeFactory>().IsStructureType("ForestCamp", s));
            if (campCount >= Formula.Current.GetMaxForestCount(lumbermill.Lvl))
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
            if (forest.Lvl > Formula.Current.GetMaxForestLevel(lumbermill.Lvl))
            {
                return Error.ForestInaccessible;
            }

            // Cost requirement
            Resource cost = Formula.Current.StructureCost(city, campType, 1);

            // Add labor count to the total cost
            cost.Labor += labors;

            if (!city.Resource.HasEnough(cost))
            {
                return Error.ResourceNotEnough;
            }

            // Make sure we can fit this many laborers in the forest and that this user isn't trying to insert more into forest than he can
            if (labors + forest.Labor > forest.MaxLabor || labors > Formula.Current.GetForestMaxLaborPerUser(forest))
            {
                return Error.ForestFull;
            }

            // find an open space around the forest
            uint emptyX = 0;
            uint emptyY = 0;
            ReverseTileLocator.Current.ForeachObject(forest.X,
                                                     forest.Y,
                                                     1,
                                                     false,
                                                     delegate(uint ox, uint oy, uint x, uint y, object custom)
                                                         {
                                                             // Check tile type                
                                                             if (
                                                                     !Ioc.Kernel.Get<ObjectTypeFactory>()
                                                                         .IsTileType("TileBuildable",
                                                                                     World.Current.Regions.GetTileType(
                                                                                                                       x,
                                                                                                                       y)))
                                                             {
                                                                 return true;
                                                             }

                                                             // Make sure it's not taken
                                                             if (World.Current[x, y].Count > 0)
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

            World.Current.Regions.LockRegion(emptyX, emptyY);

            // add structure to the map                    
            IStructure structure = Ioc.Kernel.Get<StructureFactory>().GetNewStructure(campType, 0);
            structure["Rate"] = 0; // Set initial rate for camp
            structure.X = emptyX;
            structure.Y = emptyY;

            structure.BeginUpdate();
            structure.Stats.Labor = labors;
            city.BeginUpdate();
            city.Resource.Subtract(cost);
            city.EndUpdate();

            city.Add(structure);

            if (!World.Current.Regions.Add(structure))
            {
                city.ScheduleRemove(structure, false);
                city.BeginUpdate();
                city.Resource.Add(cost);
                city.EndUpdate();

                World.Current.Regions.UnlockRegion(emptyX, emptyY);
                return Error.MapFull;
            }

            structure.EndUpdate();

            campId = structure.ObjectId;

            forest.BeginUpdate();
            forest.AddLumberjack(structure);
            forest.RecalculateForest();
            forest.EndUpdate();

            // add to queue for completion
            endTime =
                    DateTime.UtcNow.AddSeconds(
                                               CalculateTime(
                                                             Formula.Current.BuildTime(
                                                                                       Ioc.Kernel.Get<StructureFactory>()
                                                                                          .GetTime(campType, 1),
                                                                                       city,
                                                                                       city.Technologies) +
                                                             lumbermill.TileDistance(forest) * 30));
            BeginTime = DateTime.UtcNow;

            city.References.Add(structure, this);

            World.Current.Regions.UnlockRegion(emptyX, emptyY);

            return Error.Ok;
        }

        public override void Callback(object custom)
        {
            ICity city;
            if (!World.Current.TryGetObjects(cityId, out city))
            {
                return;
            }

            using (
                    Concurrency.Current.Lock(World.Current.Forests.CallbackLockHandler,
                                             new object[] {forestId},
                                             city,
                                             World.Current.Forests))
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
                if (!World.Current.Forests.TryGetValue(forestId, out forest))
                {
                    // Remove the camp
                    structure.BeginUpdate();
                    World.Current.Regions.Remove(structure);
                    city.ScheduleRemove(structure, false);
                    structure.EndUpdate();

                    StateChange(ActionState.Failed);
                    return;
                }

                // Upgrade the camp
                structure.BeginUpdate();
                structure.Technologies.Parent = structure.City.Technologies;
                Ioc.Kernel.Get<StructureFactory>().GetUpgradedStructure(structure, structure.Type, 1);
                Ioc.Kernel.Get<InitFactory>()
                   .InitGameObject(InitCondition.OnInit, structure, structure.Type, structure.Lvl);
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

        private void InterruptCatchAll(bool workerRemoved)
        {
            ICity city;
            if (!World.Current.TryGetObjects(cityId, out city))
            {
                throw new Exception("City is missing");
            }

            using (
                    Concurrency.Current.Lock(World.Current.Forests.CallbackLockHandler,
                                             new object[] {forestId},
                                             city,
                                             World.Current.Forests))
            {
                if (!IsValid())
                {
                    return;
                }

                // Give laborers back
                city.BeginUpdate();
                city.Resource.Add(Formula.Current.GetActionCancelResource(BeginTime,
                                                                          Formula.Current.StructureCost(city,
                                                                                                        campType,
                                                                                                        1)));
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
                if (World.Current.Forests.TryGetValue(forestId, out forest))
                {
                    forest.BeginUpdate();
                    forest.RemoveLumberjack(structure);
                    forest.RecalculateForest();
                    forest.EndUpdate();
                }

                // Remove the camp                        
                structure.BeginUpdate();
                World.Current.Regions.Remove(structure);
                city.ScheduleRemove(structure, false);
                structure.EndUpdate();

                StateChange(ActionState.Failed);
            }
        }

        public override void UserCancelled()
        {
            InterruptCatchAll(false);
        }

        public override void WorkerRemoved(bool wasKilled)
        {
            InterruptCatchAll(true);
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