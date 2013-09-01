using System;
using System.Data;
using Game.Data.Tribe.EventArguments;
using Newtonsoft.Json;
using Persistance;

namespace Game.Data.Tribe
{
    class TribeLogger : ITribeLogger
    {
        private const string TRIBE_LOG_DB = "tribe_logs";
        private readonly IDbManager dbManager;

        public TribeLogger(IDbManager dbManager)
        {
            this.dbManager = dbManager;

        }

        public void Listen(ITribe tribe)
        {
            tribe.Upgraded += Tribe_Upgraded;
            tribe.TribesmanContributed += Tribe_TribesmanContributed;
            tribe.TribesmanJoined += Tribe_TribesmanJoined;
            tribe.TribesmanKicked += Tribe_TribesmanKicked;
            tribe.TribesmanLeft += Tribe_TribesmanLeft;
            tribe.TribesmanRankChanged += Tribe_TribesmanRankChanged;
        }

        public void Unlisten(ITribe tribe)
        {
            tribe.Upgraded -= Tribe_Upgraded;
            tribe.TribesmanContributed -= Tribe_TribesmanContributed;
            tribe.TribesmanJoined -= Tribe_TribesmanJoined;
            tribe.TribesmanKicked -= Tribe_TribesmanKicked;
            tribe.TribesmanLeft -= Tribe_TribesmanLeft;
            tribe.TribesmanRankChanged -= Tribe_TribesmanRankChanged;
        }

        private void Tribe_Upgraded(object sender, TribeEventArgs e)
        {
            var tribe = e.Tribe;
            Save(tribe, TribeLogType.TribeUpgraded, tribe.Level);
        }

        private void Tribe_TribesmanContributed(object sender, TribesmanContributedEventArgs e)
        {
            var tribe = e.Tribe;
            Save(tribe, TribeLogType.TribeContribured, e.Player.Name, e.Resource.Crop, e.Resource.Gold, e.Resource.Iron, e.Resource.Wood);
        }

        private void Tribe_TribesmanJoined(object sender, TribesmanEventArgs e)
        {
            var tribe = e.Tribe;
            Save(tribe, TribeLogType.TribeJoined, e.Player.Name);
        }

        private void Tribe_TribesmanKicked(object sender, TribesmanKickedEventArgs e)
        {
            var tribe = e.Tribe;
            Save(tribe, TribeLogType.TribeKicked, e.Kickee.Name, e.Kicker.Name);
        }

        private void Tribe_TribesmanLeft(object sender, TribesmanEventArgs e)
        {
            var tribe = e.Tribe;
            Save(tribe, TribeLogType.TribeLeft, e.Player.Name);
        }

        private void Tribe_TribesmanRankChanged(object sender, TribesmanEventArgs e)
        {
            var tribe = e.Tribe;
            Save(tribe, TribeLogType.TribeRankChanged, e.Player.Name, e.Player.Tribesman.Rank.Name);
        }

        void Save(ITribe tribe, TribeLogType type, params object[] objs)
        {            
            dbManager.Query(string.Format(@"INSERT INTO `{0}` VALUES (NULL,@tribe_id,UTC_TIMESTAMP(),@type,@parameters)", TRIBE_LOG_DB),
                            new[]
                            {
                                    new DbColumn("tribe_id", tribe.Id, DbType.UInt32), new DbColumn("type", type, DbType.Int32),
                                    new DbColumn("parameters", JsonConvert.SerializeObject(objs), DbType.String),
                            });
        }
    }
}
