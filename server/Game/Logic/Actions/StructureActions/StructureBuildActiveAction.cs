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
using JetBrains.Annotations;

#endregion

namespace Game.Logic.Actions
{
    public class StructureBuildActiveAction : ScheduledActiveAction
    {
        private uint cityId;

        private readonly ILocker concurrency;

        private readonly Formula formula;

        private readonly IObjectTypeFactory objectTypeFactory;

        private readonly Procedure procedure;

        private readonly IRoadPathFinder roadPathFinder;

        private readonly ITileLocator tileLocator;

        private readonly IRequirementCsvFactory requirementCsvFactory;

        private readonly IStructureCsvFactory structureCsvFactory;

        private readonly CallbackProcedure callbackProcedure;

        private readonly InstantProcedure instantProcedure;

        private ushort type;

        private readonly IWorld world;

        private byte level;

        public uint X { get; private set; }

        public uint Y { get; private set; }

        private Resource cost;

        private uint structureId;

        private string tileRequirement;

        public StructureBuildActiveAction(IObjectTypeFactory objectTypeFactory,
                                          IWorld world,
                                          Formula formula,
                                          IRequirementCsvFactory requirementCsvFactory,
                                          IStructureCsvFactory structureCsvFactory,
                                          ILocker concurrency,
                                          Procedure procedure,
                                          IRoadPathFinder roadPathFinder,
                                          ITileLocator tileLocator, 
                                          CallbackProcedure callbackProcedure,
                                          InstantProcedure instantProcedure)  
        {
            this.objectTypeFactory = objectTypeFactory;
            this.world = world;
            this.formula = formula;
            this.requirementCsvFactory = requirementCsvFactory;
            this.structureCsvFactory = structureCsvFactory;
            this.concurrency = concurrency;
            this.procedure = procedure;
            this.roadPathFinder = roadPathFinder;
            this.tileLocator = tileLocator;
            this.callbackProcedure = callbackProcedure;
            this.instantProcedure = instantProcedure;
        }

        public StructureBuildActiveAction(uint cityId,
                                          ushort type,
                                          uint x,
                                          uint y,
                                          byte level,
                                          IObjectTypeFactory objectTypeFactory,
                                          IWorld world,
                                          Formula formula,
                                          IRequirementCsvFactory requirementCsvFactory,
                                          IStructureCsvFactory structureCsvFactory,
                                          ILocker concurrency,
                                          Procedure procedure,
                                          IRoadPathFinder roadPathFinder,
                                          ITileLocator tileLocator, 
                                          CallbackProcedure callbackProcedure,
                                          InstantProcedure instantProcedure)  
            : this(objectTypeFactory, world, formula, requirementCsvFactory, structureCsvFactory, concurrency, procedure, roadPathFinder, tileLocator, callbackProcedure, instantProcedure)
        {
            this.cityId = cityId;
            this.type = type;
            this.X = x;
            this.Y = y;
            this.level = level;
        }

