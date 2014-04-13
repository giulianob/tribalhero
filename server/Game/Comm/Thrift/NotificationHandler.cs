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

        private readonly ILocker locker;

        private IWorld world;

        public NotificationHandler(IProtocolFactory protocolFactory, ILocker locker, IWorld world)
        {
            this.protocolFactory = protocolFactory;
            this.locker = locker;
            this.world = world;
        }

        public void NewMessage(PlayerUnreadCount playerUnreadCount)
        {
            IPlayer player;
            locker.Lock((uint)playerUnreadCount.Id, out player).Do(() =>
            {
                if (player.Session == null)
                {
                    return;
                }

                try
                {
                    protocolFactory.CreateProtocol(player.Session).MessageSendUnreadCount(playerUnreadCount.UnreadCount);
                }
                catch { }
            });
        }

        public void NewTribeForumPost(int tribeId, int playerId)
        {
            ITribe tribe;
            if (!world.TryGetObjects((uint)tribeId, out tribe))
            {
                return;
            }

            locker.Lock(custom => tribe.Tribesmen.ToArray<ILockable>(), new object[] {}, tribe).Do(() =>
            {
                foreach (var tribesman in tribe.Tribesmen.Where(tribesman =>
                                                                tribesman.Player.Session != null &&
                                                                tribesman.Player.PlayerId != playerId))
                {
                    try
                    {
                        protocolFactory.CreateProtocol(tribesman.Player.Session).MessageBoardSendUnread();
                    }
                    catch { }
                }
            });
        }

        public void NewBattleReport(List<PlayerUnreadCount> playerUnreadCounts)
        {
            foreach (var playerUnreadCount in playerUnreadCounts)
            {
                IPlayer player;
                locker.Lock((uint)playerUnreadCount.Id, out player).Do(() =>
                {
                    if (player == null || player.Session == null)
                    {
                        return;
                    }

                    try
                    {
                        protocolFactory.CreateProtocol(player.Session)
                                       .BattleReportSendUnreadCount(playerUnreadCount.UnreadCount);
                    }
                    catch { }
                });
            }
        }
    }
}