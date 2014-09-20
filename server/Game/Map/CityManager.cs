using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Text.RegularExpressions;
using Game.Data;
using Game.Data.Events;
using Game.Logic.Formulas;
using Game.Logic.Procedures;
using Game.Setup;
using Game.Setup.DependencyInjection;
using Game.Util;
using Persistance;

namespace Game.Map
{
    public class CityManager : ICityManager
    {
        private readonly ConcurrentDictionary<uint, ICity> cities = new ConcurrentDictionary<uint, ICity>();

        private readonly LargeIdGenerator cityIdGen = new LargeIdGenerator(Config.city_id_max, Config.city_id_min);

        private readonly IDbManager dbManager;

        private readonly IRegionManager regionManager;

        public event EventHandler<NewCityEventArgs> CityAdded = (sender, args) => { };
        public event EventHandler<EventArgs> CityRemoved = (sender, args) => { };

        public CityManager(IDbManager dbManager, IRegionManager regionManager)
        {
            this.dbManager = dbManager;
            this.regionManager = regionManager;
        }

        public int Count
        {
            get
            {
                return cities.Count;
            }
        }

        public bool TryGetCity(uint cityId, out ICity city)
        {
            return cities.TryGetValue(cityId, out city);
        }

        public void AfterDbLoaded(Procedure procedure)
        {
            IEnumerator<ICity> iter = cities.Values.GetEnumerator();
            while (iter.MoveNext())
            {
                // Resave city to update times
                dbManager.Save(iter.Current);

                // Recalc city resource rate
                procedure.RecalculateCityResourceRates(iter.Current);

                //Set resource cap
                procedure.SetResourceCap(iter.Current);

                //Set up the city region (for minimap)
                MiniMapRegion region;
                if (regionManager.MiniMapRegions.TryGetMiniMapRegion(iter.Current.PrimaryPosition.X, iter.Current.PrimaryPosition.Y, out region))
                {
                    region.Add(iter.Current);
                }
            }
        }

        public void Remove(ICity city)
        {
            lock (cities)
            {
                if (!cities.TryRemove(city.Id, out city))
                {
                    return;
                }

                city.BeginUpdate();
                dbManager.DeleteDependencies(city);
                city.Deleted = City.DeletedState.Deleted;
                city.EndUpdate();                

                DeregisterEvents(city);
            }

            CityRemoved(city, new EventArgs());
        }

        public uint GetNextCityId()
        {
            return cityIdGen.GetNext();
        }

        public void Add(ICity city)
        {
            lock (cities)
            {
                cities[city.Id] = city;

                //Initial save of troops
                // TODO: Remove this when troops are being created from the city and saved immediatelly
                foreach (var stub in city.Troops)
                {
                    dbManager.Save(stub);
                }

                MiniMapRegion region;
                if (regionManager.MiniMapRegions.TryGetMiniMapRegion(city.PrimaryPosition.X, city.PrimaryPosition.Y, out region))
                {
                    region.Add(city);
                }

                RegisterEvents(city);
            }

            CityAdded(city, new NewCityEventArgs(true));
        }

        public void DbLoaderAdd(ICity city)
        {
            lock (cities)
            {
                cityIdGen.Set(city.Id);

                if (city.Deleted != City.DeletedState.Deleted)
                {
                    if (!cities.TryAdd(city.Id, city))
                    {
                        return;
                    }

                    RegisterEvents(city);
                }
            }

            CityAdded(city, new NewCityEventArgs(false));
        }

        private void RegisterEvents(ICity city)
        {
            city.PropertyChanged += CityPropertyChanged;
        }

        public bool FindCityId(string name, out uint cityId)
        {
            cityId = UInt16.MaxValue;

            var query = String.Format("SELECT `id` FROM `{0}` WHERE name = @name LIMIT 1", City.DB_TABLE);
            using (DbDataReader reader = dbManager.ReaderQuery(query, new[] {new DbColumn("name", name, DbType.String)}))
            {
                if (!reader.HasRows)
                {
                    return false;
                }
                reader.Read();
                cityId = (uint)reader[0];
                return true;
            }
        }

        private void DeregisterEvents(ICity city)
        {
            city.PropertyChanged -= CityPropertyChanged;
        }

        private void CityPropertyChanged(ICity city, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "Value" && Global.Current.FireEvents)
            {
                MiniMapRegion miniMapRegion;
                if (regionManager.MiniMapRegions.TryGetMiniMapRegion(city.PrimaryPosition.X, city.PrimaryPosition.Y, out miniMapRegion))
                {
                    miniMapRegion.MarkAsDirty();
                }
            }
        }

        public IEnumerable<ICity> AllCities()
        {
            lock (cities)
            {
                return cities.Values.AsEnumerable();
            }
        }

        public static bool IsNameValid(string cityName)
        {
            return cityName != String.Empty && cityName.Length >= 3 && cityName.Length <= 16 &&
                   Regex.IsMatch(cityName, Global.ALPHANUMERIC_NAME, RegexOptions.IgnoreCase);
        }
    }
}