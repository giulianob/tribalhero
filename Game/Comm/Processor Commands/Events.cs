#region

using Game.Data;
using Game.Util;

#endregion

namespace Game.Comm
{
    public partial class Processor
    {
        public void EventOnConnect(Session session, Packet packet)
        {
        }

        public void EventOnDisconnect(Session session, Packet packet)
        {
            if (session == null || session.Player == null)
                return;

            using (new MultiObjectLock(session.Player))
            {
                Global.Channel.Unsubscribe(session);

                if (session.Player.Session == session)
                    session.Player.Session = null;

                session.Player.SessionId = string.Empty;

                Global.DbManager.Save(session.Player);
            }
        }
    }
}