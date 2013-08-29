using System.Linq;
using Game.Data;
using Game.Data.BarbarianTribe;
using Game.Data.Troop;
using Game.Logic.Actions;
using Game.Map;
using Game.Map.LocationStrategies;
using Game.Setup;

namespace Game.Logic.Procedures
{
    partial class Procedure
    {
        /// <summary>
        ///     Creates a city under the specified player with initial troop and main building
        /// </summary>
        public virtual void CreateCity(ICityFactory cityFactory, IPlayer player, string cityName, Position cityPosition, IBarbarianTribeManager barbarianTribeManager, out ICity city)
        {
            city = cityFactory.CreateCity(world.Cities.GetNextCityId(),
                            player,
                            cityName,
                            cityPosition,
                            formula.GetInitialCityResources(),
                            formula.GetInitialCityRadius(),
                            formula.GetInitialAp());

            var mainBuildingPosition = cityPosition.Left();
            IStructure mainBuilding = city.CreateStructure(2000, 1, mainBuildingPosition.X, mainBuildingPosition.Y);

            player.Add(city);

            world.Cities.Add(city);

            mainBuilding.BeginUpdate();
            world.Regions.Add(mainBuilding);
            mainBuilding.EndUpdate();

            var defaultTroop = city.CreateTroopStub();
            defaultTroop.BeginUpdate();
            defaultTroop.AddFormation(FormationType.Normal);
            defaultTroop.AddFormation(FormationType.Garrison);
            defaultTroop.AddFormation(FormationType.InBattle);
            defaultTroop.EndUpdate();

            if (player.GetCityCount() == 1)
            {
                barbarianTribeManager.CreateBarbarianTribeNear(1, 1, city.PrimaryPosition.X, city.PrimaryPosition.Y, 10);
            }
        }

        public virtual void InitCity(ICity city, InitFactory initFactory, IActionFactory actionFactory)
        {
            var mainBuilding = city.MainBuilding;
            initFactory.InitGameObject(InitCondition.OnInit, mainBuilding, mainBuilding.Type, mainBuilding.Stats.Base.Lvl);
            city.Worker.DoPassive(city, actionFactory.CreateCityPassiveAction(city.Id), false);
        }
    }
}
