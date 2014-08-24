#region

using System;
using System.Collections.Generic;
using System.Linq;
using Game.Data;
using Game.Data.Forest;
using Game.Logic.Formulas;
using Game.Logic.Procedures;
using Game.Map;
using Game.Setup;
using Game.Util;
using Game.Util.Locking;

#endregion

namespace Game.Logic.Actions
{
    public class ForestCampBuildActiveAction : ScheduledActiveAction
    {
        private ushort campType;

        private uint cityId;

        private uint forestId;

        private ushort labors;

        private readonly Formula formula;

        private readonly IWorld world;

        private readonly IObjectTypeFactory objectTypeFactory;

        private readonly IStructureCsvFactory structureCsvFactory;

        private readonly IForestManager forestManager;

        private readonly ILocker locker;

        private uint lumbermillId;

        private uint campId;

        private readonly ITileLocator tileLocator;

        private readonly CallbackProcedure callbackProcedure;

        public ForestCampBuildActiveAction(Formula formula,
                                           IWorld world,
                                           IObjectTypeFactory objectTypeFactory,
                                           IStructureCsvFactory structureCsvFactory,
                                           IForestManager forestManager,
                                           ILocker locker, 
                                           ITileLocator tileLocator, 
                                           CallbackProcedure callbackProcedure)
        {
            this.formula = formula;
            this.world = world;
            this.objectTypeFactory = objectTypeFactory;
            this.structureCsvFactory = structureCsvFactory;
            this.forestManager = forestManager;
            this.locker = locker;
            this.tileLocator = tileLocator;
            this.callbackProcedure = callbackProcedure;
        }
 
        public ForestCampBuildActiveAction(uint cityId,
                                           uint lumbermillId,
                                           uint forestId,
                                           ushort campType,
                                           ushort labors,
                                           Formula formula,
                                           IWorld world,
                                           IObjectTypeFactory objectTypeFactory,
                                           IStructureCsvFactory structureCsvFactory,
                                           IForestManager forestManager,
                                           ILocker locker, 
                                           ITileLocator tileLocator, 
                                           CallbackProcedure callbackProcedure)
            :this(formula, world, objectTypeFactory, structureCsvFactory, forestManager, locker, tileLocator, callbackProcedure)
        {
            this.cityId = cityId;
            this.lumbermillId = lumbermillId;
            this.forestId = forestId;
            this.labors = labors;
            this.campType = campType;
        }

