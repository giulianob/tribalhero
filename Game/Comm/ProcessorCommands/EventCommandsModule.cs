#region

using Game.Data;
using Game.Database;
using Game.Setup;
using Game.Util.Locking;
using Ninject;
using Persistance;

#endregion

namespace Game.Comm.ProcessorCommands
{
    class EventCommandsModule : CommandModule
    {
        public override void RegisterCommands(Processor processor)
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
                return;

            using (Concurrency.Current.Lock(session.Player))
            {
                Global.Channel.Unsubscribe(session);

                // If player is logged in under new session already, then don't bother changing their session info
                if (session.Player.Session != session)
                    return;

                session.Player.Session = null;
                session.Player.SessionId = string.Empty;
                DbPersistance.Current.Save(session.Player);
            }
        }
    }
}