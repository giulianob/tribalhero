#region

using System;
using System.Collections.Generic;
using System.Linq;
using Game.Data;
using Game.Map;
using Game.Setup;
using Game.Util;

#endregion

namespace Game.Logic.Actions
{
    class StructureBuildAction : ScheduledActiveAction
    {
        private uint cityId;
        private uint structureId;
        private ushort type;
        private uint x, y;
        private Resource cost;

        public StructureBuildAction(uint cityId, ushort type, uint x, uint y)
        {
            this.cityId = cityId;
            this.type = type;
            this.x = x;
            this.y = y;
        }

        public StructureBuildAction(uint id, DateTime beginTime, DateTime nextTime, DateTime endTime, int workerType,
                                    byte workerIndex, ushort actionCount, Dictionary<string, string> properties)
            : base(id, beginTime, nextTime, endTime, workerType, workerIndex, actionCount)
        {
            cityId = uint.Parse(properties["city_id"]);
            structureId = uint.Parse(properties["structure_id"]);
            x = uint.Parse(properties["x"]);
            y = uint.Parse(properties["y"]);
            type = ushort.Parse(properties["type"]);
        }

        #region IAction Members

        public override Error Execute()
        {
            City city;
            if (!Global.World.TryGetObjects(cityId, out city))
                return Error.OBJECT_NOT_FOUND;

            Global.World.LockRegion(x, y);

            // cost requirement
            cost = Formula.StructureCost(city, type, 1);
            if (!city.Resource.HasEnough(cost))
            {
                Global.World.UnlockRegion(x, y);
                return Error.RESOURCE_NOT_ENOUGH;
            }

            // radius requirements
            if (city.MainBuilding.TileDistance(x, y) >= city.Radius)
            {
                Global.World.UnlockRegion(x, y);
                return Error.LAYOUT_NOT_FULLFILLED;
            }

            // layout requirement
            if (!ReqirementFactory.GetLayoutRequirement(type, 1).Validate(WorkerObject as Structure, type, x, y)) {            
                Global.World.UnlockRegion(x, y);
                return Error.LAYOUT_NOT_FULLFILLED;
            }

            // check if tile is occupied
            if (Global.World[x, y].Exists(obj => obj is Structure))
            {
                Global.DbManager.Rollback();
                Global.World.UnlockRegion(x, y);
                StateChange(ActionState.FAILED);
                return Error.STRUCTURE_EXISTS;
            }

            // check for road requirements       
            bool roadRequired = !ObjectTypeFactory.IsStructureType("NoRoadRequired", type);
            bool breaksRoad = false;
            bool buildingOnRoad = RoadManager.IsRoad(x, y);

            if (roadRequired) {
                if (buildingOnRoad) {
                    foreach (Structure str in city) {
                        if (str == city.MainBuilding)
                            continue;

                        if (!RoadPathFinder.HasPath(new Location(str.X, str.Y), new Location(city.MainBuilding.X, city.MainBuilding.Y), city, new Location(x, y))) {
                            breaksRoad = true;
                            break;
                        }
                    }
                }

                if (breaksRoad) {
                    Global.World.UnlockRegion(x, y);
                    return Error.ROAD_DESTROY_UNIQUE_PATH;
                }

                bool hasRoad = false;

                RadiusLocator.foreach_object(x, y, 1, false, delegate(uint origX, uint origY, uint x1, uint y1, object custom) {
                                                                 // TODO: Fix radius locator for each
                                                                 if (SimpleGameObject.RadiusDistance(origX, origY, x1, y1) != 1)
                                                                     return true;

                                                                 Structure curStruct = (Structure) Global.World[x1, y1].Where(obj => obj is Structure).FirstOrDefault();

                                                                 bool hasStructure = curStruct != null;

                                                                 // Make sure we have a road around this building
                                                                 if (!hasRoad && !hasStructure && RoadManager.IsRoad(x1, y1)) {
                                                                     if (!buildingOnRoad ||
                                                                         RoadPathFinder.HasPath(new Location(x1, y1), new Location(city.MainBuilding.X, city.MainBuilding.Y), city,
                                                                                                new Location(origX, origY), true))
                                                                         hasRoad = true;
                                                                 }

                                                                 return true;
                                                             }, null);

                if (!hasRoad) {
                    Global.World.UnlockRegion(x, y);
                    return Error.ROAD_NOT_AROUND;
                }
            }
            else {
                // Cant build on road if this building doesnt require roads
                if (buildingOnRoad) {
                    Global.World.UnlockRegion(x, y);
                    return Error.ROAD_DESTROY_UNIQUE_PATH;                    
                }
            }

            // add structure to the map                    
            Structure structure = StructureFactory.GetNewStructure(type, 0);
            structure.X = x;
            structure.Y = y;

            structure.BeginUpdate();
            city.BeginUpdate();
            city.Resource.Subtract(cost);
            city.EndUpdate();

            city.Add(structure);

            if (!Global.World.Add(structure))
            {
                city.ScheduleRemove(structure, false);
                city.BeginUpdate();
                city.Resource.Add(cost);
                city.EndUpdate();

                Global.World.UnlockRegion(x, y);
                return Error.MAP_FULL;
            }

            structure.EndUpdate();

            structureId = structure.ObjectId;

            // add to queue for completion
            endTime = DateTime.UtcNow.AddSeconds(Config.actions_instant_time ? 3 : Formula.BuildTime(StructureFactory.GetTime(type, 1), city.MainBuilding.Lvl, city.Technologies));
            beginTime = DateTime.UtcNow;

            city.Worker.References.Add(structure, this);

            Global.World.UnlockRegion(x, y);

            return Error.OK;
        }

