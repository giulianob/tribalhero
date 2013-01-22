﻿using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using Game.Data;
using Game.Database;
using Game.Logic.Procedures;
using Game.Util;
using Persistance;

namespace Game.Map
{
    public class CityManager : ICityManager
    {
        private readonly Dictionary<uint, ICity> cities = new Dictionary<uint, ICity>();

        private readonly LargeIdGenerator cityIdGen = new LargeIdGenerator(300000, 200000);

        private readonly IDbManager dbManager;

        private readonly IRegionManager regionManager;

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

                //Set resource cap
                procedure.SetResourceCap(iter.Current);

                //Set up the city region (for minimap)
                CityRegion region = regionManager.CityRegions.GetCityRegion(iter.Current.X, iter.Current.Y);
                if (region != null)
                {
                    region.Add(iter.Current);
                }
            }
        }

        public void Remove(ICity city)
        {
            lock (cities)
            {
                city.BeginUpdate();
                dbManager.DeleteDependencies(city);
                city.Deleted = City.DeletedState.Deleted;
                city.EndUpdate();

                cities.Remove(city.Id);
            }
        }

        public uint GetNextCityId()
        {
            return (uint)cityIdGen.GetNext();
        }

        public void Add(ICity city)
        {
            lock (cities)
            {
                cities[city.Id] = city;

                //Initial save of these objects
                dbManager.Save((IStructure)city[1]);
                foreach (var stub in city.Troops)
                {
                    dbManager.Save(stub);
                }

                CityRegion region = regionManager.CityRegions.GetCityRegion(city.X, city.Y);
                if (region != null)
                {
                    region.Add(city);
                }
            }
        }

        public void DbLoaderAdd(ICity city)
        {
            lock (cities)
            {
                cityIdGen.Set((int)city.Id);

                if (city.Deleted != City.DeletedState.Deleted)
                {
                    cities.Add(city.Id, city);
                }
            }
        }

        public bool FindCityId(string name, out uint cityId)
        {
            cityId = UInt16.MaxValue;
            using (
                    DbDataReader reader =
                            DbPersistance.Current.ReaderQuery(
                                                              String.Format(
                                                                            "SELECT `id` FROM `{0}` WHERE name = @name LIMIT 1",
                                                                            City.DB_TABLE),
                                                              new[] {new DbColumn("name", name, DbType.String)}))
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
    }
}