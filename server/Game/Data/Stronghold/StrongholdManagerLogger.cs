using System.Data;
using Game.Data.Tribe;
using Newtonsoft.Json;
using Persistance;

namespace Game.Data.Stronghold
{
    class StrongholdManagerLogger: IStrongholdManagerLogger
    {
        private const string TRIBE_LOG_DB = "tribe_logs";
        private readonly IDbManager dbManager;

        public StrongholdManagerLogger(IDbManager dbManager)
        {
            this.dbManager = dbManager;
        }

        public void Listen(IStrongholdManager strongholdManager)
        {
            strongholdManager.StrongholdGained += StrongholdManager_StrongholdGained;
            strongholdManager.StrongholdLost += StrongholdManager_StrongholdLost;
        }

        void StrongholdManager_StrongholdLost(object sender, Tribe.EventArguments.StrongholdLostEventArgs e)
        {
            Save(e.Tribe, TribeLogType.StrongholdLost, e.Stronghold.Name, e.AttackedBy.Name);
        }

        void StrongholdManager_StrongholdGained(object sender, Tribe.EventArguments.StrongholdGainedEventArgs e)
        {
            if (e.OwnBy == null)
            {
                Save(e.Tribe, TribeLogType.StrongholdOccupied, e.Stronghold.Name);
            }
            else
            {
                Save(e.Tribe, TribeLogType.StrongholdGained, e.Stronghold.Name, e.OwnBy.Name);
            }
        }

        private void Save(ITribe tribe, TribeLogType type, params object[] objs)
        {
            dbManager.Query(string.Format(@"INSERT INTO `{0}` VALUES (NULL,@tribe_id,UTC_TIMESTAMP(),@type,@parameters)", TRIBE_LOG_DB),
                            new[]
                            {
                                    new DbColumn("tribe_id", tribe.Id, DbType.UInt32),
                                    new DbColumn("type", type, DbType.Int32),
                                    new DbColumn("parameters", JsonConvert.SerializeObject(objs), DbType.String),
                            });
        }

    }
}
