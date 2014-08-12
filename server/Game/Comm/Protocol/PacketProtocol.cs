using Game.Data;

namespace Game.Comm.Protocol
{
    class PacketProtocol : IProtocol
    {
        private readonly Session session;

        public PacketProtocol(Session session)
        {
            this.session = session;
        }

        public void MessageSendUnreadCount(int unreadCount)
        {
            var packet = new Packet(Command.MessageUnread);
            packet.AddInt32(unreadCount);

            Global.Current.Channel.Post(session.Player.PlayerChannel, packet);
        }

        public void BattleReportSendUnreadCount(int unreadCount)
        {
            var packet = new Packet(Command.BattleReportUnread);
            packet.AddInt32(unreadCount);

            Global.Current.Channel.Post(session.Player.PlayerChannel, packet);
        }

        public void MessageBoardSendUnread()
        {
            var packet = new Packet(Command.ForumUnread);
            Global.Current.Channel.Post(session.Player.PlayerChannel, packet);
        }
    }
}