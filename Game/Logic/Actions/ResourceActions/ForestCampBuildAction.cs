#region

using System;
using System.Collections.Generic;
using System.Linq;
using Game.Data;
using Game.Map;
using Game.Setup;
using Game.Util;

#endregion

namespace Game.Logic.Actions {
    class ForestCampBuildAction : ScheduledActiveAction {
        private uint cityId;
        private uint lumbermillId;
        private uint campId;
        private byte labors;
        private ushort campType;
        private uint forestId;

        public ForestCampBuildAction(uint cityId, uint lumbermillId, uint forestId, ushort campType, byte labors) {
            this.cityId = cityId;
            this.lumbermillId = lumbermillId;
            this.forestId = forestId;
            this.labors = labors;
            this.campType = campType;
        }

        public ForestCampBuildAction(uint id, DateTime beginTime, DateTime nextTime, DateTime endTime, int workerType,
                                    byte workerIndex, ushort actionCount, Dictionary<string, string> properties)
            : base(id, beginTime, nextTime, endTime, workerType, workerIndex, actionCount) {
            cityId = uint.Parse(properties["city_id"]);
            lumbermillId = uint.Parse(properties["lumbermill_id"]);
            campId = uint.Parse(properties["camp_id"]);
            labors = byte.Parse(properties["labors"]);
            campType = ushort.Parse(properties["camp_type"]);
            forestId = uint.Parse(properties["forest_id"]);
        }

        public override Error Execute() {
            City city;
            Structure lumbermill;
            Forest forest;

            if (!Global.World.TryGetObjects(cityId, lumbermillId, out city, out lumbermill) || !Global.Forests.TryGetValue(forestId, out forest))
                return Error.OBJECT_NOT_FOUND;

            // Count number of camps and verify there's enough space left                
            int campCount = city.Count(s => ObjectTypeFactory.IsStructureType("ForestCamp", s));
            if (campCount >= Formula.GetMaxForestCount(lumbermill.Lvl)) {
                return Error.FOREST_CAMP_MAX_REACHED;
            }

            // Make sure some labors are being put in
            if (labors <= 0) {
                return Error.LABOR_NOT_ENOUGH;
            }

            // Make sure this user is not already milking this forest.
            if (forest.Count(obj => obj.City == city) > 0) {
                return Error.ALREADY_IN_FOREST;
            }

            // Verify user has access to this forest
            if (forest.Lvl > Formula.GetMaxForestLevel(lumbermill.Lvl)) {
                return Error.FOREST_INACCESSIBLE;
            }

            // Cost requirement
            Resource cost = Formula.StructureCost(city, campType, 1);

            // Add labor count to the total cost
            cost.Labor += labors;

            if (!city.Resource.HasEnough(cost)) {
                return Error.RESOURCE_NOT_ENOUGH;
            }

            // Make sure we can fit this many laborers in the forest and that this user isn't trying to insert more into forest than he can
            if (labors + forest.Labor > forest.MaxLabor || labors > Formula.GetForestMaxLaborPerUser(forest)) {
                return Error.FOREST_FULL;
            }

            // find an open space around the forest
            uint emptyX = 0;
            uint emptyY = 0;
            ReverseTileLocator.foreach_object(forest.X, forest.Y, 1, false, delegate(uint ox, uint oy, uint x, uint y, object custom) {
                                                                         // Check tile type                
                                                                         if (!ObjectTypeFactory.IsTileType("TileBuildable", Global.World.GetTileType(x, y))) {
                                                                             return true;
                                                                         }

                                                                         // Make sure it's not taken
                                                                         if (Global.World[x, y].Count > 0) {
                                                                             return true;
                                                                         }

                                                                         emptyX = x;
                                                                         emptyY = y;

                                                                         return false;
                                                                     }, null);

            if (emptyX == 0 || emptyY == 0) {
                return Error.MAP_FULL;
            }

            Global.World.LockRegion(emptyX, emptyY);

            // add structure to the map                    
            Structure structure = StructureFactory.GetNewStructure(campType, 0);
            structure["Rate"] = 0; // Set initial rate for camp
            structure.X = emptyX;
            structure.Y = emptyY;

            structure.BeginUpdate();
            structure.Stats.Labor = labors;
            city.BeginUpdate();
            city.Resource.Subtract(cost);
            city.EndUpdate();

            city.Add(structure);

            if (!Global.World.Add(structure)) {
                city.ScheduleRemove(structure, false);
                city.BeginUpdate();
                city.Resource.Add(cost);
                city.EndUpdate();

                Global.World.UnlockRegion(emptyX, emptyY);
                return Error.MAP_FULL;
            }

            structure.EndUpdate();

            campId = structure.ObjectId;

            forest.BeginUpdate();
            forest.AddLumberjack(structure);
            forest.RecalculateForest();
            forest.EndUpdate();

            // add to queue for completion
            endTime =
                DateTime.UtcNow.AddSeconds(Config.actions_instant_time
                                               ? 3
                                               : (Formula.BuildTime(StructureFactory.GetTime(campType, 1), city.MainBuilding.Lvl, city.Technologies) + lumbermill.RadiusDistance(forest)));
            beginTime = DateTime.UtcNow;

            city.Worker.References.Add(structure, this);

            Global.World.UnlockRegion(emptyX, emptyY);

            return Error.OK;
        }

