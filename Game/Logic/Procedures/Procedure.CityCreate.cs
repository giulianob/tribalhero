using Game.Data;
using Game.Data.BarbarianTribe;
using Game.Data.Troop;
using Game.Database;
using Game.Logic.Formulas;
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
        public virtual Error CreateCity(IPlayer player, string cityName, ILocationStrategy strategy, IBarbarianTribeManager barbarianTribeManager, out ICity city)
        {
            city = null;
            IStructure mainBuilding;
            Error error = Randomizer.MainBuilding(out mainBuilding, strategy, 1);
            if (error != Error.Ok)
            {
                world.Players.Remove(player.PlayerId);
                dbPersistance.Rollback();
                // If this happens I'll be a very happy game developer
                return error;
            }

            city = new City(world.Cities.GetNextCityId(),
                            player,
                            cityName,
                            formula.GetInitialCityResources(),
                            formula.GetInitialCityRadius(),
                            mainBuilding,
                            formula.GetInitialAp());
            player.Add(city);

            world.Cities.Add(city);
            mainBuilding.BeginUpdate();
            world.Regions.Add(mainBuilding);
            mainBuilding.EndUpdate();

            var defaultTroop = city.Troops.Create();
            defaultTroop.BeginUpdate();
            defaultTroop.AddFormation(FormationType.Normal);
            defaultTroop.AddFormation(FormationType.Garrison);
            defaultTroop.AddFormation(FormationType.InBattle);
            defaultTroop.EndUpdate();

            barbarianTribeManager.CreateBarbarianTribeNear(1, 3, city.X, city.Y);

            return Error.Ok;
        }
    }
}
