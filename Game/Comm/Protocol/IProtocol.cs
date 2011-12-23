namespace Game.Comm.Protocol
{
    public interface IProtocol
    {
        /// <summary>
        /// Sends the number of unread messages to the client
        /// </summary>
        /// <param name="unreadCount"></param>
        void MessageSendUnreadCount(int unreadCount);

        /// <summary>
        /// Sends the number of unread battle reports to the client
        /// </summary>
        /// <param name="unreadCount"></param>
        void BattleReportSendUnreadCount(int unreadCount);
    }
}