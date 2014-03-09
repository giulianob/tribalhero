#region

using Game.Data;
using Game.Util;
using Game.Util.Locking;
using Persistance;

#endregion

namespace Game.Comm.ProcessorCommands
{
    class EventCommandsModule : CommandModule
    {
        private readonly IDbManager dbManager;

        private readonly ILocker locker;

        private readonly IChannel channel;

        public EventCommandsModule(IDbManager dbManager, ILocker locker, IChannel channel)
        {
            this.dbManager = dbManager;
            this.locker = locker;
            this.channel = channel;
        }

        public override void RegisterCommands(IProcessor processor)
        {
            processor.RegisterEvent(Command.OnDisconnect, EventOnDisconnect);
            processor.RegisterEvent(Command.OnConnect, EventOnConnect);
        }

        private void EventOnConnect(Session session, Packet packet)
        {
        }

        private void EventOnDisconnect(Session session, Packet packet)
        {
            if (session == null || session.Player == null)
            {
                return;
            }

            locker.Lock(session.Player).Do(() =>
            {
                channel.Unsubscribe(session);

                // If player is logged in under new session already, then don't bother changing their session info
                if (session.Player.Session != session)
                {
                    return;
                }

                session.Player.Session = null;
                session.Player.SessionId = string.Empty;
                session.Player.LastLogin = SystemClock.Now;
                dbManager.Save(session.Player);
            });
        }
    }
}