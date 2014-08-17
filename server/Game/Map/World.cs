#region

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using Game.Battle;
using Game.Data;
using Game.Data.BarbarianTribe;
using Game.Data.Forest;
using Game.Data.Stronghold;
using Game.Data.Tribe;
using Game.Data.Troop;
using Game.Logic.Procedures;
using Game.Module.Remover;
using Game.Setup;
using Persistance;

#endregion

namespace Game.Map
{
    public class World : IWorld
    {
        private readonly IBarbarianTribeManager barbarianTribeManager;

        private readonly IDbManager dbManager;

        private readonly IStrongholdManager strongholds;

        #region Singleton

        [Obsolete("Inject IWorld instead")]
        public static IWorld Current { get; set; }

        #endregion

        public World(IRoadManager roadManager,
                     IStrongholdManager strongholdManager,
                     ICityManager cityManager,
                     IRegionManager regionManager,
                     ITribeManager tribeManager,
                     IBarbarianTribeManager barbarianTribeManager,
                     IDbManager dbManager)
        {
            this.barbarianTribeManager = barbarianTribeManager;
            this.dbManager = dbManager;
            Roads = roadManager;
            strongholds = strongholdManager;
            Cities = cityManager;
            Regions = regionManager;
            Tribes = tribeManager;
            Battles = new Dictionary<uint, IBattleManager>();
            Lock = new object();
            Players = new ConcurrentDictionary<uint, IPlayer>();
        }

        #region Object Locator

        public bool TryGetObjects(uint strongholdId, out IStronghold stronghold)
        {
            return strongholds.TryGetStronghold(strongholdId, out stronghold);
        }

        public bool TryGetObjects(uint barbarianTribeId, out IBarbarianTribe barbarianTribe)
        {
            return barbarianTribeManager.TryGetBarbarianTribe(barbarianTribeId, out barbarianTribe);
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

        #endregion        

        private ITribeManager Tribes { get; set; }

        private Dictionary<uint, IBattleManager> Battles { get; set; }

        public ICityManager Cities { get; private set; }

        public IRegionManager Regions { get; private set; }

        public IRoadManager Roads { get; private set; }

        public object Lock { get; private set; }

        public ConcurrentDictionary<uint, IPlayer> Players { get; private set; }

        public int GetActivePlayerCount()
        {
            return new ActivePlayerSelector().GetPlayerIds().Count();
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

        public void AfterDbLoaded(Procedure procedure, IForestManager forestManager)
        {
            Cities.AfterDbLoaded(procedure);

            // Launch forest creator
            forestManager.StartForestCreator();
        }

        public bool FindPlayerId(string name, out uint playerId)
        {
            playerId = UInt16.MaxValue;
            using (var reader = dbManager.ReaderQuery(String.Format("SELECT `id` FROM `{0}` WHERE name = @name LIMIT 1", Player.DB_TABLE),
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
            using (var reader = dbManager.ReaderQuery(String.Format("SELECT `id` FROM `{0}` WHERE name = @name LIMIT 1", Stronghold.DB_TABLE),
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
            using (var reader = dbManager.ReaderQuery(String.Format("SELECT `id` FROM `{0}` WHERE name = @name LIMIT 1", City.DB_TABLE),
                                                      new[] {new DbColumn("name", name, DbType.String)}))
            {
                return reader.HasRows;
            }
        }
    }
}