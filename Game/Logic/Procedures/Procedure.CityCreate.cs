using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Game.Data;
using Game.Data.Troop;
using Game.Database;
using Game.Logic.Formulas;
using Game.Map;
using Game.Setup;
using Ninject;
using Persistance;

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
        public virtual bool CreateCity(IPlayer player, string cityName, out ICity city)
        {
            city = null;
            IStructure mainBuilding;
            if (!Randomizer.MainBuilding(out mainBuilding, Formula.Current.GetInitialCityRadius(), 1))
            {
                World.Current.Players.Remove(player.PlayerId);
                DbPersistance.Current.Rollback();
                // If this happens I'll be a very happy game developer
                return false;
            }

            city = new City(player, cityName, Formula.Current.GetInitialCityResources(), Formula.Current.GetInitialCityRadius(), mainBuilding);
            player.Add(city);

            World.Current.Add(city);
            mainBuilding.BeginUpdate();
            World.Current.Add(mainBuilding);
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