        public override void Callback(object custom)
        {
            City city;
            using (new MultiObjectLock(cityId, out city))
            {
                if (!IsValid())
                    return;

                Structure structure;
                if (!city.TryGetStructure(structureId, out structure))
                {
                    StateChange(ActionState.FAILED);
                    return;
                }

                city.Worker.References.Remove(structure, this);
                structure.BeginUpdate();
                structure.Technologies.Parent = structure.City.Technologies;
                StructureFactory.GetUpgradedStructure(structure, structure.Type, 1);
                InitFactory.InitGameObject(InitCondition.ON_INIT, structure, structure.Type, structure.Lvl);

                structure.EndUpdate();

                StateChange(ActionState.COMPLETED);
            }
        }

        public override ActionType Type
        {
            get { return ActionType.STRUCTURE_BUILD; }
        }

        public override Error Validate(string[] parms)
        {
            City city;

            if (!Global.World.TryGetObjects(cityId, out city))
                return Error.OBJECT_NOT_FOUND;

            if (ushort.Parse(parms[0]) == type)
            {
                if (parms[1].Length == 0)
                {
                    ushort tileType = Global.World.GetTileType(x, y);
                    if (RoadManager.IsRoad(x, y) || ObjectTypeFactory.IsTileType("TileBuildable", tileType))
                        return Error.OK;

                    return Error.TILE_MISMATCH;
                }
                else
                {
                    string[] tokens = parms[1].Split('|');
                    ushort tileType = Global.World.GetTileType(x, y);
                    foreach (string str in tokens)
                    {
                        if (ObjectTypeFactory.IsTileType(str, tileType))
                            return Error.OK;
                    }
                    return Error.TILE_MISMATCH;
                }
            }
            return Error.ACTION_INVALID;
        }

        #endregion

        private void InterruptCatchAll(bool wasKilled)
        {
            City city;
            using (new MultiObjectLock(cityId, out city))
            {
                if (!IsValid())
                    return;

                Structure structure;
                if (!city.TryGetStructure(structureId, out structure))
                    return;

                city.Worker.References.Remove(structure, this);

                structure.BeginUpdate();
                Global.World.Remove(structure);
                city.ScheduleRemove(structure, wasKilled);
                structure.EndUpdate();

                if (!wasKilled)
                {
                    city.BeginUpdate();
                    city.Resource.Add(Formula.GetActionCancelResource(BeginTime,cost));
                    city.EndUpdate();
                }

                StateChange(ActionState.FAILED);
            }
        }

        public override void UserCancelled()
        {
            InterruptCatchAll(false);
        }

        public override void WorkerRemoved(bool wasKilled)
        {
            InterruptCatchAll(wasKilled);
        }

        #region IPersistable

        public override string Properties
        {
            get
            {
                return
                    XMLSerializer.Serialize(new[] {
                                                                new XMLKVPair("type", type), new XMLKVPair("x", x), new XMLKVPair("y", y),
                                                                new XMLKVPair("city_id", cityId),
                                                                new XMLKVPair("structure_id", structureId)
                                                            });
            }
        }

        #endregion
    }
}