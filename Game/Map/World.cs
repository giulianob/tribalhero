#region

using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;
using Game.Battle;
using Game.Data;
using Game.Data.Stronghold;
using Game.Data.Tribe;
using Game.Data.Troop;
using Game.Database;
using Game.Module.Remover;
using Game.Setup;
using Game.Util;
using Persistance;

#endregion

namespace Game.Map
{
    public class World : IWorld
    {
        #region Singleton
        
        public static IWorld Current { get; set; }
        
        #endregion

        private readonly LargeIdGenerator tribeIdGen = new LargeIdGenerator(100000, 200000);

        public ICityManager Cities { get; private set; }

        public IRegionManager Regions { get; private set; }

        public RoadManager Roads { get; private set; }

        public ForestManager Forests { get; private set; }

        public IStrongholdManager Strongholds { get; private set; }        

        public object Lock { get; private set; }

        public Dictionary<uint, IPlayer> Players { get; private set; }        

        private Dictionary<uint, ITribe> Tribes { get; set; }

        private Dictionary<uint, IBattleManager> Battles { get; set; }
        
        public int TribeCount
        {
            get
            {
                return Tribes.Count;
            }
        }

        public int GetActivePlayerCount()
        {
            return new ActivePlayerSelector(Config.idle_days).GetPlayerIds().Count();
        }

        public World(RoadManager roadManager, ForestManager forestManager, IStrongholdManager strongholdManager, ICityManager cityManager, IRegionManager regionManager)
        {
            Roads = roadManager;
            Forests = forestManager;
            Strongholds = strongholdManager;
            Cities = cityManager;
            Regions = regionManager;
            Battles = new Dictionary<uint, IBattleManager>();              
            Lock = new object();
            Players = new Dictionary<uint, IPlayer>();            
            Tribes = new Dictionary<uint, ITribe>();
        }

        #region Object Locator

        public List<ISimpleGameObject> GetObjects(uint x, uint y)
        {
            return Regions.GetObjects(x, y);
        }

        public List<ISimpleGameObject> GetObjectsWithin(uint x, uint y, int radius)
        {
            return Regions.GetObjectsWithin(x, y, radius);
        }

        public bool TryGetObjects(uint strongholdId, out IStronghold stronghold)
        {
            return Strongholds.TryGetStronghold(strongholdId, out stronghold);
        }

        public bool TryGetObjects(uint cityId, out ICity city)
        {
            return Cities.TryGetCity(cityId, out city);
        }

        public bool TryGetObjects(uint playerId, out IPlayer player)
        {
            return Players.TryGetValue(playerId, out player);
        }

        public bool TryGetObjects(uint tribeId, out ITribe tribe)
        {
            return Tribes.TryGetValue(tribeId, out tribe);
        }

        public bool TryGetObjects(uint battleId, out IBattleManager battleManager)
        {
            return Battles.TryGetValue(battleId, out battleManager);
        }

        public bool TryGetObjects(uint cityId, byte troopStubId, out ICity city, out ITroopStub troopStub)
        {
            troopStub = null;

            return Cities.TryGetCity(cityId, out city) && city.Troops.TryGetStub(troopStubId, out troopStub);
        }

        public bool TryGetObjects(uint cityId, uint structureId, out ICity city, out IStructure structure)
        {
            structure = null;

            return Cities.TryGetCity(cityId, out city) && city.TryGetStructure(structureId, out structure);
        }

        public bool TryGetObjects(uint cityId, uint troopObjectId, out ICity city, out ITroopObject troopObject)
        {
            troopObject = null;

            return Cities.TryGetCity(cityId, out city) && city.TryGetTroop(troopObjectId, out troopObject);
        }

        public bool TryGetObjects(uint cityId, out ICity city, out ITribe tribe)
        {
            tribe = null;
            if (Cities.TryGetCity(cityId, out city))
            {
                if (city.Owner.IsInTribe)
                {
                    tribe = city.Owner.Tribesman.Tribe;
                    return true;
                }
            }
            return false;
        }

        public List<ISimpleGameObject> this[uint x, uint y]
        {
            get
            {
                return Regions[x, y];
            }
        }

        #endregion

        public void Add(ITribe tribe)
        {
            lock (Lock)
            {
                tribe.Id = (uint)tribeIdGen.GetNext();
                Tribes.Add(tribe.Id, tribe);
                DbPersistance.Current.Save(tribe);
            }
        }

        public void DbLoaderAdd(ITribe tribe)
        {
            lock (Lock)
            {
                tribeIdGen.Set(tribe.Id);
                Tribes.Add(tribe.Id, tribe);
            }
        }

        public void Add(IBattleManager battleManager)
        {
            lock (Lock)
            {
                Battles.Add(battleManager.BattleId, battleManager);
            }
        }

        public void Remove(IBattleManager battleManager)
        {
            lock (Lock)
            {
                Battles.Remove(battleManager.BattleId);
            }
        }

        public void DbLoaderAdd(IBattleManager battleManager)
        {
            lock (Lock)
            {
                Battles.Add(battleManager.BattleId, battleManager);
            }
        }

        public void AfterDbLoaded()
        {
            Cities.AfterDbLoaded();

            // Launch forest creator
            Forests.StartForestCreator();
        }

        public void Remove(ITribe tribe)
        {
            lock (Lock)
            {
                Tribes.Remove(tribe.Id);
                DbPersistance.Current.Delete(tribe);
            }
        }
       
        public bool FindPlayerId(string name, out uint playerId)
        {
            playerId = UInt16.MaxValue;
            using (
                    DbDataReader reader = DbPersistance.Current.ReaderQuery(
                                                                            String.Format("SELECT `id` FROM `{0}` WHERE name = @name LIMIT 1", Player.DB_TABLE),
                                                                            new[]
                                                                            {
                                                                                    new DbColumn("name", name, DbType.String)
                                                                            }))
            {
                if (!reader.HasRows)
                {
                    return false;
                }
                reader.Read();
                playerId = (uint)reader[0];
                return true;
            }
        }

        public bool FindTribeId(string name, out uint tribeId)
        {
            tribeId = UInt16.MaxValue;
            using (
                    DbDataReader reader = DbPersistance.Current.ReaderQuery(String.Format("SELECT `id` FROM `{0}` WHERE name = @name LIMIT 1", Tribe.DB_TABLE),
                                                                            new[]
                                                                            {
                                                                                    new DbColumn("name", name, DbType.String)
                                                                            }))
            {
                if (!reader.HasRows)
                {
                    return false;
                }
                reader.Read();
                tribeId = (uint)reader[0];
                return true;
            }
        }

        public bool CityNameTaken(string name)
        {
            using (
                    DbDataReader reader = DbPersistance.Current.ReaderQuery(String.Format("SELECT `id` FROM `{0}` WHERE name = @name LIMIT 1", City.DB_TABLE),
                                                                            new[]
                                                                            {
                                                                                    new DbColumn("name", name, DbType.String)
                                                                            }))
            {
                return reader.HasRows;
            }
        }

        public bool TribeNameTaken(string name)
        {
            using (
                    DbDataReader reader = DbPersistance.Current.ReaderQuery(String.Format("SELECT `id` FROM `{0}` WHERE name = @name LIMIT 1", Tribe.DB_TABLE),
                                                                            new[]
                                                                            {
                                                                                    new DbColumn("name", name, DbType.String)
                                                                            }))
            {
                return reader.HasRows;
            }
        }
    }
}