using System.Linq;
using Game.Data;
using Game.Data.BarbarianTribe;
using Game.Data.Troop;
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
        public virtual Error CreateCity(ICityFactory cityFactory, IPlayer player, string cityName, ILocationStrategy strategy, IBarbarianTribeManager barbarianTribeManager, out ICity city)
        {
            city = cityFactory.CreateCity(world.Cities.GetNextCityId(),
                            player,
                            cityName,
                            formula.GetInitialCityResources(),
                            formula.GetInitialCityRadius(),
                            formula.GetInitialAp());

            Position position;
            var error = strategy.NextLocation(out position);
            if(error != Error.Ok)
            {
                world.Players.Remove(player.PlayerId);
                dbPersistance.Rollback();                
                return error;
            }
        
            IStructure mainBuilding = city.CreateStructure(2000, 1, position.X, position.Y);

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

            barbarianTribeManager.CreateBarbarianTribeNear(1, 1, city.X, city.Y);
            
            return Error.Ok;
        }
    }
}
