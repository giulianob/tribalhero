namespace Game.Comm.Protocol
{
    public interface IProtocolFactory
    {
        IProtocol CreateProtocol(Session session);
    }
}