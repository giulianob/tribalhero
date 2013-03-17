#region

using System;
using System.Collections.Generic;
using System.Linq;
using Game.Data;
using Game.Logic.Formulas;
using Game.Logic.Procedures;
using Game.Map;
using Game.Setup;
using Game.Util;
using Game.Util.Locking;

#endregion

namespace Game.Logic.Actions
{
    public class StructureBuildActiveAction : ScheduledActiveAction
    {
        private readonly uint cityId;

        private readonly ILocker concurrency;

        private readonly Formula formula;

        private readonly InitFactory initFactory;

        private readonly byte level;

        private readonly ObjectTypeFactory objectTypeFactory;

        private readonly Procedure procedure;

        private readonly RadiusLocator radiusLocator;

        private readonly RequirementFactory requirementFactory;

        private readonly StructureCsvFactory structureCsvFactory;

        private readonly ushort type;

        private readonly IWorld world;

        private readonly uint x;

        private readonly uint y;

        private Resource cost;

        private uint structureId;

        public StructureBuildActiveAction(uint cityId,
                                          ushort type,
                                          uint x,
                                          uint y,
                                          byte level,
                                          ObjectTypeFactory objectTypeFactory,
                                          IWorld world,
                                          Formula formula,
                                          RequirementFactory requirementFactory,
                                          RadiusLocator radiusLocator,
                                          StructureCsvFactory structureCsvFactory,
                                          InitFactory initFactory,
                                          ILocker concurrency,
                                          Procedure procedure)
        {
            this.cityId = cityId;
            this.type = type;
            this.x = x;
            this.y = y;
            this.level = level;
            this.objectTypeFactory = objectTypeFactory;
            this.world = world;
            this.formula = formula;
            this.requirementFactory = requirementFactory;
            this.radiusLocator = radiusLocator;
            this.structureCsvFactory = structureCsvFactory;
            this.initFactory = initFactory;
            this.concurrency = concurrency;
            this.procedure = procedure;
        }

        public StructureBuildActiveAction(uint id,
                                          DateTime beginTime,
                                          DateTime nextTime,
                                          DateTime endTime,
                                          int workerType,
                                          byte workerIndex,
                                          ushort actionCount,
                                          Dictionary<string, string> properties,
                                          ObjectTypeFactory objectTypeFactory,
                                          IWorld world,
                                          Formula formula,
                                          RequirementFactory requirementFactory,
                                          RadiusLocator radiusLocator,
                                          StructureCsvFactory structureCsvFactory,
                                          InitFactory initFactory,
                                          ILocker concurrency,
                                          Procedure procedure)
                : base(id, beginTime, nextTime, endTime, workerType, workerIndex, actionCount)
        {
            this.objectTypeFactory = objectTypeFactory;
            this.world = world;
            this.formula = formula;
            this.requirementFactory = requirementFactory;
            this.radiusLocator = radiusLocator;
            this.structureCsvFactory = structureCsvFactory;
            this.initFactory = initFactory;
            this.concurrency = concurrency;
            this.procedure = procedure;
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
            level = properties.TryGetValue("level", out tmp) ? byte.Parse(tmp) : (byte)1;
        }

        public ushort BuildType
        {
            get
            {
                return type;
            }
        }

