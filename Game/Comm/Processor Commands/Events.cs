#region

using Game.Data;
using Game.Setup;
using Game.Util;
using Ninject;
using Persistance;

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

            using (Ioc.Kernel.Get<MultiObjectLock>().Lock(session.Player))
            {
                Global.Channel.Unsubscribe(session);

                // If player is logged in under new session already, then don't bother changing their session info
                if (session.Player.Session != session)
                    return;

                session.Player.Session = null;
                session.Player.SessionId = string.Empty;
                Ioc.Kernel.Get<IDbManager>().Save(session.Player);
            }
        }
    }
}