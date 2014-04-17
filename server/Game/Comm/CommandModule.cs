using Game.Setup;

namespace Game.Comm
{
    public abstract class CommandModule
    {
        public abstract void RegisterCommands(IProcessor processor);

        protected void ReplySuccess(Session session, Packet packet)
        {
            var reply = new Packet(packet) {Option = (ushort)Packet.Options.Reply};
            session.Write(reply);
        }

        protected Packet ReplyError(Session session, Packet packet, Error error)
        {
            return ReplyError(session, packet, error, true);
        }

        protected Packet ReplyError(Session session, Packet packet, Error error, bool sendPacket)
        {
            var reply = new Packet(packet) {Option = (ushort)Packet.Options.Failed | (ushort)Packet.Options.Reply};
            reply.AddInt32((int)error);

            if (sendPacket)
            {
                session.Write(reply);
            }

            return reply;
        }

        protected void ReplyWithResult(Session session, Packet packet, Error error)
        {
            if (error == Error.Ok)
            {
                ReplySuccess(session, packet);
            }
            else
            {
                ReplyError(session, packet, error);
            }
        }
    }
}