        public override void LoadProperties(IDictionary<string, string> properties)
        {
            cityId = uint.Parse(properties["city_id"]);
            structureId = uint.Parse(properties["structure_id"]);
            X = uint.Parse(properties["x"]);
            Y = uint.Parse(properties["y"]);
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

            var maxConcurrentUpgradesResult = formula.CityMaxConcurrentBuildActions(type, ActionId, city, objectTypeFactory);

            if (maxConcurrentUpgradesResult != Error.Ok)
            {
                return maxConcurrentUpgradesResult;
            }
            
            var structureBaseStats = structureCsvFactory.GetBaseStats(type, level);

            var lockedRegions = world.Regions.LockRegions(X, Y, structureBaseStats.Size);

            foreach (var position in tileLocator.ForeachMultitile(X, Y, structureBaseStats.Size))
            {
                var tileType = world.Regions.GetTileType(position.X, position.Y);
                // tile requirement
                if (!string.IsNullOrEmpty(tileRequirement) && !objectTypeFactory.IsTileType(tileRequirement, tileType))
                {
                    world.Regions.UnlockRegions(lockedRegions);
                    return Error.TileMismatch;                    
                }

                // Only allow buildings that require resource tile to built on them
                if (tileRequirement != "TileResource" && objectTypeFactory.IsTileType("TileResource", tileType))
                {
                    world.Regions.UnlockRegions(lockedRegions);
                    return Error.TileMismatch;
                }

                // dont allow building on edge of world
                if (!world.Regions.IsValidXandY(position.X, position.Y))
                {
                    world.Regions.UnlockRegions(lockedRegions);
                    return Error.ActionInvalid;
                }

                // radius requirements
                if (tileLocator.TileDistance(city.PrimaryPosition, 1, position, 1) >= city.Radius)
                {
                    world.Regions.UnlockRegions(lockedRegions);
                    return Error.NotWithinWalls;
                }

                // check if tile is occupied
                if (world.Regions.GetObjectsInTile(position.X, position.Y).Any(obj => obj is IStructure))
                {
                    world.Regions.UnlockRegions(lockedRegions);
                    return Error.StructureExists;
                }
            }

            // cost requirement
            cost = formula.StructureCost(city, structureBaseStats);
            if (!city.Resource.HasEnough(cost))
            {
                world.Regions.UnlockRegions(lockedRegions);
                return Error.ResourceNotEnough;
            }

            // layout requirement
            if (!requirementCsvFactory.GetLayoutRequirement(type, level).Validate(WorkerObject as IStructure, type, X, Y, structureBaseStats.Size))
            {
                world.Regions.UnlockRegions(lockedRegions);
                return Error.LayoutNotFullfilled;
            }

            // check for road requirements       
            var requiresRoad = !objectTypeFactory.IsObjectType("NoRoadRequired", type);
            var canBuild = roadPathFinder.CanBuild(new Position(X, Y), structureBaseStats.Size, city, requiresRoad);
            if (canBuild != Error.Ok)
            {
                world.Regions.UnlockRegions(lockedRegions);
                return canBuild;
            }

            // add structure to the map                    
            IStructure structure = city.CreateStructure(type, 0, X, Y);
            
            city.BeginUpdate();
            city.Resource.Subtract(cost);
            city.EndUpdate();

            structure.BeginUpdate();

            if (!world.Regions.Add(structure))
            {
                city.ScheduleRemove(structure, false);
                city.BeginUpdate();
                city.Resource.Add(cost);
                city.EndUpdate();
                structure.EndUpdate();

                world.Regions.UnlockRegions(lockedRegions);
                return Error.MapFull;
            }
            
            structure.EndUpdate();

            callbackProcedure.OnStructureUpgrade(structure);

            structureId = structure.ObjectId;

            // add to queue for completion
            if (instantProcedure.BuildNext(city, structure))
            {
                endTime = beginTime = SystemClock.Now;
            }
            else
            {
                var buildTime = formula.BuildTime(structureBaseStats.BuildTime, city, city.Technologies);
                endTime = SystemClock.Now.AddSeconds(CalculateTime(buildTime));
                BeginTime = SystemClock.Now;
            }
            city.References.Add(structure, this);

            world.Regions.UnlockRegions(lockedRegions);
            return Error.Ok;
        }       

        public override void Callback(object custom)
        {
            ICity city;
            concurrency.Lock(cityId, out city).Do(() =>
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

                city.BeginUpdate();
                structure.BeginUpdate();

                structure.Technologies.Parent = structure.City.Technologies;
                structureCsvFactory.GetUpgradedStructure(structure, structure.Type, level);                
                
                procedure.OnStructureUpgradeDowngrade(structure);
                
                structure.EndUpdate();
                city.EndUpdate();
                
                callbackProcedure.OnStructureUpgrade(structure);

                StateChange(ActionState.Completed);
            });
        }

        public override Error Validate(string[] parms)
        {
            ICity city;

            if (!world.TryGetObjects(cityId, out city))
            {
                return Error.ObjectNotFound;
            }

            // Type
            if (ushort.Parse(parms[0]) != type)
            {
                return Error.ActionInvalid;
            }

            // Level
            if (parms[2] != string.Empty && byte.Parse(parms[2]) != level)
            {
                return Error.ActionInvalid;
            }

            if (parms[2] == string.Empty && level != 1)
            {
                return Error.ActionInvalid;
            }
            
            // Tile Requirement            
            tileRequirement = parms[1];

            return Error.Ok;
        }

        private void InterruptCatchAll(bool wasKilled)
        {
            ICity city;
            concurrency.Lock(cityId, out city).Do(() =>
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
            });
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
                                new XmlKvPair("type", type), new XmlKvPair("x", X), new XmlKvPair("y", Y),
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