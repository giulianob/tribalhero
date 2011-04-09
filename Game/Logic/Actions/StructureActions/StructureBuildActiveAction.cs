#region

using System;
using System.Collections.Generic;
using System.Linq;
using Game.Data;
using Game.Logic.Formulas;
using Game.Map;
using Game.Setup;
using Game.Util;

#endregion

namespace Game.Logic.Actions
{
    class StructureBuildActiveAction : ScheduledActiveAction
    {
        private readonly uint cityId;
        private readonly ushort type;
        private readonly uint x;
        private readonly uint y;
        private Resource cost;
        private uint structureId;
        private byte level;

        public StructureBuildActiveAction(uint cityId, ushort type, uint x, uint y, byte level)
        {
            this.cityId = cityId;
            this.type = type;
            this.x = x;
            this.y = y;
            this.level = level;
        }

        public StructureBuildActiveAction(uint id,
                                    DateTime beginTime,
                                    DateTime nextTime,
                                    DateTime endTime,
                                    int workerType,
                                    byte workerIndex,
                                    ushort actionCount,
                                    Dictionary<string, string> properties) : base(id, beginTime, nextTime, endTime, workerType, workerIndex, actionCount)
        {
            cityId = uint.Parse(properties["city_id"]);
            structureId = uint.Parse(properties["structure_id"]);
            x = uint.Parse(properties["x"]);
            y = uint.Parse(properties["y"]);
            type = ushort.Parse(properties["type"]);
            cost = new Resource(int.Parse(properties["crop"]),
                                int.Parse(properties["gold"]),
                                int.Parse(properties["iron"]),
                                int.Parse(properties["wood"]),
                                int.Parse(properties["labor"]));
            string tmp;
            level = properties.TryGetValue("level",out tmp) ? byte.Parse(tmp) : (byte)1;          
        }

        public ushort BuildType
        {
            get
            {
                return type;
            }
        }

        public override ActionType Type
        {
            get
            {
                return ActionType.StructureBuildActive;
            }
        }

