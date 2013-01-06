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
using Game.Logic.Procedures;
using Game.Module.Remover;
using Game.Setup;
using Persistance;

#endregion

namespace Game.Map
{
    public class World : IWorld
    {
        #region Singleton

        public static IWorld Current { get; set; }

        #endregion

        public World(RoadManager roadManager,
                     ForestManager forestManager,
                     IStrongholdManager strongholdManager,
                     ICityManager cityManager,
                     IRegionManager regionManager,
                     ITribeManager tribeManager)
        {
            Roads = roadManager;
            Forests = forestManager;
            Strongholds = strongholdManager;
            Cities = cityManager;
            Regions = regionManager;
            Tribes = tribeManager;
            Battles = new Dictionary<uint, IBattleManager>();
            Lock = new object();
            Players = new Dictionary<uint, IPlayer>();
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
            return Tribes.TryGetTribe(tribeId, out tribe);
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

        private IStrongholdManager Strongholds { get; set; }

        private ITribeManager Tribes { get; set; }

        private Dictionary<uint, IBattleManager> Battles { get; set; }

        public ICityManager Cities { get; private set; }

        public IRegionManager Regions { get; private set; }

        public RoadManager Roads { get; private set; }

        public ForestManager Forests { get; private set; }

        public object Lock { get; private set; }

        public Dictionary<uint, IPlayer> Players { get; private set; }

        public int GetActivePlayerCount()
        {
            return new ActivePlayerSelector(Config.idle_days).GetPlayerIds().Count();
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

        public void AfterDbLoaded(Procedure procedure)
        {
            Cities.AfterDbLoaded(procedure);

            // Launch forest creator
            Forests.StartForestCreator();
        }

        public bool FindPlayerId(string name, out uint playerId)
        {
            playerId = UInt16.MaxValue;
            using (
                    DbDataReader reader =
                            DbPersistance.Current.ReaderQuery(
                                                              String.Format(
                                                                            "SELECT `id` FROM `{0}` WHERE name = @name LIMIT 1",
                                                                            Player.DB_TABLE),
                                                              new[] {new DbColumn("name", name, DbType.String)}))
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

        public bool FindStrongholdId(string name, out uint strongholdId)
        {
            strongholdId = UInt16.MaxValue;
            using (
                    DbDataReader reader =
                            DbPersistance.Current.ReaderQuery(
                                                              String.Format(
                                                                            "SELECT `id` FROM `{0}` WHERE name = @name LIMIT 1",
                                                                            Stronghold.DB_TABLE),
                                                              new[] {new DbColumn("name", name, DbType.String)}))
            {
                if (!reader.HasRows)
                {
                    return false;
                }
                reader.Read();
                strongholdId = (uint)reader[0];
                return true;
            }
        }

        public bool CityNameTaken(string name)
        {
            using (
                    DbDataReader reader =
                            DbPersistance.Current.ReaderQuery(
                                                              String.Format(
                                                                            "SELECT `id` FROM `{0}` WHERE name = @name LIMIT 1",
                                                                            City.DB_TABLE),
                                                              new[] {new DbColumn("name", name, DbType.String)}))
            {
                return reader.HasRows;
            }
        }
    }
}