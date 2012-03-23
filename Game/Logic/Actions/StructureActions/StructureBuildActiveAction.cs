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
using Ninject;

#endregion

namespace Game.Logic.Actions
{
    public class StructureBuildActiveAction : ScheduledActiveAction
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
            if (!World.Current.TryGetObjects(cityId, out city))
                return Error.ObjectNotFound;

            int maxConcurrentUpgrades = Formula.Current.ConcurrentBuildUpgrades(((IStructure)city[1]).Lvl);

            if (!Ioc.Kernel.Get<ObjectTypeFactory>().IsStructureType("UnlimitedBuilding", type) &&
                    city.Worker.ActiveActions.Values.Count(
                                                           action =>
                                                           action.ActionId != ActionId &&
                                                           (action.Type == ActionType.StructureUpgradeActive ||
                                                            (action.Type == ActionType.StructureBuildActive &&
                                                             !Ioc.Kernel.Get<ObjectTypeFactory>().IsStructureType("UnlimitedBuilding",
                                                                                                ((StructureBuildActiveAction)action).BuildType)))) >= maxConcurrentUpgrades)
                return Error.ActionTotalMaxReached;

            if (!World.Current.IsValidXandY(x, y))
                return Error.ActionInvalid;

            World.Current.LockRegion(x, y);

            // cost requirement
            cost = Formula.Current.StructureCost(city, type, level);
            if (!city.Resource.HasEnough(cost))
            {
                World.Current.UnlockRegion(x, y);
                return Error.ResourceNotEnough;
            }

            // radius requirements
            if (SimpleGameObject.TileDistance(city.X, city.Y, x, y) >= city.Radius)
            {
                World.Current.UnlockRegion(x, y);
                return Error.LayoutNotFullfilled;
            }

            // layout requirement
            if (!Ioc.Kernel.Get<RequirementFactory>().GetLayoutRequirement(type, level).Validate(WorkerObject as IStructure, type, x, y))
            {
                World.Current.UnlockRegion(x, y);
                return Error.LayoutNotFullfilled;
            }

            // check if tile is occupied
            if (World.Current[x, y].Exists(obj => obj is IStructure))
            {
                World.Current.UnlockRegion(x, y);
                return Error.StructureExists;
            }

            // check for road requirements       
            bool roadRequired = !Ioc.Kernel.Get<ObjectTypeFactory>().IsStructureType("NoRoadRequired", type);
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

                        if (Ioc.Kernel.Get<ObjectTypeFactory>().IsStructureType("NoRoadRequired", str.Type))
                            continue;