        public override void Callback(object custom) {
            City city;
            if (!Global.World.TryGetObjects(cityId, out city)) {
                return;
            }

            using (new CallbackLock(Global.Forests.CallbackLockHandler, new object[] { forestId }, city, Global.Forests)) {
                if (!IsValid()) return;

                Structure structure;
                if (!city.TryGetStructure(campId, out structure)) {
                    // Give back the labors to the city
                    city.BeginUpdate();
                    city.Resource.Labor.Add(labors);
                    city.EndUpdate();                

                    StateChange(ActionState.FAILED);
                    return;
                }

                city.Worker.References.Remove(structure, this);

                // Get forest. If it doesn't exist, we need to delete the structure.
                Forest forest;
                if (!Global.Forests.TryGetValue(forestId, out forest)) {
                    // Give back the labors to the city
                    city.BeginUpdate();
                    city.Resource.Labor.Add(labors);
                    city.EndUpdate();                

                    // Remove the camp
                    structure.BeginUpdate();
                    Global.World.Remove(structure);
                    city.ScheduleRemove(structure, false);
                    structure.EndUpdate();

                    StateChange(ActionState.FAILED);
                    return;
                }

                // Upgrade the camp
                structure.BeginUpdate();
                structure.Technologies.Parent = structure.City.Technologies;
                StructureFactory.GetUpgradedStructure(structure, structure.Type, 1);
                InitFactory.InitGameObject(InitCondition.ON_INIT, structure, structure.Type, structure.Lvl);
                structure.EndUpdate();

                // Recalculate the forest
                forest.BeginUpdate();
                forest.RecalculateForest();
                forest.EndUpdate();

                StateChange(ActionState.COMPLETED);
            }
        }

        public override ActionType Type {
            get { return ActionType.FOREST_CAMP_BUILD; }
        }

        public override Error Validate(string[] parms) {
            if (ushort.Parse(parms[0]) == campType) {
                return Error.OK;
            }

            return Error.ACTION_INVALID;
        }

        private void InterruptCatchAll(bool workerRemoved) {
            City city;
            if (!Global.World.TryGetObjects(cityId, out city)) {
                throw new Exception("City is missing");
            }

            using (new CallbackLock(Global.Forests.CallbackLockHandler, new object[] { forestId }, city, Global.Forests)) {
                if (!IsValid())
                    return;

                // Give laborers back
                city.BeginUpdate();
                city.Resource.Labor.Add(labors);
                city.Resource.Add(Formula.GetActionCancelResource(BeginTime, Formula.StructureCost(city, campType, 1)));
                city.EndUpdate();                

                // Get camp
                Structure structure;
                if (!city.TryGetStructure(campId, out structure)) {
                    StateChange(ActionState.FAILED);
                    return;
                }

                city.Worker.References.Remove(structure, this);

                // Remove camp from forest and recalculate forest
                Forest forest;
                if (Global.Forests.TryGetValue(forestId, out forest)) {
                    forest.BeginUpdate();
                    forest.RemoveLumberjack(structure);
                    forest.RecalculateForest();
                    forest.EndUpdate();
                }

                // Remove the camp                        
                structure.BeginUpdate();
                Global.World.Remove(structure);
                city.ScheduleRemove(structure, false);
                structure.EndUpdate();

                StateChange(ActionState.FAILED);
            }
        }

        public override void UserCancelled() {
            InterruptCatchAll(false);
        }

        public override void WorkerRemoved(bool wasKilled) {
            InterruptCatchAll(true);
        }

        #region IPersistable

        public override string Properties {
            get {
                return
                    XMLSerializer.Serialize(new[]
                                                {
                                                    new XMLKVPair("city_id", cityId), new XMLKVPair("lumbermill_id", lumbermillId), new XMLKVPair("camp_id", campId), new XMLKVPair("labors", labors),
                                                    new XMLKVPair("camp_type", campType), new XMLKVPair("forest_id", forestId)
                                                });
            }
        }

        #endregion
    }
}