using System;
using System.Collections.Generic;
using Game.Comm.Protocol;
using Game.Data;
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
            Player player;
            using (Concurrency.Current.Lock((uint)playerUnreadCount.Id, out player))
            {
                if (player.Session == null)
                    return;

                try
                {
                    protocolFactory.CreateProtocol(player.Session).MessageSendUnreadCount(playerUnreadCount.UnreadCount);
                }
                catch
                {
                }
            }
        }

        public void NewTribeForumPost(int tribeId, int unreadCount)
        {
            throw new NotImplementedException();
        }

        public void NewBattleReport(List<PlayerUnreadCount> playerUnreadCounts)
        {
            foreach (var playerUnreadCount in playerUnreadCounts)
            {
                Player player;
                using (Concurrency.Current.Lock((uint)playerUnreadCount.Id, out player))
                {
                    if (player.Session == null)
                        continue;

                    try
                    {
                        protocolFactory.CreateProtocol(player.Session).BattleReportSendUnreadCount(playerUnreadCount.UnreadCount);
                    }
                    catch
                    {
                    }
                }
            }
        }
    }
}
