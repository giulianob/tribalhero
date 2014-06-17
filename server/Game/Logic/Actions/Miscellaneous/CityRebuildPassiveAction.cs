using System;
using System.Collections.Generic;
using System.Linq;
using Game.Data;
using Game.Data.Troop;
using Game.Logic.Procedures;
using Game.Map;
using Game.Module.Remover;
using Game.Setup;
using Game.Util;
using Game.Util.Locking;

namespace Game.Logic.Actions
{
    public class CityRebuildPassiveAction : ScheduledPassiveAction
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


        public CityRebuildPassiveAction(IActionFactory actionFactory,
                                        ILocker locker,
                                        CallbackProcedure callbackProcedure,
                                        IStructureCsvFactory structureCsvFactory,
                                        CityProcedure cityProcedure,
                                        IWorld world)
        {
            this.actionFactory = actionFactory;
            this.locker = locker;
            this.callbackProcedure = callbackProcedure;
            this.structureCsvFactory = structureCsvFactory;
            this.cityProcedure = cityProcedure;
            this.world = world;
        }

        public CityRebuildPassiveAction(uint id,
                                        Resource resource,
                                        int structureUpgrades,
                                        int technologyUpgrades,
                                        IActionFactory actionFactory,
                                        ILocker locker,
                                        CallbackProcedure callbackProcedure,
                                        IStructureCsvFactory structureCsvFactory,
                                        CityProcedure cityProcedure,
                                        IWorld world)
                : this(actionFactory, locker, callbackProcedure, structureCsvFactory, cityProcedure, world)
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
                return ActionType.CityRebuildPassive;
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

        public override Error Validate(string[] parms)
        {
            return Error.Ok;
        }

        public override Error Execute()
        {
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
                var position = newCity.MainBuilding.PrimaryPosition.Left();

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