        public override ConcurrencyType ActionConcurrency
        {
            get
            {
                return ConcurrencyType.Normal;
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
            ICity city;
            if (!world.TryGetObjects(cityId, out city))
            {
                return Error.ObjectNotFound;
            }

            int maxConcurrentUpgrades = formula.ConcurrentBuildUpgrades(((IStructure)city[1]).Lvl);

            if (!objectTypeFactory.IsObjectType("UnlimitedBuilding", type) &&
                city.Worker.ActiveActions.Values.Count(
                                                       action =>
                                                       action.ActionId != ActionId &&
                                                       (action.Type == ActionType.StructureUpgradeActive ||
                                                        (action.Type == ActionType.StructureBuildActive &&
                                                         !objectTypeFactory.IsObjectType("UnlimitedBuilding",
                                                                                            ((StructureBuildActiveAction
                                                                                             )action).BuildType)))) >=
                maxConcurrentUpgrades)
            {
                return Error.ActionTotalMaxReached;
            }

            if (!world.Regions.IsValidXandY(x, y))
            {
                return Error.ActionInvalid;
            }

            world.Regions.LockRegion(x, y);

            // cost requirement
            cost = formula.StructureCost(city, type, level);
            if (!city.Resource.HasEnough(cost))
            {
                world.Regions.UnlockRegion(x, y);
                return Error.ResourceNotEnough;
            }

            // radius requirements
            if (SimpleGameObject.TileDistance(city.X, city.Y, x, y) >= city.Radius)
            {
                world.Regions.UnlockRegion(x, y);
                return Error.LayoutNotFullfilled;
            }

            // layout requirement
            if (!requirementFactory.GetLayoutRequirement(type, level).Validate(WorkerObject as IStructure, type, x, y))
            {
                world.Regions.UnlockRegion(x, y);
                return Error.LayoutNotFullfilled;
            }

            // check if tile is occupied
            if (world[x, y].Exists(obj => obj is IStructure))
            {
                world.Regions.UnlockRegion(x, y);
                return Error.StructureExists;
            }

            // check for road requirements       
            bool roadRequired = !objectTypeFactory.IsObjectType("NoRoadRequired", type);
            bool buildingOnRoad = world.Roads.IsRoad(x, y);

            if (roadRequired)
            {
                if (buildingOnRoad)
                {
                    bool breaksRoad = false;

                    foreach (var str in city)
                    {
                        if (str.IsMainBuilding)
                        {
                            continue;
                        }

                        if (objectTypeFactory.IsObjectType("NoRoadRequired", str.Type))
                        {
                            continue;
                        }

                        if (
                                !RoadPathFinder.HasPath(new Position(str.X, str.Y),
                                                        new Position(city.X, city.Y),
                                                        city,
                                                        new Position(x, y)))
                        {
                            breaksRoad = true;
                            break;
                        }
                    }

                    if (breaksRoad)
                    {
                        world.Regions.UnlockRegion(x, y);
                        return Error.RoadDestroyUniquePath;
                    }

                    // Make sure all neighboring roads have a diff path
                    bool allNeighborsHaveOtherPaths = true;
                    radiusLocator.ForeachObject(x,
                                                y,
                                                1,
                                                false,
                                                delegate(uint origX, uint origY, uint x1, uint y1, object custom)
                                                    {
                                                        if (SimpleGameObject.RadiusDistance(origX, origY, x1, y1) != 1)
                                                        {
                                                            return true;
                                                        }

                                                        if (city.X == x1 && city.Y == y1)
                                                        {
                                                            return true;
                                                        }

                                                        if (world.Roads.IsRoad(x1, y1))
                                                        {
                                                            if (
                                                                    !RoadPathFinder.HasPath(new Position(x1, y1),
                                                                                            new Position(city.X, city.Y),
                                                                                            city,
                                                                                            new Position(origX, origY)))
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
                        world.Regions.UnlockRegion(x, y);
                        return Error.RoadDestroyUniquePath;
                    }
                }

                bool hasRoad = false;

                radiusLocator.ForeachObject(x,
                                            y,
                                            1,
                                            false,
                                            delegate(uint origX, uint origY, uint x1, uint y1, object custom)
                                                {
                                                    if (SimpleGameObject.RadiusDistance(origX, origY, x1, y1) != 1)
                                                    {
                                                        return true;
                                                    }

                                                    var curStruct =
                                                            (IStructure)
                                                            world[x1, y1].FirstOrDefault(obj => obj is IStructure);

                                                    bool hasStructure = curStruct != null;

                                                    // Make sure we have a road around this building
                                                    if (!hasRoad && !hasStructure && world.Roads.IsRoad(x1, y1))
                                                    {
                                                        if (!buildingOnRoad ||
                                                            RoadPathFinder.HasPath(new Position(x1, y1),
                                                                                   new Position(city.X, city.Y),
                                                                                   city,
                                                                                   new Position(origX, origY)))
                                                        {
                                                            hasRoad = true;
                                                        }
                                                    }

                                                    return true;
                                                },
                                            null);

                if (!hasRoad)
                {
                    world.Regions.UnlockRegion(x, y);
                    return Error.RoadNotAround;
                }
            }
            else
            {
                // Cant build on road if this building doesnt require roads
                if (buildingOnRoad)
                {
                    world.Regions.UnlockRegion(x, y);
                    return Error.RoadDestroyUniquePath;
                }
            }

            // add structure to the map                    
            IStructure structure = city.CreateStructure(type, 0);
            structure.BeginUpdate();
            structure.X = x;
            structure.Y = y;

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

                world.Regions.UnlockRegion(x, y);
                return Error.MapFull;
            }

            initFactory.InitGameObject(InitCondition.OnInit, structure, structure.Type, structure.Stats.Base.Lvl);
            structure.EndUpdate();

            structureId = structure.ObjectId;

            // add to queue for completion
            endTime =
                    DateTime.UtcNow.AddSeconds(
                                               CalculateTime(formula.BuildTime(structureCsvFactory.GetTime(type, level),
                                                                               city,
                                                                               city.Technologies)));
            BeginTime = DateTime.UtcNow;

            city.References.Add(structure, this);

            world.Regions.UnlockRegion(x, y);

            return Error.Ok;
        }

        public override void Callback(object custom)
        {
            ICity city;
            using (concurrency.Lock(cityId, out city))
            {
                if (!IsValid())
                {
                    return;
                }

                IStructure structure;
                if (!city.TryGetStructure(structureId, out structure))
                {
                    StateChange(ActionState.Failed);
                    return;
                }

                city.References.Remove(structure, this);
                structure.BeginUpdate();
                structure.Technologies.Parent = structure.City.Technologies;
                structureCsvFactory.GetUpgradedStructure(structure, structure.Type, level);
                initFactory.InitGameObject(InitCondition.OnInit, structure, structure.Type, structure.Lvl);

                structure.EndUpdate();
                city.BeginUpdate();
                procedure.OnStructureUpgradeDowngrade(structure);
                city.EndUpdate();
                StateChange(ActionState.Completed);
            }
        }

        public override Error Validate(string[] parms)
        {
            ICity city;

            if (!world.TryGetObjects(cityId, out city))
            {
                return Error.ObjectNotFound;
            }

            if (parms[2] != string.Empty && byte.Parse(parms[2]) != level)
            {
                return Error.ActionInvalid;
            }

            if (parms[2] == string.Empty && level != 1)
            {
                return Error.ActionInvalid;
            }

            if (ushort.Parse(parms[0]) == type)
            {
                if (parms[1].Length == 0)
                {
                    ushort tileType = world.Regions.GetTileType(x, y);
                    if (world.Roads.IsRoad(x, y) || objectTypeFactory.IsTileType("TileBuildable", tileType))
                    {
                        return Error.Ok;
                    }

                    return Error.TileMismatch;
                }
                else
                {
                    string[] tokens = parms[1].Split('|');
                    ushort tileType = world.Regions.GetTileType(x, y);
                    foreach (var str in tokens)
                    {
                        if (objectTypeFactory.IsTileType(str, tileType))
                        {
                            return Error.Ok;
                        }
                    }
                    return Error.TileMismatch;
                }
            }
            return Error.ActionInvalid;
        }

        private void InterruptCatchAll(bool wasKilled)
        {
            ICity city;
            using (concurrency.Lock(cityId, out city))
            {
                if (!IsValid())
                {
                    return;
                }

                IStructure structure;
                if (!city.TryGetStructure(structureId, out structure))
                {
                    StateChange(ActionState.Failed);
                    return;
                }

                city.References.Remove(structure, this);

                structure.BeginUpdate();
                world.Regions.Remove(structure);
                city.ScheduleRemove(structure, wasKilled);
                structure.EndUpdate();

                if (!wasKilled)
                {
                    city.BeginUpdate();
                    city.Resource.Add(formula.GetActionCancelResource(BeginTime, cost));
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
                                new XmlKvPair("type", type), new XmlKvPair("x", x), new XmlKvPair("y", y),
                                new XmlKvPair("city_id", cityId), new XmlKvPair("structure_id", structureId),
                                new XmlKvPair("wood", cost.Wood), new XmlKvPair("crop", cost.Crop),
                                new XmlKvPair("iron", cost.Iron), new XmlKvPair("gold", cost.Gold),
                                new XmlKvPair("labor", cost.Labor), new XmlKvPair("level", level),
                        });
            }
        }

        #endregion
    }
}