        public override Error Execute()
        {
            City city;
            if (!Global.World.TryGetObjects(cityId, out city))
                return Error.ObjectNotFound;

            if (!ObjectTypeFactory.IsStructureType("UnlimitedBuilding", type) && city.Worker.ActiveActions.Values.Count(action => action.ActionId != ActionId && (action.Type == ActionType.StructureBuildActive || action.Type == ActionType.StructureUpgradeActive) && !ObjectTypeFactory.IsStructureType("UnlimitedBuilding", ((StructureBuildActiveAction)action).type)) >= 2)
                return Error.ActionAlreadyInProgress;

            Global.World.LockRegion(x, y);

            // cost requirement
            cost = Formula.StructureCost(city, type, level);
            if (!city.Resource.HasEnough(cost))
            {
                Global.World.UnlockRegion(x, y);
                return Error.ResourceNotEnough;
            }

            // radius requirements
            if (SimpleGameObject.TileDistance(city.X, city.Y, x, y) >= city.Radius)
            {
                Global.World.UnlockRegion(x, y);
                return Error.LayoutNotFullfilled;
            }

            // layout requirement
            if (!RequirementFactory.GetLayoutRequirement(type, level).Validate(WorkerObject as Structure, type, x, y))
            {
                Global.World.UnlockRegion(x, y);
                return Error.LayoutNotFullfilled;
            }

            // check if tile is occupied
            if (Global.World[x, y].Exists(obj => obj is Structure))
            {
                Global.World.UnlockRegion(x, y);
                return Error.StructureExists;
            }

            // check for road requirements       
            bool roadRequired = !ObjectTypeFactory.IsStructureType("NoRoadRequired", type);
            bool buildingOnRoad = RoadManager.IsRoad(x, y);

            if (roadRequired)
            {
                if (buildingOnRoad)
                {
                    bool breaksRoad = false;

                    foreach (var str in city)
                    {
                        if (str.IsMainBuilding)
                            continue;

                        if (ObjectTypeFactory.IsStructureType("NoRoadRequired", str.Type))
                            continue;

                        if (
                                !RoadPathFinder.HasPath(new Location(str.X, str.Y),
                                                        new Location(city.X, city.Y),
                                                        city,
                                                        new Location(x, y)))
                        {
                            breaksRoad = true;
                            break;
                        }
                    }

                    if (breaksRoad)
                    {
                        Global.World.UnlockRegion(x, y);
                        return Error.RoadDestroyUniquePath;
                    }

                    // Make sure all neighboring roads have a diff path
                    bool allNeighborsHaveOtherPaths = true;
                    RadiusLocator.ForeachObject(x,
                                                y,
                                                1,
                                                false,
                                                delegate(uint origX, uint origY, uint x1, uint y1, object custom)
                                                    {
                                                        if (SimpleGameObject.RadiusDistance(origX, origY, x1, y1) != 1)
                                                            return true;

                                                        if (city.X == x1 && city.Y == y1)
                                                            return true;

                                                        if (RoadManager.IsRoad(x1, y1))
                                                        {
                                                            if (
                                                                    !RoadPathFinder.HasPath(new Location(x1, y1),
                                                                                            new Location(city.X, city.Y),
                                                                                            city,
                                                                                            new Location(origX, origY)))
                                                            {
                                                                allNeighborsHaveOtherPaths = false;
                                                                return false;
                                                            }
                                                        }

                                                        return true;
                                                    },
                                                null);

                    if (!allNeighborsHaveOtherPaths)
                    {
                        Global.World.UnlockRegion(x, y);
                        return Error.RoadDestroyUniquePath;
                    }
                }

                bool hasRoad = false;

                RadiusLocator.ForeachObject(x,
                                            y,
                                            1,
                                            false,
                                            delegate(uint origX, uint origY, uint x1, uint y1, object custom)
                                                {
                                                    // TODO: Fix radius locator for each
                                                    if (SimpleGameObject.RadiusDistance(origX, origY, x1, y1) != 1)
                                                        return true;

                                                    var curStruct = (Structure)Global.World[x1, y1].Where(obj => obj is Structure).FirstOrDefault();

                                                    bool hasStructure = curStruct != null;

                                                    // Make sure we have a road around this building
                                                    if (!hasRoad && !hasStructure && RoadManager.IsRoad(x1, y1))
                                                    {
                                                        if (!buildingOnRoad ||
                                                            RoadPathFinder.HasPath(new Location(x1, y1),
                                                                                   new Location(city.X, city.Y),
                                                                                   city,
                                                                                   new Location(origX, origY)))
                                                            hasRoad = true;
                                                    }

                                                    return true;
                                                },
                                            null);

                if (!hasRoad)
                {
                    Global.World.UnlockRegion(x, y);
                    return Error.RoadNotAround;
                }
            }
            else
            {
                // Cant build on road if this building doesnt require roads
                if (buildingOnRoad)
                {
                    Global.World.UnlockRegion(x, y);
                    return Error.RoadDestroyUniquePath;
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
                return Error.MapFull;
            }

            InitFactory.InitGameObject(InitCondition.OnInit, structure, structure.Type, structure.Stats.Base.Lvl);
            structure.EndUpdate();

            structureId = structure.ObjectId;

            // add to queue for completion
            endTime = DateTime.UtcNow.AddSeconds(CalculateTime(Formula.BuildTime(StructureFactory.GetTime(type, level), city, city.Technologies)));
            BeginTime = DateTime.UtcNow;

            city.Worker.References.Add(structure, this);

            Global.World.UnlockRegion(x, y);

            return Error.Ok;
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
                    StateChange(ActionState.Failed);
                    return;
                }

                city.Worker.References.Remove(structure, this);
                structure.BeginUpdate();
                structure.Technologies.Parent = structure.City.Technologies;
                StructureFactory.GetUpgradedStructure(structure, structure.Type, level);
                InitFactory.InitGameObject(InitCondition.OnInit, structure, structure.Type, structure.Lvl);

                structure.EndUpdate();

                StateChange(ActionState.Completed);
            }
        }

        public override Error Validate(string[] parms)
        {
            City city;

            if (!Global.World.TryGetObjects(cityId, out city))
                return Error.ObjectNotFound;

            if(parms[2] != string.Empty && byte.Parse(parms[2])!=level)            
                return Error.ActionInvalid;

            if (parms[2] == string.Empty && level != 1)
                return Error.ActionInvalid;

            if (ushort.Parse(parms[0]) == type)
            {
                if (parms[1].Length == 0)
                {
                    ushort tileType = Global.World.GetTileType(x, y);
                    if (RoadManager.IsRoad(x, y) || ObjectTypeFactory.IsTileType("TileBuildable", tileType))
                        return Error.Ok;

                    return Error.TileMismatch;
                }
                else
                {
                    string[] tokens = parms[1].Split('|');
                    ushort tileType = Global.World.GetTileType(x, y);
                    foreach (var str in tokens)
                    {
                        if (ObjectTypeFactory.IsTileType(str, tileType))
                            return Error.Ok;
                    }
                    return Error.TileMismatch;
                }
            }
            return Error.ActionInvalid;
        }

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
                    city.Resource.Add(Formula.GetActionCancelResource(BeginTime, cost));
                    city.EndUpdate();
                }

                StateChange(ActionState.Failed);
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
                        XmlSerializer.Serialize(new[]
                                                {
                                                        new XmlKvPair("type", type), new XmlKvPair("x", x), new XmlKvPair("y", y), new XmlKvPair("city_id", cityId),
                                                        new XmlKvPair("structure_id", structureId), new XmlKvPair("wood", cost.Wood), new XmlKvPair("crop", cost.Crop),
                                                        new XmlKvPair("iron", cost.Iron), new XmlKvPair("gold", cost.Gold), new XmlKvPair("labor", cost.Labor), new XmlKvPair("level", level),
                                                });
            }
        }

        #endregion
    }
}