                        if (!RoadPathFinder.HasPath(new Location(str.X, str.Y), new Location(city.X, city.Y), city, new Location(x, y)))
                        {
                            breaksRoad = true;
                            break;
                        }
                    }

                    if (breaksRoad)
                    {
                        World.Current.UnlockRegion(x, y);
                        return Error.RoadDestroyUniquePath;
                    }

                    // Make sure all neighboring roads have a diff path
                    bool allNeighborsHaveOtherPaths = true;
                    RadiusLocator.Current.ForeachObject(x,
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
                        World.Current.UnlockRegion(x, y);
                        return Error.RoadDestroyUniquePath;
                    }
                }

                bool hasRoad = false;

                RadiusLocator.Current.ForeachObject(x,
                                            y,
                                            1,
                                            false,
                                            delegate(uint origX, uint origY, uint x1, uint y1, object custom)
                                                {
                                                    if (SimpleGameObject.RadiusDistance(origX, origY, x1, y1) != 1)
                                                        return true;

                                                    var curStruct = (IStructure)World.Current[x1, y1].FirstOrDefault(obj => obj is IStructure);

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
                    World.Current.UnlockRegion(x, y);
                    return Error.RoadNotAround;
                }
            }
            else
            {
                // Cant build on road if this building doesnt require roads
                if (buildingOnRoad)
                {
                    World.Current.UnlockRegion(x, y);
                    return Error.RoadDestroyUniquePath;
                }
            }

            // add structure to the map                    
            IStructure structure = Ioc.Kernel.Get<StructureFactory>().GetNewStructure(type, 0);
            structure.X = x;
            structure.Y = y;

            structure.BeginUpdate();
            city.BeginUpdate();
            city.Resource.Subtract(cost);
            city.EndUpdate();

            city.Add(structure);

            if (!World.Current.Add(structure))
            {
                city.ScheduleRemove(structure, false);
                city.BeginUpdate();
                city.Resource.Add(cost);
                city.EndUpdate();

                World.Current.UnlockRegion(x, y);
                return Error.MapFull;
            }

            Ioc.Kernel.Get<InitFactory>().InitGameObject(InitCondition.OnInit, structure, structure.Type, structure.Stats.Base.Lvl);
            structure.EndUpdate();

            structureId = structure.ObjectId;

            // add to queue for completion
            endTime = DateTime.UtcNow.AddSeconds(CalculateTime(Formula.Current.BuildTime(Ioc.Kernel.Get<StructureFactory>().GetTime(type, level), city, city.Technologies)));
            BeginTime = DateTime.UtcNow;

            city.Worker.References.Add(structure, this);

            World.Current.UnlockRegion(x, y);

            return Error.Ok;
        }

        public override void Callback(object custom)
        {
            ICity city;
            using (Concurrency.Current.Lock(cityId, out city))
            {
                if (!IsValid())
                    return;

                IStructure structure;
                if (!city.TryGetStructure(structureId, out structure))
                {
                    StateChange(ActionState.Failed);
                    return;
                }

                city.Worker.References.Remove(structure, this);
                structure.BeginUpdate();
                structure.Technologies.Parent = structure.City.Technologies;
                Ioc.Kernel.Get<StructureFactory>().GetUpgradedStructure(structure, structure.Type, level);
                Ioc.Kernel.Get<InitFactory>().InitGameObject(InitCondition.OnInit, structure, structure.Type, structure.Lvl);

                structure.EndUpdate();
                city.BeginUpdate();
                Procedure.Current.OnStructureUpgradeDowngrade(structure);
                city.EndUpdate();
                StateChange(ActionState.Completed);
            }
        }

        public override Error Validate(string[] parms)
        {
            ICity city;

            if (!World.Current.TryGetObjects(cityId, out city))
                return Error.ObjectNotFound;

            if(parms[2] != string.Empty && byte.Parse(parms[2])!=level)            
                return Error.ActionInvalid;

            if (parms[2] == string.Empty && level != 1)
                return Error.ActionInvalid;

            if (ushort.Parse(parms[0]) == type)
            {
                if (parms[1].Length == 0)
                {
                    ushort tileType = World.Current.GetTileType(x, y);
                    if (RoadManager.IsRoad(x, y) || Ioc.Kernel.Get<ObjectTypeFactory>().IsTileType("TileBuildable", tileType))
                        return Error.Ok;

                    return Error.TileMismatch;
                }
                else
                {
                    string[] tokens = parms[1].Split('|');
                    ushort tileType = World.Current.GetTileType(x, y);
                    foreach (var str in tokens)
                    {
                        if (Ioc.Kernel.Get<ObjectTypeFactory>().IsTileType(str, tileType))
                            return Error.Ok;
                    }
                    return Error.TileMismatch;
                }
            }
            return Error.ActionInvalid;
        }

        private void InterruptCatchAll(bool wasKilled)
        {
            ICity city;
            using (Concurrency.Current.Lock(cityId, out city))
            {
                if (!IsValid())
                    return;

                IStructure structure;
                if (!city.TryGetStructure(structureId, out structure))
                    return;

                city.Worker.References.Remove(structure, this);

                structure.BeginUpdate();
                World.Current.Remove(structure);
                city.ScheduleRemove(structure, wasKilled);
                structure.EndUpdate();

                if (!wasKilled)
                {
                    city.BeginUpdate();
                    city.Resource.Add(Formula.Current.GetActionCancelResource(BeginTime, cost));
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