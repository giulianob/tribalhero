using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Game.Data;
using Game.Data.Troop;
using Game.Logic.Formulas;
using Game.Setup;

namespace Game.Logic.Procedures
{
    partial class Procedure
    {
        /// <summary>
        /// Creates a city under the specified player with initial troop and main building
        /// </summary>
        /// <param name="player"></param>
        /// <param name="cityName"></param>
        /// <param name="city"></param>
        /// <returns></returns>
        public static bool CreateCity(Player player, string cityName, out City city)
        {
            city = null;
            Structure mainBuilding;
            if (!Randomizer.MainBuilding(out mainBuilding, Formula.GetInitialCityRadius(), 1))
            {
                Global.World.Players.Remove(player.PlayerId);
                Global.DbManager.Rollback();
                // If this happens I'll be a very happy game developer
                return false;
            }

            city = new City(player, cityName, Formula.GetInitialCityResources(), Formula.GetInitialCityRadius(), mainBuilding);
            player.Add(city);

            Global.World.Add(city);
            mainBuilding.BeginUpdate();
            Global.World.Add(mainBuilding);
            mainBuilding.EndUpdate();

            var defaultTroop = new TroopStub();
            defaultTroop.BeginUpdate();
            defaultTroop.AddFormation(FormationType.Normal);
            defaultTroop.AddFormation(FormationType.Garrison);
            defaultTroop.AddFormation(FormationType.InBattle);
            city.Troops.Add(defaultTroop);
            defaultTroop.EndUpdate();

            return true;
        }
    }
}
