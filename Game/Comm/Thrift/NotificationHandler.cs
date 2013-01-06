using System.Collections.Generic;
using System.Linq;
using Game.Comm.Protocol;
using Game.Data;
using Game.Data.Tribe;
using Game.Map;
using Game.Util.Locking;

namespace Game.Comm.Thrift
{
    class NotificationHandler : Notification.Iface
    {
        private readonly IProtocolFactory protocolFactory;

        public NotificationHandler(IProtocolFactory protocolFactory)
        {
            this.protocolFactory = protocolFactory;
        }

        public void NewMessage(PlayerUnreadCount playerUnreadCount)
        {
            IPlayer player;
            using (Concurrency.Current.Lock((uint)playerUnreadCount.Id, out player))
            {
                if (player.Session == null)
                {
                    return;
                }

                try
                {
                    protocolFactory.CreateProtocol(player.Session).MessageSendUnreadCount(playerUnreadCount.UnreadCount);
                }
                catch
                {
                }
            }
        }

        public void NewTribeForumPost(int tribeId, int playerId)
        {
            ITribe tribe;
            if (!World.Current.TryGetObjects((uint)tribeId, out tribe))
            {
                return;
            }

            using (Concurrency.Current.Lock(custom => tribe.Tribesmen.ToArray(), new object[] {}, tribe))
            {
                foreach (
                        var tribesman in
                                tribe.Tribesmen.Where(
                                                      tribesman =>
                                                      tribesman.Player.Session != null &&
                                                      tribesman.Player.PlayerId != playerId))
                {
                    try
                    {
                        protocolFactory.CreateProtocol(tribesman.Player.Session).MessageBoardSendUnread();
                    }
                    catch
                    {
                    }
                }
            }
        }

        public void NewBattleReport(List<PlayerUnreadCount> playerUnreadCounts)
        {
            foreach (var playerUnreadCount in playerUnreadCounts)
            {
                IPlayer player;
                using (Concurrency.Current.Lock((uint)playerUnreadCount.Id, out player))
                {
                    if (player == null || player.Session == null)
                    {
                        continue;
                    }

                    try
                    {
                        protocolFactory.CreateProtocol(player.Session)
                                       .BattleReportSendUnreadCount(playerUnreadCount.UnreadCount);
                    }
                    catch
                    {
                    }
                }
            }
        }
    }
}