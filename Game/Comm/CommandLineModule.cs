using Game.Setup;

namespace Game.Comm
{
    public abstract class CommandLineModule
    {
        public abstract void RegisterCommands(CommandLineProcessor processor);

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
    }
}