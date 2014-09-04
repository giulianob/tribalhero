using System;
using System.Collections.Generic;
using System.Linq;
using Game.Battle;
using Game.Data;
using Game.Logic.Procedures;
using Game.Map;
using Game.Setup;
using Game.Util;
using Game.Util.Locking;
using Persistance;

namespace Game.Logic.Actions
{
    public class CityMovePassiveAction : ScheduledPassiveAction
    {
        private uint cityId;

        private Resource resource;

        private int structureUpgrades;

        private int technologyUpgrades;

        private readonly IActionFactory actionFactory;
        private readonly ILocker locker;
        private readonly CallbackProcedure callbackProcedure;
        private readonly IStructureCsvFactory structureCsvFactory;
        private readonly CityProcedure cityProcedure;

        private readonly IWorld world;

        private readonly TechnologyFactory technologyFactory;

        private readonly ITileLocator tileLocator;

        private readonly IObjectTypeFactory objectTypeFactory;

        private readonly IDbManager dbManager;

        public CityMovePassiveAction(IActionFactory actionFactory,
                                        ILocker locker,
                                        CallbackProcedure callbackProcedure,
                                        IStructureCsvFactory structureCsvFactory,
                                        CityProcedure cityProcedure,
                                        IWorld world,
                                        TechnologyFactory technologyFactory,
                                        ITileLocator tileLocator,
                                        IObjectTypeFactory objectTypeFactory,
                                        IDbManager dbManager)
        {
            this.actionFactory = actionFactory;
            this.locker = locker;
            this.callbackProcedure = callbackProcedure;
            this.structureCsvFactory = structureCsvFactory;
            this.cityProcedure = cityProcedure;
            this.world = world;
            this.technologyFactory = technologyFactory;
            this.tileLocator = tileLocator;
            this.objectTypeFactory = objectTypeFactory;
            this.dbManager = dbManager;
        }

        public CityMovePassiveAction(uint id,
                                        Resource resource,
                                        int structureUpgrades,
                                        int technologyUpgrades,
                                        IActionFactory actionFactory,
                                        ILocker locker,
                                        CallbackProcedure callbackProcedure,
                                        IStructureCsvFactory structureCsvFactory,
                                        CityProcedure cityProcedure,
                                        IWorld world,
                                        TechnologyFactory technologyFactory,
                                        ITileLocator tileLocator,
                                        IObjectTypeFactory objectTypeFactory,
                                        IDbManager dbManager)
            : this(actionFactory, locker, callbackProcedure, structureCsvFactory, cityProcedure, world, technologyFactory, tileLocator, objectTypeFactory, dbManager)
        {
            this.cityId = id;
            this.resource = resource;
            this.structureUpgrades = structureUpgrades;
            this.technologyUpgrades = technologyUpgrades;
        }

        public override void LoadProperties(IDictionary<string, string> properties)
        {
            cityId = uint.Parse(properties["city_id"]);
            resource = new Resource(int.Parse(properties["crop"]),
                                    int.Parse(properties["gold"]),
                                    int.Parse(properties["iron"]),
                                    int.Parse(properties["wood"]));
            structureUpgrades = int.Parse(properties["structure_upgrades"]);
            technologyUpgrades = int.Parse(properties["technology_upgrades"]);
        }

        #region Overrides of GameAction

        public override ActionType Type
        {
            get
            {
                return ActionType.CityMovePassive;
            }
        }

        public override string Properties
        {
            get
            {
                return XmlSerializer.Serialize(new[]
                {
                    new XmlKvPair("city_id", cityId),
                    new XmlKvPair("crop", resource.Crop),
                    new XmlKvPair("gold", resource.Gold),
                    new XmlKvPair("iron", resource.Iron),
                    new XmlKvPair("wood", resource.Wood),
                    new XmlKvPair("structure_upgrades", structureUpgrades),
                    new XmlKvPair("technology_upgrades", technologyUpgrades),
                });
            }
        }

        public override Error SystemCancelable
        {
            get
            {
                return Error.UncancelableCityMove;
            }
        }

        public override Error Validate(string[] parms)
        {
            return Error.Ok;
        }

        public override Error Execute()
        {
            ICity city;
            if (!world.TryGetObjects(cityId, out city))
                return Error.CityNotFound;
            
            city.Owner.LastMoved = SystemClock.Now;
            dbManager.Save(city.Owner);
            
            beginTime = SystemClock.Now;
            endTime = SystemClock.Now.AddSeconds(10);
            return Error.Ok;
        }

        public override void UserCancelled()
        {
        }

        public override void WorkerRemoved(bool wasKilled)
        {
        }

        #endregion

        public override void Callback(object custom)
        {
            // upgrade building
            ICity newCity;

            locker.Lock(cityId, out newCity).Do(() =>
            {
                if (!IsValid())
                {
                    return;
                }

                if (newCity == null)
                {
                    StateChange(ActionState.Failed);
                    return;
                }

                // complete mainbuilding
                var structure = newCity.MainBuilding;
                structure.BeginUpdate();
                structureCsvFactory.GetUpgradedStructure(structure, structure.Type, (byte)(structure.Lvl + 1));
                structure.EndUpdate();

                // kick off citypassive action
                cityProcedure.InitCity(newCity, callbackProcedure, actionFactory);

                // build cranny
                
                // find resource tile
                var positions =
                        tileLocator.ForeachTile(newCity.MainBuilding.PrimaryPosition.X, newCity.MainBuilding.PrimaryPosition.Y, newCity.Radius)
                                   .Where(p => objectTypeFactory.IsTileType("TileResource", world.Regions.GetTileType(p.X, p.Y)))
                                   .ToList();
                var position = positions.Shuffle((int)cityId).First();
                

                // build object

                // add structure to the map                    
                IStructure cranny = newCity.CreateStructure(3013, 1, position.X, position.Y);

                cranny.BeginUpdate();
                cranny.Properties.Add("Crop", resource.Crop);
                cranny.Properties.Add("Wood", resource.Wood);
                cranny.Properties.Add("Gold", resource.Gold);
                cranny.Properties.Add("Iron", resource.Iron);
                cranny.Properties.Add("Structure Upgrades", structureUpgrades);
                cranny.Properties.Add("Technology Upgrades", technologyUpgrades);

                cranny.Technologies.BeginUpdate();
                if (structureUpgrades > 0)
                {
                    cranny.Technologies.Add(new Technology(technologyFactory.GetTechnologyBase(30131, 1)), true);
                }

                if (technologyUpgrades > 0)
                {
                    cranny.Technologies.Add(new Technology(technologyFactory.GetTechnologyBase(30132, 1)), true);
                }
                cranny.Technologies.EndUpdate();

                if (!world.Regions.Add(cranny))
                {
                    throw new Exception("How can this happen??!?");
                }
                cranny.EndUpdate();
                callbackProcedure.OnStructureUpgrade(cranny);

                // add resource to building

                StateChange(ActionState.Completed);
            });
        }
    }
}
