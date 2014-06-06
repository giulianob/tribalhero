using Game.Data;
using Game.Data.BarbarianTribe;
using Game.Data.Troop;
using Game.Logic.Actions;
using Game.Map;

namespace Game.Logic.Procedures
{
    partial class Procedure
    {
        /// <summary>
        ///     Creates a city under the specified player with initial troop and main building
        /// </summary>
        public virtual void CreateCity(ICityFactory cityFactory, IPlayer player, string cityName, byte level, Position cityPosition, IBarbarianTribeManager barbarianTribeManager, out ICity city)
        {
            city = cityFactory.CreateCity(world.Cities.GetNextCityId(),
                            player,
                            cityName,
                            cityPosition,
                            formula.GetInitialCityResources(),
                            formula.GetInitialCityRadius(),
                            formula.GetInitialAp(),
                            Theme.DEFAULT_THEME_ID,
                            Theme.DEFAULT_THEME_ID);

            var mainBuildingPosition = cityPosition.Left();
            IStructure mainBuilding = city.CreateStructure(2000, level, mainBuildingPosition.X, mainBuildingPosition.Y);

            player.Add(city);

            world.Cities.Add(city);

            city.BeginUpdate();

            mainBuilding.BeginUpdate();
            world.Regions.Add(mainBuilding);
            mainBuilding.EndUpdate();

            var defaultTroop = city.CreateTroopStub();
            defaultTroop.BeginUpdate();
            defaultTroop.AddFormation(FormationType.Normal);
            defaultTroop.AddFormation(FormationType.Garrison);
            defaultTroop.AddFormation(FormationType.InBattle);
            defaultTroop.EndUpdate();
            
            RecalculateCityResourceRates(city);
            SetResourceCap(city);

            city.EndUpdate();

            if (player.GetCityCount() == 1)
            {
                barbarianTribeManager.CreateBarbarianTribeNear(1, 10, city.PrimaryPosition.X, city.PrimaryPosition.Y, 10);
            }
        }

        public virtual void InitCity(ICity city, CallbackProcedure callbackProcedure, IActionFactory actionFactory)
        {
            var mainBuilding = city.MainBuilding;
            callbackProcedure.OnStructureConvert(mainBuilding);       
            city.Worker.DoPassive(city, actionFactory.CreateCityPassiveAction(city.Id), false);
        }
    }
}
