using Game.Data;
using Game.Data.Troop;
using Game.Map.LocationStrategies;
using Game.Setup;

namespace Game.Logic.Procedures
{
    partial class Procedure
    {
        /// <summary>
        ///     Creates a city under the specified player with initial troop and main building
        /// </summary>
        public virtual Error CreateCity(ICityFactory cityFactory, IPlayer player, string cityName, ILocationStrategy strategy, out ICity city)
        {
            city = cityFactory.CreateCity(world.Cities.GetNextCityId(),
                            player,
                            cityName,
                            formula.GetInitialCityResources(),
                            formula.GetInitialCityRadius(),
                            formula.GetInitialAp());

            IStructure mainBuilding;

            Error error = Randomizer.MainBuilding(city, strategy, 1, out mainBuilding);
            if (error != Error.Ok)
            {
                world.Players.Remove(player.PlayerId);
                dbPersistance.Rollback();                

                return error;
            }

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

            return Error.Ok;
        }
    }
}