        public override void LoadProperties(IDictionary<string, string> properties)
        {
            cityId = uint.Parse(properties["city_id"]);
            lumbermillId = uint.Parse(properties["lumbermill_id"]);
            campId = uint.Parse(properties["camp_id"]);
            labors = ushort.Parse(properties["labors"]);
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
            IForest forest;

            if (!world.TryGetObjects(cityId, lumbermillId, out city, out lumbermill) || !forestManager.TryGetValue(forestId, out forest))
            {
                return Error.ObjectNotFound;
            }

            // Count number of camps and verify there's enough space left                
            int campCount = city.Count(s => objectTypeFactory.IsStructureType("ForestCamp", s));
            if (campCount >= 5)
            {
                return Error.ForestCampMaxReached;
            }

            // Make sure some labors are being put in
            if (labors <= 0)
            {
                return Error.LaborNotEnough;
            }
            
            // Make sure we have the specified number of laborers
            int currentInUsedLabor = lumbermill.City.Where(s => objectTypeFactory.IsStructureType("ForestCamp", s)).Sum(x => x.Stats.Labor);
            if (formula.GetLumbermillMaxLabor(lumbermill) < labors + currentInUsedLabor)
            {
                return Error.LaborOverflow;
            }

            // Make sure it's within the limit of a forest camp
            if (labors > formula.GetForestCampMaxLabor(lumbermill))
            {
                return Error.ForestCampMaxLaborReached;
            }

            // Make sure this user is not already milking this forest.
            if (forest.Count(obj => obj.City == city) > 0)
            {
                return Error.AlreadyInForest;
            }

            // Cost requirement
            Resource cost = formula.StructureCost(city, structureCsvFactory.GetBaseStats(campType, 1));

            // Add labor count to the total cost
            cost.Labor += labors;

            if (!city.Resource.HasEnough(cost))
            {
                return Error.ResourceNotEnough;
            }

            // find an open space around the forest
            uint emptyX = 0;
            uint emptyY = 0;
            foreach (var position in tileLocator.ForeachTile(forest.PrimaryPosition.X, forest.PrimaryPosition.Y, 1, false).Reverse())
            {
                // Make sure it's not taken
                if (world.Regions.GetObjectsInTile(position.X, position.Y).Any())
                {
                    continue;
                }

                emptyX = position.X;
                emptyY = position.Y;

                break;
            }

            if (emptyX == 0 || emptyY == 0)
            {
                return Error.ForestFull;
            }

            world.Regions.LockRegion(emptyX, emptyY);

            // add structure to the map                    
            IStructure structure = city.CreateStructure(campType, 0, emptyX, emptyY);

            structure.BeginUpdate();
            structure["Rate"] = 0; // Set initial rate for camp
            structure.Stats.Labor = labors;

            city.BeginUpdate();
            city.Resource.Subtract(cost);
            city.EndUpdate();

            if (!world.Regions.Add(structure))
            {
                city.ScheduleRemove(structure, false);
                city.BeginUpdate();
                city.Resource.Add(cost);
                city.EndUpdate();
                structure.EndUpdate();

                world.Regions.UnlockRegion(emptyX, emptyY);
                return Error.MapFull;
            }

            structure.EndUpdate();

            campId = structure.ObjectId;

            forest.BeginUpdate();
            forest.AddLumberjack(structure);
            forest.RecalculateForest();
            forest.EndUpdate();

            lumbermill.BeginUpdate();
            lumbermill["Labor"] = formula.GetForestCampLaborerString(lumbermill);
            lumbermill.EndUpdate();

            // add to queue for completion
            var campBuildTime = structureCsvFactory.GetTime(campType, 1);
            var actionEndTime = formula.GetLumbermillCampBuildTime(campBuildTime, lumbermill, forest, tileLocator);

            endTime = SystemClock.Now.AddSeconds(CalculateTime(actionEndTime));
            BeginTime = SystemClock.Now;

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

            locker.Lock(forestManager.CallbackLockHandler,
                        new object[] {forestId},
                        city)
                  .Do(() =>
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
                      IForest forest;
                      if (!forestManager.TryGetValue(forestId, out forest))
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
                      structureCsvFactory.GetUpgradedStructure(structure, structure.Type, 1);                      
                      structure.EndUpdate();

                      callbackProcedure.OnStructureUpgrade(structure);

                      // Recalculate the forest
                      forest.BeginUpdate();
                      forest.RecalculateForest();
                      forest.EndUpdate();

                      StateChange(ActionState.Completed);
                  });
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

            locker.Lock(forestManager.CallbackLockHandler, new object[] {forestId}, city)
                  .Do(() =>
                  {
                      if (!IsValid())
                      {
                          return;
                      }

                      // Get camp
                      IStructure structure;
                      if (!city.TryGetStructure(campId, out structure))
                      {
                          StateChange(ActionState.Failed);
                          return;
                      }

                      // Give any cost associated with the camp back (laborers are not done here)
                      city.BeginUpdate();
                      city.Resource.Add(formula.GetActionCancelResource(BeginTime, formula.StructureCost(city, structure.Stats.Base)));
                      city.EndUpdate();

                      city.References.Remove(structure, this);

                      // Remove camp from forest and recalculate forest
                      IForest forest;
                      if (forestManager.TryGetValue(forestId, out forest))
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
                  });
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

        public override Error SystemCancelable
        {
            get
            {
                return Error.UncancelableForestCampBuild;
            }
        }

        #endregion
    }
}