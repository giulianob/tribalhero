using Game.Data;
using Game.Data.Troop;

namespace Game.Logic.Procedures
{
    partial class Procedure
    {
        /// <summary>
        ///     Creates a city under the specified player with initial troop and main building
        /// </summary>
        public virtual bool CreateCity(ICityFactory cityFactory, IPlayer player, string cityName, out ICity city)
        {
            city = cityFactory.CreateCity(world.Cities.GetNextCityId(),
                            player,
                            cityName,
                            formula.GetInitialCityResources(),
                            formula.GetInitialCityRadius(),
                            formula.GetInitialAp());

            IStructure mainBuilding;

            if (!Randomizer.MainBuilding(city, out mainBuilding, formula.GetInitialCityRadius(), 1))
            {
                city = null;
                world.Players.Remove(player.PlayerId);
                dbPersistance.Rollback();                
                return false;
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

            return true;
        }
    }
}