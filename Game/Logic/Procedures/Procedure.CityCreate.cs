using Game.Data;
using Game.Data.Troop;
using Game.Database;
using Game.Logic.Formulas;
using Game.Map;

namespace Game.Logic.Procedures
{
    partial class Procedure
    {
        /// <summary>
        ///     Creates a city under the specified player with initial troop and main building
        /// </summary>
        /// <param name="player"></param>
        /// <param name="cityName"></param>
        /// <param name="city"></param>
        /// <returns></returns>
        public virtual bool CreateCity(IPlayer player, string cityName, out ICity city)
        {
            city = null;
            IStructure mainBuilding;
            if (!Randomizer.MainBuilding(out mainBuilding, formula.GetInitialCityRadius(), 1))
            {
                world.Players.Remove(player.PlayerId);
                dbPersistance.Rollback();
                // If this happens I'll be a very happy game developer
                return false;
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

            return true;
        }
    }
}