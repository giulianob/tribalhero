namespace Game.Comm.Protocol
{
    public class ProtocolFactory : IProtocolFactory
    {
        public IProtocol CreateProtocol(Session session)
        {
            return new PacketProtocol(session);
        }
    }
}